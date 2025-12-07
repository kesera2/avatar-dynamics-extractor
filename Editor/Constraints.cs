using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Constraint.Components;

namespace dev.kesera2.physbone_extractor
{
    public class Constraints
    {
        private string vrcPositionConstraintName;
        private string vrcRotationConstraintName;
        private string vrcParentConstraintName;
        private string vrcScaleConstraintName;
        private string vrcAimConstraintName;
        private string vrcLookAtConstraintName;
        
        public Constraints(string vrcPositionConstraintName, string vrcRotationConstraintName,
            string vrcParentConstraintName, string vrcScaleConstraintName, string vrcAimConstraintName,
            string vrcLookAtConstraintName)
        {
            this.vrcPositionConstraintName = vrcPositionConstraintName;
            this.vrcRotationConstraintName = vrcRotationConstraintName;
            this.vrcParentConstraintName = vrcParentConstraintName;
            this.vrcScaleConstraintName = vrcScaleConstraintName;
            this.vrcAimConstraintName = vrcAimConstraintName;
            this.vrcLookAtConstraintName = vrcLookAtConstraintName;
        }
        public List<VRCConstraintBase> CopyVRCConstraint(GameObject searchRoot, GameObject avatarDynamics,
            string constraintsGameObjectName)
        {
            var constraintsParent = new GameObject(constraintsGameObjectName);
            constraintsParent.transform.SetParent(avatarDynamics.transform);
            var constrainedChildren = Constraints.GetConstrainedChildren(searchRoot);
            ConvertToVRCConstraint(searchRoot);
            var constraintComponents = new List<VRCConstraintBase>();
            constraintComponents.AddRange(Utility.CopyComponent<VRCPositionConstraint>(searchRoot, constraintsParent,
                vrcPositionConstraintName,
                CopyVRCPositionConstraint));
            constraintComponents.AddRange(Utility.CopyComponent<VRCRotationConstraint>(searchRoot, constraintsParent,
                vrcRotationConstraintName,
                CopyVRCRotationConstraint));
            constraintComponents.AddRange(Utility.CopyComponent<VRCParentConstraint>(searchRoot, constraintsParent,
                vrcParentConstraintName,
                CopyVRCParentConstraint));
            constraintComponents.AddRange(Utility.CopyComponent<VRCScaleConstraint>(searchRoot, constraintsParent,
                vrcScaleConstraintName,
                CopyVRCScaleConstraint));
            constraintComponents.AddRange(Utility.CopyComponent<VRCAimConstraint>(searchRoot, constraintsParent,
                vrcAimConstraintName,
                CopyVRCAimConstraint));
            constraintComponents.AddRange(Utility.CopyComponent<VRCLookAtConstraint>(searchRoot, constraintsParent,
                vrcLookAtConstraintName,
                CopyVRCLookAtConstraint));

            return constraintComponents;
        }

        private static void CopyVRCConstraintBase(VRCConstraintBase source, VRCConstraintBase destination)
        {
            destination.name = source.name;
            destination.IsActive = source.IsActive;
            destination.GlobalWeight = source.GlobalWeight;
            destination.runInEditMode = source.runInEditMode;
            // Constraint Settings
            destination.Locked = source.Locked;
            // Sources
            destination.Sources = source.Sources;
            // Advanced Settings
            destination.TargetTransform = source.TargetTransform;
            destination.SolveInLocalSpace = source.SolveInLocalSpace;
            destination.FreezeToWorld = source.FreezeToWorld;
            destination.RebakeOffsetsWhenUnfrozen = source.RebakeOffsetsWhenUnfrozen;
        }

        private static void CopyVRCPositionConstraint(VRCPositionConstraint source, VRCPositionConstraint destination)
        {
            CopyVRCConstraintBase(source, destination);
            // Constraint Settings
            destination.PositionAtRest = source.PositionAtRest;
            destination.PositionOffset = source.PositionOffset;
            // Freeze Axes
            destination.AffectsPositionX = source.AffectsPositionX;
            destination.AffectsPositionY = source.AffectsPositionY;
            destination.AffectsPositionZ = source.AffectsPositionZ;
        }

        private static void CopyVRCRotationConstraint(VRCRotationConstraint source, VRCRotationConstraint destination)
        {
            CopyVRCConstraintBase(source, destination);
            // Constraint Settings
            destination.RotationAtRest = source.RotationAtRest;
            destination.RotationOffset = source.RotationOffset;
            // Freeze Axes
            destination.AffectsRotationX = source.AffectsRotationX;
            destination.AffectsRotationY = source.AffectsRotationY;
            destination.AffectsRotationZ = source.AffectsRotationZ;
        }

