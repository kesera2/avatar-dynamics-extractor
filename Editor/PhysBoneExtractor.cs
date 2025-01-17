using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace dev.kesera2.physbone_extractor
{
    public class PhysBoneExtractor : EditorWindow
    {
        private GameObject prefabRoot; // 元のGameObject
        private GameObject searchRoot; // 探索するGameObject
        private bool _isKeepPbVersion = false;
        private bool isDeleteEnabled = true;
        private static int _selectedLanguage = 0;
        private GameObject _avatarDynamics;
        private Transform _avatarArmature;
        private string avatarDynamicsGameObjectName;
        private string pbGameObjectName;
        private string pbColliderGameObjectName;
        private string contactsGameObjectName;
        private bool splitContacts;
        private string contactSenderGameObjectName;
        private string contactReceiverGameObjectName;
        private bool isSearchRootSet;
        private bool prefabRootChanged;

        [MenuItem("Tools/kesera2/PhysBone Extractor")]
        public static void ShowWindow()
        {
            var window = GetWindow<PhysBoneExtractor>(Settings.WindowTitle);
            window.minSize = Settings.windowMinSize;
        }

        private void OnEnable()
        {
            Init();
        }

        private void Init()
        {
            Localization.LoadLocalization(_selectedLanguage);
            avatarDynamicsGameObjectName ??= Settings.AvatarDynamicsGameObjectName;
            pbGameObjectName ??= Settings.PhysboneGameObjectName;
            pbColliderGameObjectName ??= Settings.PhysboneColliderGameObjectName;
            contactsGameObjectName ??= Settings.ContactsGameObjectName;
            contactSenderGameObjectName ??= Settings.ContactSenderGameObjectName;
            contactReceiverGameObjectName ??= Settings.ContactReceiverGameObjectName;
        }

        private void DrawSelectLanguage()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                var selectedLanguage =
                    EditorGUILayout.Popup(_selectedLanguage, Localization.DisplayNames,
                        Settings.LanguageGuiLayoutOptions);
                if (_selectedLanguage != selectedLanguage)
                    Localization.LoadLocalization(Localization.SupportedLanguages[selectedLanguage]);
                _selectedLanguage = selectedLanguage;
            }
        }

        private void OnGUI()
        {
            DrawSelectLanguage();
            DrawLogo();
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(Localization.S("label.search.root"), Settings.LabelGuiLayoutOptions);
                    if (prefabRoot)
                    {
                        EditorGUILayout.LabelField(Localization.S("label.prefab.root"), Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField("AvatarDynamicsの名前", Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField("PhysBoneの名前", Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField("PhysBoneColliderの名前", Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField("Contactsの名前", Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField("Contactsを分類分けする", Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField("ContactSenderの名前", Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField("ContactReceiverの名前", Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("option.remove.original"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("option.keep.pb.version"),
                            Settings.LabelGuiLayoutOptions);
                    }
                }

                using (new GUILayout.VerticalScope())
                {
                    DrawAvatarRootField();
                    if (prefabRoot)
                    {
                        DrawSearchRootField();
                        avatarDynamicsGameObjectName = EditorGUILayout.TextField(avatarDynamicsGameObjectName);
                        pbGameObjectName = EditorGUILayout.TextField(pbGameObjectName);
                        pbColliderGameObjectName = EditorGUILayout.TextField(pbColliderGameObjectName);
                        contactsGameObjectName = EditorGUILayout.TextField(contactsGameObjectName);
                        splitContacts = EditorGUILayout.Toggle(splitContacts);
                        using (new EditorGUI.DisabledScope(!splitContacts))
                        {
                            contactSenderGameObjectName = EditorGUILayout.TextField(contactSenderGameObjectName);
                            contactReceiverGameObjectName = EditorGUILayout.TextField(contactReceiverGameObjectName);
                        }

                        isDeleteEnabled = EditorGUILayout.Toggle(isDeleteEnabled);
                        _isKeepPbVersion = EditorGUILayout.Toggle(_isKeepPbVersion);
                    }
                }
            }

            if (!ValidatePrefabRoot()) return;
            using (new EditorGUI.DisabledScope(!CanExecute()))
            {
                if (GUILayout.Button(Localization.S("button.extract")))
                {
                    if (!DisplayConfirmDialog())
                    {
                        return;
                    }

                    if (prefabRoot != null && searchRoot != null)
                    {
                        CreateAvatarDynamics();
                        var hasPhysbone =
                            CopyComponent<VRCPhysBone>(_avatarDynamics, pbGameObjectName, CopyVRCPhysBone);
                        var hasPhysboneCollider = CopyComponent<VRCPhysBoneCollider>(_avatarDynamics,
                            pbColliderGameObjectName, CopyVRCPhysboneCollider);
                        var hasContacts = CopyVRCContacts();
                        if (!(hasPhysbone || hasPhysboneCollider || hasContacts))
                            // Destroy the GameObject of AvatarDynamics if there is no target components.
                            DestroyImmediate(_avatarDynamics);
                    }
                    else
                    {
                        Debug.LogWarning("Please assign both Target and Search Root GameObjects.");
                    }
                }
            }

            DisplayInfo();
            DisplayWarnSearchRoot();
        }

        private void DrawSearchRootField()
        {
            _avatarArmature = GetArmatureTransform();
            if ((_avatarArmature && !isSearchRootSet) || prefabRootChanged)
            {
                searchRoot = _avatarArmature.gameObject;
                isSearchRootSet = true;
                prefabRootChanged = false;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                searchRoot =
                    (GameObject)EditorGUILayout.ObjectField(searchRoot, typeof(GameObject), true);
            }
        }

        private void DrawAvatarRootField()
        {
            var avatarRoot = (GameObject)EditorGUILayout.ObjectField(prefabRoot,
                typeof(GameObject), true);
            if (avatarRoot != prefabRoot)
            {
                prefabRootChanged = true;
            }

            prefabRoot = avatarRoot;
        }

        private bool CanExecute()
        {
            return searchRoot;
        }

        private void CreateAvatarDynamics()
        {
            // Create PB GameObject under the target GameObject
            _avatarDynamics = new GameObject(avatarDynamicsGameObjectName);
            _avatarDynamics.transform.SetParent(prefabRoot.transform);
        }

        private bool CopyComponent<T>(GameObject parent, string gameObjectName, System.Action<T, T> copyAction,
            bool useParentGameObject = false) where T : Component
        {
            GameObject componentsParent;
            if (useParentGameObject)
            {
                componentsParent = parent;
            }
            else
            {
                componentsParent = new GameObject(gameObjectName);
                componentsParent.transform.SetParent(parent.transform);
            }

            var components = new List<T>(searchRoot.GetComponentsInChildren<T>());

            foreach (var sourceComponent in components)
            {
                var sourceTransform = sourceComponent.transform;
                // Create new GameObject and copy component
                var newComponent = new GameObject(sourceTransform.name);
                newComponent.transform.SetParent(useParentGameObject ? parent.transform : componentsParent.transform);

                // Copy the component using the provided copy action
                var destComponent = newComponent.AddComponent<T>();
                copyAction(sourceComponent, destComponent);

                // Set the Root Transform if applicable
                if (destComponent is VRCPhysBone destPhysBone && sourceComponent is VRCPhysBone sourcePhysBone)
                {
                    if (!destPhysBone.rootTransform) destPhysBone.rootTransform = sourcePhysBone.transform;
                }
                else if (destComponent is VRCPhysBoneCollider destCollider &&
                         sourceComponent is VRCPhysBoneCollider sourceCollider)
                {
                    if (!destCollider.rootTransform) destCollider.rootTransform = sourceCollider.transform;
                }
                else if (destComponent is VRCContactSender destSender &&
                         sourceComponent is VRCContactSender sourceSender)
                {
                    if (!destSender.rootTransform) destSender.rootTransform = sourceSender.transform;
                }
                else if (destComponent is VRCContactReceiver destReceiver &&
                         sourceComponent is VRCContactReceiver sourceReceiver)
                {
                    if (!destReceiver.rootTransform) destReceiver.rootTransform = sourceReceiver.transform;
                }

                // Remove original component
                if (isDeleteEnabled) DestroyImmediate(sourceComponent);
            }

            if (components.Count == 0)
            {
                DestroyImmediate(componentsParent);
            }

            return components.Count > 0;
        }

        private bool CopyVRCContacts()
        {
            var contactsParent = new GameObject(contactsGameObjectName);
            contactsParent.transform.SetParent(_avatarDynamics.transform);

            var hasContactSender = CopyComponent<VRCContactSender>(contactsParent, contactSenderGameObjectName,
                CopyVRCContactSender, !splitContacts);
            var hasContactReceiver = CopyComponent<VRCContactReceiver>(contactsParent, contactReceiverGameObjectName,
                CopyVRCContactReceiver, !splitContacts);
            if (!(hasContactSender && hasContactReceiver))
            {
                DestroyImmediate(contactsParent);
            }

            return hasContactSender && hasContactReceiver;
        }


        private void CopyVRCPhysBone(VRCPhysBone source, VRCPhysBone destination)
        {
            // Vesrion
            if (_isKeepPbVersion) destination.version = source.version;

            // Copy properties from source to destination
            destination.name = source.name;
            destination.enabled = source.enabled;

            // Transforms
            destination.rootTransform = source.rootTransform;
            destination.ignoreTransforms = source.ignoreTransforms;
            destination.endpointPosition = source.endpointPosition;
            destination.multiChildType = source.multiChildType;

            // Forces
            destination.integrationType = source.integrationType;
            destination.pull = source.pull;
            destination.pullCurve = source.pullCurve;
            destination.spring = source.spring;
            destination.springCurve = source.springCurve;
            destination.stiffness = source.stiffness;
            destination.stiffnessCurve = source.stiffnessCurve;
            destination.gravity = source.gravity;
            destination.gravityCurve = source.gravityCurve;
            destination.gravityFalloff = source.gravityFalloff;
            destination.gravityFalloffCurve = source.gravityFalloffCurve;
            destination.immobileType = source.immobileType;
            destination.immobile = source.immobile;
            destination.immobileCurve = source.immobileCurve;

            // Limits
            destination.limitType = source.limitType;
            destination.maxAngleX = source.maxAngleX;
            destination.maxAngleZ = source.maxAngleZ;
            destination.maxAngleXCurve = source.maxAngleXCurve;
            destination.maxAngleZCurve = source.maxAngleZCurve;
            destination.staticFreezeAxis = source.staticFreezeAxis;
            destination.limitRotation = source.limitRotation;
            destination.limitOpacity = source.limitOpacity;
            destination.limitRotationXCurve = source.limitRotationXCurve;
            destination.limitRotationYCurve = source.limitRotationYCurve;
            destination.limitRotationZCurve = source.limitRotationZCurve;

            // Colliders
            destination.radius = source.radius;
            destination.radiusCurve = source.radiusCurve;
            destination.allowCollision = source.allowCollision;
            destination.colliders = source.colliders;
            destination.collisionFilter = source.collisionFilter;

            // Stretch & Squish
            destination.maxStretch = source.maxStretch;
            destination.maxStretchCurve = source.maxStretchCurve;
            destination.stretchMotion = source.stretchMotion;
            destination.stretchMotionCurve = source.stretchMotionCurve;
            destination.maxSquish = source.maxSquish;
            destination.maxSquishCurve = source.maxSquishCurve;

            // Grab & Pose
            destination.allowGrabbing = source.allowGrabbing;
            destination.allowPosing = source.allowPosing;
            destination.grabMovement = source.grabMovement;
            destination.snapToHand = source.snapToHand;
            // destination.grabFilter = source.grabFilter;
            // destination.poseFilter = source.poseFilter;

            // Options
            destination.parameter = source.parameter;
            destination.isAnimated = source.isAnimated;
            destination.resetWhenDisabled = source.resetWhenDisabled;

            // Gizmos
            destination.showGizmos = source.showGizmos;
            destination.boneOpacity = source.boneOpacity;
            destination.limitOpacity = source.limitOpacity;
        }

        private void CopyVRCPhysboneCollider(VRCPhysBoneCollider source, VRCPhysBoneCollider destination)
        {
            // Copy properties from source to destination
            destination.name = source.name;
            destination.enabled = source.enabled;
            destination.rootTransform = source.rootTransform;
            destination.shapeType = source.shapeType;
            destination.insideBounds = source.insideBounds;
            destination.radius = source.radius;
            destination.height = source.height;
            destination.position = source.position;
            destination.rotation = source.rotation;
            destination.bonesAsSpheres = source.bonesAsSpheres;
            destination.isGlobalCollider = source.isGlobalCollider;
            destination.playerId = source.playerId;
            destination.shape = source.shape;
        }

        private Transform GetArmatureTransform()
        {
            if (!prefabRoot) return null;
            return prefabRoot.GetComponentsInChildren<Transform>()
                .FirstOrDefault(t => t.name.ToLower() == "armature");
        }

        private void CopyVRCContactSender(VRCContactSender source, VRCContactSender destination)
        {
            destination.name = source.name;
            destination.rootTransform = source.rootTransform;
            destination.shapeType = source.shapeType;
            destination.radius = source.radius;
            destination.position = source.position;
            destination.rotation = source.rotation;
            destination.collisionTags = source.collisionTags;
        }

        private void CopyVRCContactReceiver(VRCContactReceiver source, VRCContactReceiver destination)
        {
            destination.name = source.name;
            destination.rootTransform = source.rootTransform;
            // Shape
            destination.shapeType = source.shapeType;
            destination.radius = source.radius;
            destination.position = source.position;
            destination.rotation = source.rotation;
            // Filtering
            destination.allowSelf = source.allowSelf;
            destination.allowOthers = source.allowOthers;
            destination.localOnly = source.localOnly;
            destination.collisionTags = source.collisionTags;
            // Receiver
            destination.receiverType = source.receiverType;
            destination.parameter = source.parameter;
            destination.minVelocity = source.minVelocity;
        }


        private static void DrawLogo()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (!File.Exists(Settings.GetLogoPath())) return;
                var fileData = File.ReadAllBytes(Settings.GetLogoPath());
                var logo = new Texture2D(2, 2); // サイズは後で自動調整される
                logo.LoadImage(fileData); // PNG, JPG 形式の画像をロード
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(new GUIContent(logo), GUILayout.Height(100), GUILayout.Width(400));
                GUILayout.FlexibleSpace();
            }
        }

        private void DisplayInfo()
        {
            if (searchRoot)
            {
                EditorGUILayout.HelpBox(Localization.S("info.description"), MessageType.Info);
            }
        }

        private bool ValidatePrefabRoot()
        {
            if (!prefabRoot)
            {
                EditorGUILayout.HelpBox(Localization.S("warn.assign.prefab"), MessageType.Warning);
                isSearchRootSet = false;
                return false;
            }

            if (Utility.hasParentGameObject(prefabRoot))
            {
                EditorGUILayout.HelpBox(Localization.S("warn.assign.avatar.root"), MessageType.Warning);
                return false;
            }

            return true;
        }

        private void DisplayWarnSearchRoot()
        {
            if (!searchRoot)
            {
                EditorGUILayout.HelpBox(Localization.S("warn.need.assign.search.gameobject"), MessageType.Warning);
            }
        }

        private bool DisplayConfirmDialog()
        {
            var message = Localization.S("msg.dialog.message") + "\n";
            if (isDeleteEnabled)
            {
                message += "\n" + Localization.S("msg.dialog.remove.original");
            }
            else
            {
                message += "\n" + Localization.S("msg.dialog.keep.original");
            }

            if (_isKeepPbVersion)
            {
                message += "\n" + Localization.S("msg.dialog.keep.pb.version");
            }
            else
            {
                message += "\n" + Localization.S("msg.dialog.update.pb.version");
            }

            // メッセージボックスを表示
            return EditorUtility.DisplayDialog(
                Localization.S("msg.dialog.title"), // ダイアログのタイトル
                message, // メッセージ
                "OK",
                "Cancel"
            );
        }
    }
}