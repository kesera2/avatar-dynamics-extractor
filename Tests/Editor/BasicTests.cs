using NUnit.Framework;
using UnityEngine;
using dev.kesera2.physbone_extractor;

namespace dev.kesera2.physbone_extractor.Tests
{
    public class BasicTests
    {
        [Test]
        public void Settings_DefaultValues_AreCorrect()
        {
            // Test Settings constants
            Assert.AreEqual("AvatarDynamics", Settings.AvatarDynamicsGameObjectName);
            Assert.AreEqual("PB", Settings.PhysboneGameObjectName);
            Assert.AreEqual("PB_Colliders", Settings.PhysboneColliderGameObjectName);
            Assert.AreEqual("Contacts", Settings.ContactsGameObjectName);
            Assert.AreEqual("ContactSender", Settings.ContactSenderGameObjectName);
            Assert.AreEqual("ContactReceiver", Settings.ContactReceiverGameObjectName);
            Assert.AreEqual("Constraints", Settings.ConstraintsGameObjectName);
        }

        [Test]
        public void Settings_LabelGuiLayoutOptions_IsNotNull()
        {
            // Test Settings GUI options
            var labelOptions = Settings.LabelGuiLayoutOptions;
            Assert.IsNotNull(labelOptions);
            Assert.Greater(labelOptions.Length, 0);
        }

        [Test]
        public void Settings_LanguageGuiLayoutOptions_IsNotNull()
        {
            // Test Settings language options
            var languageOptions = Settings.LanguageGuiLayoutOptions;
            Assert.IsNotNull(languageOptions);
            Assert.Greater(languageOptions.Length, 0);
        }

        [Test]
        public void Service_Instantiation_DoesNotThrow()
        {
            // Test that service can be instantiated without VRC components
            AvatarDynamicsExtractorService service = null;
            Assert.DoesNotThrow(() => {
                service = new AvatarDynamicsExtractorService();
            });
            Assert.IsNotNull(service);
        }

        [Test]
        public void Service_DefaultProperties_AreSet()
        {
            // Test service default properties
            var service = new AvatarDynamicsExtractorService();
            
            Assert.AreEqual(Settings.AvatarDynamicsGameObjectName, service.AvatarDynamicsGameObjectName);
            Assert.AreEqual(Settings.PhysboneGameObjectName, service.PbGameObjectName);
            Assert.AreEqual(Settings.PhysboneColliderGameObjectName, service.PbColliderGameObjectName);
            Assert.AreEqual(Settings.ContactsGameObjectName, service.ContactsGameObjectName);
            Assert.AreEqual(Settings.ContactSenderGameObjectName, service.ContactSenderGameObjectName);
            Assert.AreEqual(Settings.ContactReceiverGameObjectName, service.ContactReceiverGameObjectName);
            Assert.AreEqual(Settings.ConstraintsGameObjectName, service.ConstraintsGameObjectName);
            
            // Test default boolean values
            Assert.IsFalse(service.IsKeepPbVersion);
            Assert.IsTrue(service.IsDeleteEnabled);
            Assert.IsFalse(service.SplitContacts);
        }

        [Test]
        public void Service_PropertySetters_Work()
        {
            var service = new AvatarDynamicsExtractorService();
            
            // Test string property setters
            service.AvatarDynamicsGameObjectName = "TestDynamics";
            service.PbGameObjectName = "TestPB";
            service.PbColliderGameObjectName = "TestColliders";
            service.ContactsGameObjectName = "TestContacts";
            service.ConstraintsGameObjectName = "TestConstraints";
            
            Assert.AreEqual("TestDynamics", service.AvatarDynamicsGameObjectName);
            Assert.AreEqual("TestPB", service.PbGameObjectName);
            Assert.AreEqual("TestColliders", service.PbColliderGameObjectName);
            Assert.AreEqual("TestContacts", service.ContactsGameObjectName);
            Assert.AreEqual("TestConstraints", service.ConstraintsGameObjectName);
            
            // Test boolean property setters
            service.IsKeepPbVersion = true;
            service.IsDeleteEnabled = false;
            service.SplitContacts = true;
            
            Assert.IsTrue(service.IsKeepPbVersion);
            Assert.IsFalse(service.IsDeleteEnabled);
            Assert.IsTrue(service.SplitContacts);
        }

        [Test]
        public void Service_PrefabRootSetter_Works()
        {
            var service = new AvatarDynamicsExtractorService();
            var testRoot = new GameObject("TestRoot");
            
            try
            {
                service.SetPrefabRoot(testRoot);
                Assert.AreEqual(testRoot, service.PrefabRoot);
                
                service.SetPrefabRoot(null);
                Assert.IsNull(service.PrefabRoot);
            }
            finally
            {
                if (testRoot != null)
                    Object.DestroyImmediate(testRoot);
            }
        }

        [Test]
        public void Service_SearchRootProperty_Works()
        {
            var service = new AvatarDynamicsExtractorService();
            var testSearchRoot = new GameObject("TestSearchRoot");
            
            try
            {
                service.SearchRoot = testSearchRoot;
                Assert.AreEqual(testSearchRoot, service.SearchRoot);
                
                service.SearchRoot = null;
                Assert.IsNull(service.SearchRoot);
            }
            finally
            {
                if (testSearchRoot != null)
                    Object.DestroyImmediate(testSearchRoot);
            }
        }

        [Test]
        public void Service_CanExecute_ReturnsFalseWithoutSearchRoot()
        {
            var service = new AvatarDynamicsExtractorService();
            service.SearchRoot = null;
            
            Assert.IsFalse(service.CanExecute());
        }

        [Test]
        public void Service_CanExecute_ReturnsTrueWithSearchRoot()
        {
            var service = new AvatarDynamicsExtractorService();
            var testSearchRoot = new GameObject("TestSearchRoot");
            
            try
            {
                service.SearchRoot = testSearchRoot;
                Assert.IsTrue(service.CanExecute());
            }
            finally
            {
                if (testSearchRoot != null)
                    Object.DestroyImmediate(testSearchRoot);
            }
        }

        [Test]
        public void Utility_HasParentGameObject_Works()
        {
            var parent = new GameObject("Parent");
            var child = new GameObject("Child");
            child.transform.SetParent(parent.transform);
            
            try
            {
                Assert.IsTrue(Utility.hasParentGameObject(child));
                Assert.IsFalse(Utility.hasParentGameObject(parent));
            }
            finally
            {
                Object.DestroyImmediate(parent); // This will also destroy child
            }
        }
    }
}