        private static void CopyVRCParentConstraint(VRCParentConstraint source, VRCParentConstraint destination)
        {
            CopyVRCConstraintBase(source, destination);
            // Constraint Settings
            destination.PositionAtRest = source.PositionAtRest;
            destination.RotationAtRest = source.RotationAtRest;
            // Freeze Axes
            destination.AffectsRotationX = source.AffectsRotationX;
            destination.AffectsRotationY = source.AffectsRotationY;
            destination.AffectsRotationZ = source.AffectsRotationZ;
            destination.AffectsPositionX = source.AffectsPositionX;
            destination.AffectsPositionY = source.AffectsPositionY;
            destination.AffectsPositionZ = source.AffectsPositionZ;
        }

        private static void CopyVRCScaleConstraint(VRCScaleConstraint source, VRCScaleConstraint destination)
        {
            CopyVRCConstraintBase(source, destination);
            // Constraint Settings
            destination.ScaleAtRest = source.ScaleAtRest;
            destination.ScaleOffset = source.ScaleOffset;
            // Freeze Axes
            destination.AffectsScaleX = source.AffectsScaleX;
            destination.AffectsScaleY = source.AffectsScaleY;
            destination.AffectsScaleZ = source.AffectsScaleZ;
        }

        private static void CopyVRCAimConstraint(VRCAimConstraint source, VRCAimConstraint destination)
        {
            CopyVRCConstraintBase(source, destination);
            destination.AimAxis = source.AimAxis;
            destination.UpAxis = source.UpAxis;
            destination.WorldUp = source.WorldUp;
            destination.WorldUpVector = source.WorldUpVector;
            // Constraint Settings
            destination.RotationOffset = source.RotationOffset;
            destination.RotationAtRest = source.RotationAtRest;
            // Freeze Axes
            destination.AffectsRotationX = source.AffectsRotationX;
            destination.AffectsRotationY = source.AffectsRotationY;
            destination.AffectsRotationZ = source.AffectsRotationZ;
        }

        private static void CopyVRCLookAtConstraint(VRCLookAtConstraint source, VRCLookAtConstraint destination)
        {
            CopyVRCConstraintBase(source, destination);
            destination.UseUpTransform = source.UseUpTransform;
            destination.Roll = source.Roll;
            // Constraint Settings
            destination.RotationOffset = source.RotationOffset;
            destination.RotationAtRest = source.RotationAtRest;
        }

        public static List<GameObject> GetConstrainedChildren(GameObject parent)
        {
            List<GameObject> result = new List<GameObject>();
            if (parent == null) return result;

            if (parent.GetComponent<UnityEngine.Animations.IConstraint>() != null)
                result.Add(parent);

            foreach (Transform child in parent.transform)
            {
                result.AddRange(GetConstrainedChildren(child.gameObject));
            }

            return result;
        }

        public static void ConvertToVRCConstraint(GameObject searchRoot)
        {
            var unityConstraints = new List<IConstraint>(searchRoot.GetComponentsInChildren<IConstraint>(true));
            Constraints.ConvertConstraints(unityConstraints);
        }

        public static List<VRCConstraintBase> ConvertConstraints(List<IConstraint> unityConstraints)
        {
            List<VRCConstraintBase> vrcConstraints = new List<VRCConstraintBase>();

            // VRCConstraintManager の型を取得
            Type managerType = Type.GetType("VRC.SDK3.Dynamics.Constraint.Components.VRCConstraintManager, VRC.SDK3.Dynamics.Constraint");
            if (managerType == null)
            {
                Debug.LogError("VRCConstraintManager type not found");
                return vrcConstraints;
            }

            // TryCreateSubstituteConstraint メソッドを取得
            MethodInfo tryCreateMethod = managerType.GetMethod(
                "TryCreateSubstituteConstraint",
                BindingFlags.Public | BindingFlags.Static
            );

            if (tryCreateMethod == null)
            {
                Debug.LogError("TryCreateSubstituteConstraint method not found");
                return vrcConstraints;
            }

            // ジェネリックメソッドを構築
            MethodInfo genericMethod = tryCreateMethod.MakeGenericMethod(
                typeof(VRCPositionConstraint),
                typeof(VRCRotationConstraint),
                typeof(VRCScaleConstraint),
                typeof(VRCParentConstraint),
                typeof(VRCAimConstraint),
                typeof(VRCLookAtConstraint)
            );

            foreach (IConstraint unityConstraint in unityConstraints)
            {
                Component unityComponent = unityConstraint as Component;

                // メソッド呼び出しのパラメータ
                object[] parameters = new object[] { unityConstraint, null, null, false };

                // リフレクション経由でメソッドを呼び出し
                bool success = (bool)genericMethod.Invoke(null, parameters);
                VRCConstraintBase substitute = parameters[1] as VRCConstraintBase;

                if (success && substitute != null)
                {
                    vrcConstraints.Add(substitute);
                }

                Undo.DestroyObjectImmediate(unityComponent);
            }

            return vrcConstraints;
        }
    }
}