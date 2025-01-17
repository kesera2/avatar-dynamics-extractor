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
        private bool isKeepPBVersion = false;
        private bool isDeleteEnabled = true;
        private static int _selectedLanguage = 0;
        private GameObject _avatarDynamics;
        private Transform _avatarArmature;
        private string avatarDynamicsGameObjectName;
        private string pbGameObjectName;
        private string pbColliderGameObjectName;
        private string contactsGameObjectName;
        private string contactSenderGameObjectName;
        private string contactReceiverGameObjectName;
        private bool isSearchRootSet;

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
                        Settings.LanguagGuiLayoutOptions);
                if (_selectedLanguage != selectedLanguage)
                    Localization.LoadLocalization(Localization.SupportedLanguages[selectedLanguage]);
                _selectedLanguage = selectedLanguage;
            }
        }

        private void OnGUI()
        {
            DrawSelectLanguage();
            DrawLogo();
            DrawAvatarRootField();
            if (!ValidatePrefabRoot())
            {
                return;
            }

            _avatarArmature = GetArmatureTransform();
            if (_avatarArmature && !isSearchRootSet)
            {
                searchRoot = _avatarArmature.gameObject;
                isSearchRootSet = true;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(Localization.S("label.search.root"), Settings.LabelGuiLayoutOptions);
                searchRoot =
                    (GameObject)EditorGUILayout.ObjectField(searchRoot, typeof(GameObject), true);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("AvatarDynamicsの名前", Settings.LabelGuiLayoutOptions);
                avatarDynamicsGameObjectName = EditorGUILayout.TextField(avatarDynamicsGameObjectName);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("PhysBoneの名前", Settings.LabelGuiLayoutOptions);
                pbGameObjectName = EditorGUILayout.TextField(pbGameObjectName);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("PhysBoneColliderの名前", Settings.LabelGuiLayoutOptions);
                pbColliderGameObjectName = EditorGUILayout.TextField(pbColliderGameObjectName);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Contactsの名前", Settings.LabelGuiLayoutOptions);
                contactsGameObjectName = EditorGUILayout.TextField(contactsGameObjectName);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("ContactSenderの名前", Settings.LabelGuiLayoutOptions);
                contactSenderGameObjectName = EditorGUILayout.TextField(contactSenderGameObjectName);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("ContactReceiverの名前", Settings.LabelGuiLayoutOptions);
                contactReceiverGameObjectName = EditorGUILayout.TextField(contactReceiverGameObjectName);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(Localization.S("option.remove.original"), Settings.LabelGuiLayoutOptions);
                isDeleteEnabled = EditorGUILayout.Toggle(isDeleteEnabled);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(Localization.S("option.keep.pb.version"), Settings.LabelGuiLayoutOptions);
                isKeepPBVersion = EditorGUILayout.Toggle(isKeepPBVersion);
            }

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
                        var hasPhysbone = CopyPhysBones();
                        var hasPhysboneCollider = CopyPhysBoneColliders();
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

        private void DrawAvatarRootField()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(Localization.S("label.prefab.root"), Settings.LabelGuiLayoutOptions);
                prefabRoot = (GameObject)EditorGUILayout.ObjectField(prefabRoot,
                    typeof(GameObject), true);
            }
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

        private bool CopyPhysBones()
        {
            var pbParent = new GameObject(pbGameObjectName);
            pbParent.transform.SetParent(_avatarDynamics.transform);

            var vrcPhysBones = new List<VRCPhysBone>(searchRoot.GetComponentsInChildren<VRCPhysBone>());

            foreach (var sourcePhysBone in vrcPhysBones)
            {
                var sourceTransform = sourcePhysBone.transform;
                // Create new GameObject and copy component
                var newPhysBone = new GameObject(sourceTransform.name);
                newPhysBone.transform.SetParent(pbParent.transform);

                // Copy the VRC Phys Bone component
                var newVrcPhysBone = newPhysBone.AddComponent<VRCPhysBone>();
                CopyVRCPhysBone(sourcePhysBone, newVrcPhysBone);

                // Set the Root Transform
                if (!newVrcPhysBone.rootTransform) newVrcPhysBone.rootTransform = sourcePhysBone.transform;

                // Remove original VRC Phys Bone component
                if (isDeleteEnabled) DestroyImmediate(sourcePhysBone);
            }

            if (vrcPhysBones.Count == 0)
            {
                DestroyImmediate(pbParent);
            }

            return vrcPhysBones.Count > 0;
        }

        private bool CopyPhysBoneColliders()
        {
            var pbColliderParent = new GameObject(pbColliderGameObjectName);
            pbColliderParent.transform.SetParent(_avatarDynamics.transform);
            var vrcPhysboneColliders =
                new List<VRCPhysBoneCollider>(searchRoot.GetComponentsInChildren<VRCPhysBoneCollider>());
            foreach (var sourcePbCollider in vrcPhysboneColliders)
            {
                var sourceTransform = sourcePbCollider.transform;
                // Create new GameObject and copy component
                var newCollider = new GameObject(sourceTransform.name);
                newCollider.transform.SetParent(pbColliderParent.transform);

                // Copy the VRC Phys Bone Collider component
                var destPbCollider = newCollider.AddComponent<VRCPhysBoneCollider>();
                CopyVRCPhysboneCollider(sourcePbCollider, destPbCollider);

                // Set the Root Transform
                if (!destPbCollider.rootTransform) destPbCollider.rootTransform = sourcePbCollider.transform;

                // Remove original VRC Phys Bone Collider component
                if (isDeleteEnabled) DestroyImmediate(sourcePbCollider);
            }

            if (vrcPhysboneColliders.Count == 0)
            {
                DestroyImmediate(pbColliderParent);
            }

            return vrcPhysboneColliders.Count > 0;
        }

        private bool CopyVRCContacts()
        {
            var contactsParent = new GameObject(contactsGameObjectName);
            contactsParent.transform.SetParent(_avatarDynamics.transform);
            return CopyVRCContactSender(contactsParent) && CopyVRCContactReceiver(contactsParent);
        }

        private bool CopyVRCContactSender(GameObject parent)
        {
            var contactsParent = new GameObject(contactSenderGameObjectName);
            contactsParent.transform.SetParent(parent.transform);
            var vrcContactSenders = new List<VRCContactSender>(searchRoot.GetComponentsInChildren<VRCContactSender>());
            foreach (var sourceContactSender in vrcContactSenders)
            {
                var sourceTransform = sourceContactSender.transform;
                // Create new GameObject and copy component
                var newContactSender = new GameObject(sourceTransform.name);
                newContactSender.transform.SetParent(contactsParent.transform);

                // Copy the VRC Contact Sender component
                var destContactSender = newContactSender.AddComponent<VRCContactSender>();
                CopyVRCContactSender(sourceContactSender, destContactSender);

                // Set the Root Transform
                if (!destContactSender.rootTransform) destContactSender.rootTransform = sourceContactSender.transform;

                // Remove original VRC Contact Sender component
                if (isDeleteEnabled) DestroyImmediate(sourceContactSender);
            }

            if (vrcContactSenders.Count == 0)
            {
                DestroyImmediate(contactsParent);
            }

            return vrcContactSenders.Count > 0;
        }

        private bool CopyVRCContactReceiver(GameObject parent)
        {
            var contactsParent = new GameObject(contactReceiverGameObjectName);
            contactsParent.transform.SetParent(parent.transform);
            var vrcContactReceivers =
                new List<VRCContactReceiver>(searchRoot.GetComponentsInChildren<VRCContactReceiver>());
            foreach (var sourceContactReceiver in vrcContactReceivers)
            {
                var sourceTransform = sourceContactReceiver.transform;
                // Create new GameObject and copy component
                var newContactReceiver = new GameObject(sourceTransform.name);
                newContactReceiver.transform.SetParent(contactsParent.transform);

                // Copy the VRC Contact Receiver component
                var destContactReceiver = newContactReceiver.AddComponent<VRCContactReceiver>();
                CopyVRCContactReceiver(sourceContactReceiver, destContactReceiver);

                // Set the Root Transform
                if (!destContactReceiver.rootTransform)
                    destContactReceiver.rootTransform = sourceContactReceiver.transform;

                // Remove original VRC Contact Receiver component
                if (isDeleteEnabled) DestroyImmediate(sourceContactReceiver);
            }

            if (vrcContactReceivers.Count == 0)
            {
                DestroyImmediate(contactsParent);
            }

            return vrcContactReceivers.Count > 0;
        }

        private void CopyVRCPhysBone(VRCPhysBone source, VRCPhysBone destination)
        {
            // Vesrion
            if (isKeepPBVersion) destination.version = source.version;

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
                if (File.Exists(Settings.GetLogoPath()))
                {
                    byte[] fileData = File.ReadAllBytes(Settings.GetLogoPath());
                    Texture2D logo = new Texture2D(2, 2); // サイズは後で自動調整される
                    logo.LoadImage(fileData); // PNG, JPG 形式の画像をロード
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(new GUIContent(logo), GUILayout.Height(100), GUILayout.Width(400));
                    GUILayout.FlexibleSpace();
                }
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
            if (prefabRoot == null)
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

            if (isKeepPBVersion)
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