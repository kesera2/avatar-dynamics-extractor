using System.Collections.Generic;
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
        private static string[] languageOptions = { "ja-jp", "en-us" };
        private static int _selectedLanguage = 0;
        private GameObject _avatarDynamics;
        private Transform _avatarArmature;
        private bool isSearchRootSet;

        [MenuItem("Tools/kesera2/PhysBone Extractor")]
        public static void ShowWindow()
        {
            Localization.LoadLocalization(languageOptions[_selectedLanguage]);
            GetWindow<PhysBoneExtractor>("PhysBone Extractor");
        }
        
        private void ShowSelectLanguage()
        {
            var selectedLanguage = EditorGUILayout.Popup("Language", _selectedLanguage, languageOptions);
            if (_selectedLanguage != selectedLanguage) Localization.LoadLocalization(languageOptions[selectedLanguage]);
            _selectedLanguage = selectedLanguage;
        }

        private void OnGUI()
        {
            // 言語選択ポップアップ
            ShowSelectLanguage();
            prefabRoot =
                (GameObject)EditorGUILayout.ObjectField(Localization.S("label.prefab.root"), prefabRoot, typeof(GameObject), true);
            if (prefabRoot == null)
            {
                EditorGUILayout.HelpBox(Localization.S("warn.assign.prefab"), MessageType.Warning);
                isSearchRootSet = false;
                return;
            }
            if (Utility.hasParentGameObject(prefabRoot))
            {
                EditorGUILayout.HelpBox(Localization.S("warn.assign.avatar.root"), MessageType.Warning);
                return;
            }

            _avatarArmature = GetArmatureTransform();
            if (_avatarArmature && !isSearchRootSet)
            {
                searchRoot = _avatarArmature.gameObject;
                isSearchRootSet = true;
            }
            searchRoot =
                (GameObject)EditorGUILayout.ObjectField(Localization.S("label.search.root"), searchRoot, typeof(GameObject), true);
            isDeleteEnabled = EditorGUILayout.Toggle(Localization.S("option.remove.original"), isDeleteEnabled);
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

        private void CreateAvatarDynamics()
        {
            // Create PB GameObject under the target GameObject
            _avatarDynamics = new GameObject("AvatarDynamics");
            _avatarDynamics.transform.SetParent(prefabRoot.transform);
        }

        private bool CopyPhysBones()
        {
            var pbParent = new GameObject("PB");
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
                if (!newVrcPhysBone.rootTransform) newVrcPhysBone.rootTransform = newPhysBone.transform;

                // Remove original VRC Phys Bone component
                if (isDeleteEnabled) DestroyImmediate(sourcePhysBone);
            }

            return vrcPhysBones.Count > 0;
        }

        private bool CopyPhysBoneColliders()
        {
            var pbColliderParent = new GameObject("PBCollider");
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
                if (!destPbCollider.rootTransform) destPbCollider.rootTransform = newCollider.transform;

                // Remove original VRC Phys Bone Collider component
                if (isDeleteEnabled) DestroyImmediate(sourcePbCollider);
            }

            return vrcPhysboneColliders.Count > 0;
        }

        private void CopyVRCPhysBone(VRCPhysBone source, VRCPhysBone destination)
        {
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
            destination.spring = source.spring;
            destination.stiffness = source.stiffness;
            destination.gravity = source.gravity;
            destination.gravityCurve = source.gravityCurve;
            // Limits
            destination.limitType = source.limitType;
            destination.maxAngleX = source.maxAngleX;
            destination.maxAngleZ = source.maxAngleZ;
            destination.maxAngleXCurve = source.maxAngleXCurve;
            destination.maxAngleZCurve = source.maxAngleZCurve;
            destination.limitRotation = source.limitRotation;
            destination.limitOpacity = source.limitOpacity;
            destination.limitRotationXCurve = source.limitRotationXCurve;
            destination.limitRotationYCurve = source.limitRotationYCurve;
            destination.limitRotationZCurve = source.limitRotationZCurve;
            // Colliders
            destination.radius = source.radius;
            destination.allowCollision = source.allowCollision;
            destination.colliders = source.colliders;
            // Stretch & Squish
            destination.maxSquish = source.maxSquish;
            // Grab & Pose
            destination.allowGrabbing = source.allowGrabbing;
            destination.allowPosing = source.allowPosing;
            destination.grabMovement = source.grabMovement;
            destination.snapToHand = source.snapToHand;
            // Options
            destination.parameter = source.parameter;
            destination.isAnimated = source.isAnimated;
            destination.resetWhenDisabled = source.resetWhenDisabled;
            destination.immobileType = source.immobileType;
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

        private bool DisplayConfirmDialog()
        {
            // メッセージボックスを表示
            return EditorUtility.DisplayDialog(
                Localization.S("msg.dialog.title"), // ダイアログのタイトル
                Localization.S("msg.dialog.message"), // メッセージ
                "OK",
                "Cancel"
            );
        }
    }
}