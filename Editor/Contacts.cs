using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Dynamics.Contact.Components;

namespace dev.kesera2.physbone_extractor
{
    public class Contacts
    {
        private string contactsGameObjectName;
        private string contactSenderGameObjectName;
        private string contactReceiverGameObjectName;
        private bool splitContacts;

        public Contacts(string contactsGameObjectName, string contactSenderGameObjectName, 
            string contactReceiverGameObjectName, bool splitContacts)
        {
            this.contactsGameObjectName = contactsGameObjectName;
            this.contactSenderGameObjectName = contactSenderGameObjectName;
            this.contactReceiverGameObjectName = contactReceiverGameObjectName;
            this.splitContacts = splitContacts;
        }

        public bool CopyVRCContacts(GameObject searchRoot, GameObject avatarDynamics, 
            out List<VRCContactSender> copiedContactSenderComponents, 
            out List<VRCContactReceiver> copiedContactReceiverComponents)
        {
            var contactsParent = new GameObject(contactsGameObjectName);
            contactsParent.transform.SetParent(avatarDynamics.transform);

            copiedContactReceiverComponents = Utility.CopyComponent<VRCContactReceiver>(searchRoot, contactsParent,
                contactReceiverGameObjectName,
                CopyVRCContactReceiver, !splitContacts);
            copiedContactSenderComponents = Utility.CopyComponent<VRCContactSender>(searchRoot, contactsParent,
                contactSenderGameObjectName,
                CopyVRCContactSender, !splitContacts);

            if (copiedContactReceiverComponents.Count == 0 && copiedContactSenderComponents.Count == 0)
            {
                Object.DestroyImmediate(contactsParent);
                return false;
            }

            return true;
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
    }
}