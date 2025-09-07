using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.Constraint.Components;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace dev.kesera2.physbone_extractor
{
    public static class Utility
    {
        public static List<T> CopyComponent<T>(GameObject searchRoot, GameObject parent, string gameObjectName, System.Action<T, T> copyAction,
            bool useParentGameObject = false) where T : Component
        {
            GameObject componentsParent;

            // Create a parent GameObject for the copied components
            if (useParentGameObject)
            {
                componentsParent = parent;
            }
            else
            {
                // Create a new GameObject if the parent GameObject is not used
                componentsParent = new GameObject(gameObjectName);
                componentsParent.transform.SetParent(parent.transform);
            }

            // Get all components of the specified type
            var components = new List<T>(searchRoot.GetComponentsInChildren<T>());

            // Create a list to store the copied components
            var destComponents = new List<T>();

            foreach (var sourceComponent in components)
            {
                var sourceTransform = sourceComponent.transform;
                // Create new GameObject and copy component
                var newComponent = new GameObject(sourceTransform.name);
                newComponent.transform.SetParent(useParentGameObject ? parent.transform : componentsParent.transform);

                // Copy the component using the provided copy action
                var destComponent = newComponent.AddComponent<T>();
                copyAction(sourceComponent, destComponent);

                // Add the copied component to the list
                destComponents.Add(destComponent);

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
                else if (destComponent is VRCRotationConstraint destConstraint &&
                         sourceComponent is VRCRotationConstraint sourceConstraint)
                {
                    if (!destConstraint.TargetTransform) destConstraint.TargetTransform = sourceConstraint.transform;
                }
            }

            if (components.Count == 0 && !useParentGameObject)
            {
                Undo.DestroyObjectImmediate(componentsParent);
            }

            return destComponents;
        }
        
        public static bool hasParentGameObject(GameObject gameObject)
        {
            return gameObject.transform.parent != null;
        }
    }
}