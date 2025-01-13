using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace dev.kesera2.physbone_extractor
{
    public class PhysBoneExtractor : EditorWindow
    {
        [SerializeField] private GameObject prefabRoot; // 元のGameObject
        [SerializeField] private GameObject searchRoot; // 探索するGameObject
        [SerializeField] private bool isDeleteEnabled = false;
        private static int _selectedLanguage = 0;
        private GameObject _avatarDynamics;
        private Transform _avatarArmature;
        private bool isSearchRootSet;

        [MenuItem("Tools/kesera2/PhysBone Extractor")]
        public static void ShowWindow()
        {
            var window = GetWindow<PhysBoneExtractor>(Settings.WindowTitle);
            window.minSize = Settings.windowMinSize;
        }

        private void OnEnable()
        {
            Localization.LoadLocalization(_selectedLanguage);
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
                GUILayout.Label(Localization.S("option.remove.original"), Settings.LabelGuiLayoutOptions);
                isDeleteEnabled = EditorGUILayout.Toggle(isDeleteEnabled);
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
                        if (!(hasPhysbone || hasPhysboneCollider))
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
            _avatarDynamics = new GameObject(Settings.AvatarDynamicsGameObjectName);
            _avatarDynamics.transform.SetParent(prefabRoot.transform);
        }

        private bool CopyPhysBones()
        {
            var pbParent = new GameObject(Settings.PhysboneGameObjectName);
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

            return vrcPhysBones.Count > 0;
        }

        private bool CopyPhysBoneColliders()
        {
            var pbColliderParent = new GameObject(Settings.PhysboneColliderGameObjectName);
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

            return vrcPhysboneColliders.Count > 0;
        }

        private void CopyVRCPhysBone(VRCPhysBone source, VRCPhysBone destination)
        {
            // Vesrion
            destination.version = source.version;
            
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
            var message = Localization.S("msg.dialog.message");
            if (isDeleteEnabled)
            {
                message += "\n" + Localization.S("msg.dialog.remove.original");
            }
            else
            {
                message += "\n" + Localization.S("msg.dialog.keep.original");
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