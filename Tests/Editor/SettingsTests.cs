using NUnit.Framework;
using UnityEngine;
using dev.kesera2.physbone_extractor;

namespace dev.kesera2.physbone_extractor.Tests
{
    public class SettingsTests
    {
        [Test]
        public void WindowTitle_IsNotNullOrEmpty()
        {
            Assert.IsNotNull(Settings.WindowTitle);
            Assert.IsNotEmpty(Settings.WindowTitle);
        }

        [Test]
        public void AvatarDynamicsGameObjectName_HasExpectedValue()
        {
            Assert.AreEqual("AvatarDynamics", Settings.AvatarDynamicsGameObjectName);
        }

        [Test]
        public void PhysboneGameObjectName_HasExpectedValue()
        {
            Assert.AreEqual("PB", Settings.PhysboneGameObjectName);
        }

        [Test]
        public void PhysboneColliderGameObjectName_HasExpectedValue()
        {
            Assert.AreEqual("PB_Colliders", Settings.PhysboneColliderGameObjectName);
        }

        [Test]
        public void ContactsGameObjectName_HasExpectedValue()
        {
            Assert.AreEqual("Contacts", Settings.ContactsGameObjectName);
        }

        [Test]
        public void ContactSenderGameObjectName_HasExpectedValue()
        {
            Assert.AreEqual("ContactSender", Settings.ContactSenderGameObjectName);
        }

        [Test]
        public void ContactReceiverGameObjectName_HasExpectedValue()
        {
            Assert.AreEqual("ContactReceiver", Settings.ContactReceiverGameObjectName);
        }

        [Test]
        public void ConstraintsGameObjectName_HasExpectedValue()
        {
            Assert.AreEqual("Constraints", Settings.ConstraintsGameObjectName);
        }

        [Test]
        public void VrcConstraintNames_HaveExpectedValues()
        {
            Assert.AreEqual("VRCPositionConstraint", Settings.VrcPositionConstraintName);
            Assert.AreEqual("VRCRotationConstraint", Settings.VrcRotationConstraintName);
            Assert.AreEqual("VRCParentConstraint", Settings.VrcParentConstraintName);
            Assert.AreEqual("VRCScaleConstraint", Settings.VrcScaleConstraintName);
            Assert.AreEqual("VRCAimConstraint", Settings.VrcAimConstraintName);
            Assert.AreEqual("VRCLookAtConstraint", Settings.VrcLookAtConstraintName);
        }

        [Test]
        public void WindowMinSize_IsValid()
        {
            var minSize = Settings.windowMinSize;
            Assert.Greater(minSize.x, 0f);
            Assert.Greater(minSize.y, 0f);
            Assert.AreEqual(450f, minSize.x);
            Assert.AreEqual(450f, minSize.y);
        }

        [Test]
        public void LabelGuiLayoutOptions_IsNotNull()
        {
            var options = Settings.LabelGuiLayoutOptions;
            Assert.IsNotNull(options);
            Assert.Greater(options.Length, 0);
        }

        [Test]
        public void LanguageGuiLayoutOptions_IsNotNull()
        {
            var options = Settings.LanguageGuiLayoutOptions;
            Assert.IsNotNull(options);
            Assert.Greater(options.Length, 0);
        }

        [Test]
        public void GetLogoPath_ReturnsValidPath()
        {
            var logoPath = Settings.GetLogoPath();
            Assert.IsNotNull(logoPath);
            Assert.IsNotEmpty(logoPath);
            Assert.IsTrue(logoPath.Contains("avatar-dynamics-extractor-logo.png"));
        }
    }
}