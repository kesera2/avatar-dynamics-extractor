using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using VRC.Dynamics;
using VRC.SDK3.Avatars;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Constraint.Components;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;
using YamlDotNet.Core;

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
        private string contactSenderGameObjectName;
        private string contactReceiverGameObjectName;
        private string constraintsGameObjectName;
        private string vrcPositionConstraintName;
        private string vrcRotationConstraintName;
        private string vrcParentConstraintName;
        private string vrcScaleConstraintName;
        private string vrcAimConstraintName;
        private string vrcLookAtConstraintName;
        private List<VRCPhysBone> copiedPbComponents;
        private List<VRCPhysBoneCollider> copiedPbColliderComponents;
        private List<VRCContactSender> copiedContactSenderComponents;
        private List<VRCContactReceiver> copiedContactReceiverComponents;
        private List<VRCConstraintBase> copiedConstraintComponents;
        private bool splitContacts;
        private bool isSearchRootSet;
        private bool prefabRootChanged;

        [MenuItem("Tools/kesera2/Avatar Dynamics Extractor")]
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
            constraintsGameObjectName ??= Settings.ConstraintsGameObjectName;
            vrcPositionConstraintName ??= Settings.VrcPositionConstraintName;
            vrcRotationConstraintName ??= Settings.VrcRotationConstraintName;
            vrcParentConstraintName ??= Settings.VrcParentConstraintName;
            vrcScaleConstraintName ??= Settings.VrcScaleConstraintName;
            vrcAimConstraintName ??= Settings.VrcAimConstraintName;
            vrcLookAtConstraintName ??= Settings.VrcLookAtConstraintName;
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
                    EditorGUILayout.LabelField(Localization.S("label.prefab.root"), Settings.LabelGuiLayoutOptions);
                    if (prefabRoot)
                    {
                        EditorGUILayout.LabelField(Localization.S("label.search.root"), Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.avatar_dynamics"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.pb"), Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.pbc"), Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.contacts"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("option.separate.contacts"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.contact_sender"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.contact_receiver"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.constraints"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.position_constraints"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.rotation_constraints"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.parent_constraints"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.scale_constraints"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.aim_constraints"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.lookat_constraints"),
                            Settings.LabelGuiLayoutOptions);
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
                        constraintsGameObjectName = EditorGUILayout.TextField(constraintsGameObjectName);
                        vrcPositionConstraintName = EditorGUILayout.TextField(vrcPositionConstraintName);
                        vrcRotationConstraintName = EditorGUILayout.TextField(vrcRotationConstraintName);
                        vrcParentConstraintName = EditorGUILayout.TextField(vrcParentConstraintName);
                        vrcScaleConstraintName = EditorGUILayout.TextField(vrcScaleConstraintName);
                        vrcAimConstraintName = EditorGUILayout.TextField(vrcAimConstraintName);
                        vrcLookAtConstraintName = EditorGUILayout.TextField(vrcLookAtConstraintName);
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
                        var constraints = new Constraints(vrcPositionConstraintName, vrcRotationConstraintName,
                            vrcParentConstraintName, vrcScaleConstraintName, vrcAimConstraintName,
                            vrcLookAtConstraintName);
                        copiedConstraintComponents = constraints.CopyVRCConstraint(searchRoot, _avatarDynamics, constraintsGameObjectName);
                        copiedPbColliderComponents = Utility.CopyComponent<VRCPhysBoneCollider>(searchRoot,
                            _avatarDynamics,
                            pbColliderGameObjectName, CopyVRCPhysboneCollider);
                        copiedPbComponents =
                            Utility.CopyComponent<VRCPhysBone>(searchRoot, _avatarDynamics, pbGameObjectName,
                                CopyVRCPhysBone);
                        var hasContacts = CopyVRCContacts();
                        if (!(copiedPbComponents.Count > 0 || copiedPbColliderComponents.Count > 0 || hasContacts))
                            // Destroy the GameObject of AvatarDynamics if there is no target components.
                            DestroyImmediate(_avatarDynamics);
                        RemoveOriginalComponent();
                        ChangeOrder();
                    }
                    else
                    {
                        Debug.LogWarning("Please assign both Target and Search Root GameObjects.");
                    }

                    DisplayEndDialog();
                }
            }

            DisplayInfo();
            ValidateSearchRoot();
            DisplayWarnSearchRoot();
        }

        /**
         * Change the order of the child objects of the AvatarDynamics GameObject.
         */
        private void ChangeOrder()
        {
            if (copiedPbComponents.Count == 0 && copiedPbColliderComponents.Count == 0 &&
                copiedContactSenderComponents.Count == 0 && copiedContactReceiverComponents.Count == 0)
            {
                return;
            }

            // Define the order of the child objects
            string[] order =
            {
                pbGameObjectName,
                pbColliderGameObjectName,
                contactsGameObjectName,
                contactSenderGameObjectName,
                contactReceiverGameObjectName
            };

            // Get all child objects of the AvatarDynamics GameObject
            Transform[] children = _avatarDynamics.GetComponentsInChildren<Transform>(true);

            // Create a list to store the sorted child objects
            List<Transform> sortedChildren = new List<Transform>();

            // Add the child objects in the order defined above
            foreach (string name in order)
            {
                foreach (Transform child in children)
                {
                    if (child.name == name)
                    {
                        sortedChildren.Add(child);
                        break; // Exit the loop if the child object is found
                    }
                }
            }

            // Set the order of the child objects
            for (int i = 0; i < sortedChildren.Count; i++)
            {
                sortedChildren[i].SetSiblingIndex(i);
            }
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


        private bool CopyVRCContacts()
        {
            var contactsParent = new GameObject(contactsGameObjectName);
            contactsParent.transform.SetParent(_avatarDynamics.transform);

            copiedContactReceiverComponents = Utility.CopyComponent<VRCContactReceiver>(searchRoot, contactsParent,
                contactReceiverGameObjectName,
                CopyVRCContactReceiver, !splitContacts);
            copiedContactSenderComponents = Utility.CopyComponent<VRCContactSender>(searchRoot, contactsParent,
                contactSenderGameObjectName,
                CopyVRCContactSender, !splitContacts);

            if (copiedContactReceiverComponents.Count == 0 && copiedContactSenderComponents.Count == 0)
            {
                DestroyImmediate(contactsParent);
                return false;
            }

            return true;
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
            destination.colliders = new List<VRCPhysBoneColliderBase>(from sourceCollider in source.colliders
                join copiedCollider in copiedPbColliderComponents on sourceCollider.name equals copiedCollider.name
                select copiedCollider).ToList();
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

        private void RemoveOriginalComponent()
        {
            if (isDeleteEnabled)
            {
                var physboneComponents = new List<VRCPhysBone>(searchRoot.GetComponentsInChildren<VRCPhysBone>());
                physboneComponents.ForEach(DestroyImmediate);
                var physboneColliderComponents =
                    new List<VRCPhysBoneCollider>(searchRoot.GetComponentsInChildren<VRCPhysBoneCollider>());
                physboneColliderComponents.ForEach(DestroyImmediate);
                var contactSenderComponents =
                    new List<VRCContactSender>(searchRoot.GetComponentsInChildren<VRCContactSender>());
                contactSenderComponents.ForEach(DestroyImmediate);
                var contactReceiverComponents =
                    new List<VRCContactReceiver>(searchRoot.GetComponentsInChildren<VRCContactReceiver>());
                contactReceiverComponents.ForEach(DestroyImmediate);
                var constraintsComponents =
                    new List<VRCConstraintBase>(searchRoot.GetComponentsInChildren<VRCConstraintBase>());
                constraintsComponents.ForEach(DestroyImmediate);
            }
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

        private bool ValidateSearchRoot()
        {
            if (searchRoot.transform.parent != prefabRoot.transform)
            {
                EditorGUILayout.HelpBox(Localization.S("warn.search.root.not.child"), MessageType.Warning);
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

        private void DisplayEndDialog()
        {
            EditorUtility.DisplayDialog(
                Localization.S("msg.dialog.title"), // ダイアログのタイトル
                string.Format(Localization.S("msg.dialog.end"), copiedPbComponents.Count,
                    copiedPbColliderComponents.Count,
                    copiedContactSenderComponents.Count,
                    copiedContactReceiverComponents.Count,
                    copiedConstraintComponents.Count),
                "OK"
            );
        }
    }
}