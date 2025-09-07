using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace dev.kesera2.physbone_extractor
{
    public class PhysBones
    {
        private string pbGameObjectName;
        private string pbColliderGameObjectName;
        private bool isKeepPbVersion;

        public PhysBones(string pbGameObjectName, string pbColliderGameObjectName, bool isKeepPbVersion)
        {
            this.pbGameObjectName = pbGameObjectName;
            this.pbColliderGameObjectName = pbColliderGameObjectName;
            this.isKeepPbVersion = isKeepPbVersion;
        }

        public List<VRCPhysBoneCollider> CopyVRCPhysBoneColliders(GameObject searchRoot, GameObject avatarDynamics)
        {
            return Utility.CopyComponent<VRCPhysBoneCollider>(searchRoot, avatarDynamics,
                pbColliderGameObjectName, CopyVRCPhysboneCollider);
        }

        public List<VRCPhysBone> CopyVRCPhysBones(GameObject searchRoot, GameObject avatarDynamics, List<VRCPhysBoneCollider> copiedPbColliderComponents)
        {
            return Utility.CopyComponent<VRCPhysBone>(searchRoot, avatarDynamics, pbGameObjectName,
                (source, destination) => CopyVRCPhysBone(source, destination, copiedPbColliderComponents));
        }

        private void CopyVRCPhysBone(VRCPhysBone source, VRCPhysBone destination, List<VRCPhysBoneCollider> copiedPbColliderComponents)
        {
            // Vesrion
            if (isKeepPbVersion) destination.version = source.version;

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
    }
}