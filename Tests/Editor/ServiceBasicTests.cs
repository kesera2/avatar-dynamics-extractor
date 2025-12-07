using NUnit.Framework;
using UnityEngine;
using dev.kesera2.physbone_extractor;

namespace dev.kesera2.physbone_extractor.Tests
{
    public class ServiceBasicTests
    {
        private AvatarDynamicsExtractorService _service;

        [SetUp]
        public void SetUp()
        {
            // Test service instantiation (VRC-independent)
            _service = new AvatarDynamicsExtractorService();
        }

        [Test]
        public void Service_CanBeInstantiated()
        {
            Assert.IsNotNull(_service);
        }

        [Test]
        public void Service_DefaultProperties_AreSet()
        {
            // Test that properties are initialized with Settings values
            Assert.AreEqual(Settings.AvatarDynamicsGameObjectName, _service.AvatarDynamicsGameObjectName);
            Assert.AreEqual(Settings.PhysboneGameObjectName, _service.PbGameObjectName);
            Assert.AreEqual(Settings.PhysboneColliderGameObjectName, _service.PbColliderGameObjectName);
            Assert.AreEqual(Settings.ContactsGameObjectName, _service.ContactsGameObjectName);
            Assert.AreEqual(Settings.ContactSenderGameObjectName, _service.ContactSenderGameObjectName);
            Assert.AreEqual(Settings.ContactReceiverGameObjectName, _service.ContactReceiverGameObjectName);
            Assert.AreEqual(Settings.ConstraintsGameObjectName, _service.ConstraintsGameObjectName);
        }

        [Test]
        public void Service_DefaultConstraintNames_AreSet()
        {
            Assert.AreEqual(Settings.VrcPositionConstraintName, _service.VrcPositionConstraintName);
            Assert.AreEqual(Settings.VrcRotationConstraintName, _service.VrcRotationConstraintName);
            Assert.AreEqual(Settings.VrcParentConstraintName, _service.VrcParentConstraintName);
            Assert.AreEqual(Settings.VrcScaleConstraintName, _service.VrcScaleConstraintName);
            Assert.AreEqual(Settings.VrcAimConstraintName, _service.VrcAimConstraintName);
            Assert.AreEqual(Settings.VrcLookAtConstraintName, _service.VrcLookAtConstraintName);
        }

        [Test]
        public void Service_DefaultBooleanOptions_AreSet()
        {
            Assert.IsFalse(_service.IsKeepPbVersion);
            Assert.IsTrue(_service.IsDeleteEnabled);
            Assert.IsFalse(_service.SplitContacts);
        }

        [Test]
        public void Service_PropertySetters_Work()
        {
            // Test string property setters
            _service.AvatarDynamicsGameObjectName = "TestDynamics";
            _service.PbGameObjectName = "TestPB";
            _service.PbColliderGameObjectName = "TestColliders";
            _service.ContactsGameObjectName = "TestContacts";
            _service.ConstraintsGameObjectName = "TestConstraints";
            
            Assert.AreEqual("TestDynamics", _service.AvatarDynamicsGameObjectName);
            Assert.AreEqual("TestPB", _service.PbGameObjectName);
            Assert.AreEqual("TestColliders", _service.PbColliderGameObjectName);
            Assert.AreEqual("TestContacts", _service.ContactsGameObjectName);
            Assert.AreEqual("TestConstraints", _service.ConstraintsGameObjectName);
        }

        [Test]
        public void Service_BooleanPropertySetters_Work()
        {
            // Test boolean property setters
            _service.IsKeepPbVersion = true;
            _service.IsDeleteEnabled = false;
            _service.SplitContacts = true;
            
            Assert.IsTrue(_service.IsKeepPbVersion);
            Assert.IsFalse(_service.IsDeleteEnabled);
            Assert.IsTrue(_service.SplitContacts);
        }

        [Test]
        public void Service_ContactSeparationProperties_Work()
        {
            _service.SplitContacts = true;
            _service.ContactSenderGameObjectName = "CustomSenders";
            _service.ContactReceiverGameObjectName = "CustomReceivers";
            
            Assert.IsTrue(_service.SplitContacts);
            Assert.AreEqual("CustomSenders", _service.ContactSenderGameObjectName);
            Assert.AreEqual("CustomReceivers", _service.ContactReceiverGameObjectName);
        }

        [Test]
        public void Service_PrefabRootProperty_InitiallyNull()
        {
            Assert.IsNull(_service.PrefabRoot);
        }

        [Test]
        public void Service_SearchRootProperty_InitiallyNull()
        {
            Assert.IsNull(_service.SearchRoot);
        }

        [Test]
        public void Service_SetPrefabRoot_Works()
        {
            var testGameObject = new GameObject("TestPrefabRoot");
            
            try
            {
                _service.SetPrefabRoot(testGameObject);
                Assert.AreEqual(testGameObject, _service.PrefabRoot);
                
                _service.SetPrefabRoot(null);
                Assert.IsNull(_service.PrefabRoot);
            }
            finally
            {
                if (testGameObject != null)
                    Object.DestroyImmediate(testGameObject);
            }
        }

        [Test]
        public void Service_SearchRootSetter_Works()
        {
            var testGameObject = new GameObject("TestSearchRoot");
            
            try
            {
                _service.SearchRoot = testGameObject;
                Assert.AreEqual(testGameObject, _service.SearchRoot);
                
                _service.SearchRoot = null;
                Assert.IsNull(_service.SearchRoot);
            }
            finally
            {
                if (testGameObject != null)
                    Object.DestroyImmediate(testGameObject);
            }
        }

        [Test]
        public void Service_CanExecute_WithoutSearchRoot_ReturnsFalse()
        {
            _service.SearchRoot = null;
            Assert.IsFalse(_service.CanExecute());
        }

        [Test]
        public void Service_CanExecute_WithSearchRoot_ReturnsTrue()
        {
            var testGameObject = new GameObject("TestSearchRoot");
            
            try
            {
                _service.SearchRoot = testGameObject;
                Assert.IsTrue(_service.CanExecute());
            }
            finally
            {
                if (testGameObject != null)
                    Object.DestroyImmediate(testGameObject);
            }
        }

        [Test]
        public void Service_GetCopiedComponentCounts_InitiallyZero()
        {
            var (pbCount, pbcCount, senderCount, receiverCount, constraintCount) = _service.GetCopiedComponentCounts();
            
            Assert.AreEqual(0, pbCount);
            Assert.AreEqual(0, pbcCount);
            Assert.AreEqual(0, senderCount);
            Assert.AreEqual(0, receiverCount);
            Assert.AreEqual(0, constraintCount);
        }
    }
}