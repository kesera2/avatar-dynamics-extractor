using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace dev.kesera2.physbone_extractor
{
    public class AvatarDynamicsExtractorService
    {
        public GameObject PrefabRoot { get; private set; }
        public GameObject SearchRoot { get; set; }
        public bool IsKeepPbVersion { get; set; } = false;
        public bool IsDeleteEnabled { get; set; } = true;
        public bool SplitContacts { get; set; }
        
        private GameObject _avatarDynamics;
        private Transform _avatarArmature;
        private bool _isSearchRootSet;
        private bool _prefabRootChanged;
        
        public string AvatarDynamicsGameObjectName { get; set; }
        public string PbGameObjectName { get; set; }
        public string PbColliderGameObjectName { get; set; }
        public string ContactsGameObjectName { get; set; }
        public string ContactSenderGameObjectName { get; set; }
        public string ContactReceiverGameObjectName { get; set; }
        public string ConstraintsGameObjectName { get; set; }
        public string VrcPositionConstraintName { get; set; }
        public string VrcRotationConstraintName { get; set; }
        public string VrcParentConstraintName { get; set; }
        public string VrcScaleConstraintName { get; set; }
        public string VrcAimConstraintName { get; set; }
        public string VrcLookAtConstraintName { get; set; }
        
        private List<VRCPhysBone> _copiedPbComponents;
        private List<VRCPhysBoneCollider> _copiedPbColliderComponents;
        private List<VRCContactSender> _copiedContactSenderComponents;
        private List<VRCContactReceiver> _copiedContactReceiverComponents;
        private List<VRCConstraintBase> _copiedConstraintComponents;

        public AvatarDynamicsExtractorService()
        {
            InitializeDefaultValues();
        }

        private void InitializeDefaultValues()
        {
            AvatarDynamicsGameObjectName = Settings.AvatarDynamicsGameObjectName;
            PbGameObjectName = Settings.PhysboneGameObjectName;
            PbColliderGameObjectName = Settings.PhysboneColliderGameObjectName;
            ContactsGameObjectName = Settings.ContactsGameObjectName;
            ContactSenderGameObjectName = Settings.ContactSenderGameObjectName;
            ContactReceiverGameObjectName = Settings.ContactReceiverGameObjectName;
            ConstraintsGameObjectName = Settings.ConstraintsGameObjectName;
            VrcPositionConstraintName = Settings.VrcPositionConstraintName;
            VrcRotationConstraintName = Settings.VrcRotationConstraintName;
            VrcParentConstraintName = Settings.VrcParentConstraintName;
            VrcScaleConstraintName = Settings.VrcScaleConstraintName;
            VrcAimConstraintName = Settings.VrcAimConstraintName;
            VrcLookAtConstraintName = Settings.VrcLookAtConstraintName;
        }

        public void SetPrefabRoot(GameObject newPrefabRoot)
        {
            if (newPrefabRoot != PrefabRoot)
            {
                _prefabRootChanged = true;
            }
            PrefabRoot = newPrefabRoot;
        }

        public void UpdateSearchRoot()
        {
            _avatarArmature = GetArmatureTransform();
            if ((_avatarArmature && !_isSearchRootSet) || _prefabRootChanged)
            {
                SearchRoot = _avatarArmature.gameObject;
                _isSearchRootSet = true;
                _prefabRootChanged = false;
            }
        }

        public bool CanExecute()
        {
            return SearchRoot;
        }

        public bool ValidatePrefabRoot()
        {
            if (!PrefabRoot)
            {
                EditorGUILayout.HelpBox(Localization.S("warn.assign.prefab"), MessageType.Warning);
                _isSearchRootSet = false;
                return false;
            }

            if (Utility.hasParentGameObject(PrefabRoot))
            {
                EditorGUILayout.HelpBox(Localization.S("warn.assign.avatar.root"), MessageType.Warning);
                return false;
            }

            return true;
        }

        public bool ValidateSearchRoot()
        {
            if (SearchRoot != null && SearchRoot.transform.parent != PrefabRoot.transform)
            {
                EditorGUILayout.HelpBox(Localization.S("warn.search.root.not.child"), MessageType.Warning);
                return false;
            }

            return true;
        }

        public bool ExecuteExtraction()
        {
            if (PrefabRoot == null || SearchRoot == null)
            {
                Debug.LogWarning("Please assign both Target and Search Root GameObjects.");
                return false;
            }

            CreateAvatarDynamics();
            
            var constraints = new Constraints(VrcPositionConstraintName, VrcRotationConstraintName,
                VrcParentConstraintName, VrcScaleConstraintName, VrcAimConstraintName,
                VrcLookAtConstraintName);
            _copiedConstraintComponents =
                constraints.CopyVRCConstraint(SearchRoot, _avatarDynamics, ConstraintsGameObjectName);
            
            var physBones = new PhysBones(PbGameObjectName, PbColliderGameObjectName, IsKeepPbVersion);
            _copiedPbColliderComponents = physBones.CopyVRCPhysBoneColliders(SearchRoot, _avatarDynamics);
            _copiedPbComponents =
                physBones.CopyVRCPhysBones(SearchRoot, _avatarDynamics, _copiedPbColliderComponents);
            
            var contacts = new Contacts(ContactsGameObjectName, ContactSenderGameObjectName,
                ContactReceiverGameObjectName, SplitContacts);
            var hasContacts = contacts.CopyVRCContacts(SearchRoot, _avatarDynamics,
                out _copiedContactSenderComponents, out _copiedContactReceiverComponents);
            
            if (!(_copiedPbComponents.Count > 0 || _copiedPbColliderComponents.Count > 0 || hasContacts))
                // Destroy the GameObject of AvatarDynamics if there is no target components.
                UnityEngine.Object.DestroyImmediate(_avatarDynamics);
                
            RemoveOriginalComponent();
            ChangeOrder();
            
            return true;
        }

        public (int, int, int, int, int) GetCopiedComponentCounts()
        {
            return (_copiedPbComponents?.Count ?? 0, 
                    _copiedPbColliderComponents?.Count ?? 0,
                    _copiedContactSenderComponents?.Count ?? 0, 
                    _copiedContactReceiverComponents?.Count ?? 0,
                    _copiedConstraintComponents?.Count ?? 0);
        }

        private Transform GetArmatureTransform()
        {
            if (!PrefabRoot) return null;
            return PrefabRoot.GetComponentsInChildren<Transform>()
                .FirstOrDefault(t => t.name.ToLower() == "armature");
        }

        private void CreateAvatarDynamics()
        {
            // Create PB GameObject under the target GameObject
            _avatarDynamics = new GameObject(AvatarDynamicsGameObjectName);
            _avatarDynamics.transform.SetParent(PrefabRoot.transform);
        }

        private void RemoveOriginalComponent()
        {
            if (IsDeleteEnabled)
            {
                var physboneComponents = new List<VRCPhysBone>(SearchRoot.GetComponentsInChildren<VRCPhysBone>());
                physboneComponents.ForEach(UnityEngine.Object.DestroyImmediate);
                var physboneColliderComponents =
                    new List<VRCPhysBoneCollider>(SearchRoot.GetComponentsInChildren<VRCPhysBoneCollider>());
                physboneColliderComponents.ForEach(UnityEngine.Object.DestroyImmediate);
                var contactSenderComponents =
                    new List<VRCContactSender>(SearchRoot.GetComponentsInChildren<VRCContactSender>());
                contactSenderComponents.ForEach(UnityEngine.Object.DestroyImmediate);
                var contactReceiverComponents =
                    new List<VRCContactReceiver>(SearchRoot.GetComponentsInChildren<VRCContactReceiver>());
                contactReceiverComponents.ForEach(UnityEngine.Object.DestroyImmediate);
                var constraintsComponents =
                    new List<VRCConstraintBase>(SearchRoot.GetComponentsInChildren<VRCConstraintBase>());
                constraintsComponents.ForEach(UnityEngine.Object.DestroyImmediate);
            }
        }

        private void ChangeOrder()
        {
            if (_copiedPbComponents.Count == 0 && _copiedPbColliderComponents.Count == 0 &&
                _copiedContactSenderComponents.Count == 0 && _copiedContactReceiverComponents.Count == 0)
            {
                return;
            }

            // Define the order of the child objects
            string[] order =
            {
                PbGameObjectName,
                PbColliderGameObjectName,
                ContactsGameObjectName,
                ContactSenderGameObjectName,
                ContactReceiverGameObjectName
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
    }
}