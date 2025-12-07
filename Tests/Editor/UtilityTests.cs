using NUnit.Framework;
using UnityEngine;
using dev.kesera2.physbone_extractor;

namespace dev.kesera2.physbone_extractor.Tests
{
    public class UtilityTests
    {
        [Test]
        public void HasParentGameObject_WithParent_ReturnsTrue()
        {
            // Arrange
            var parent = new GameObject("Parent");
            var child = new GameObject("Child");
            child.transform.SetParent(parent.transform);
            
            try
            {
                // Act & Assert
                Assert.IsTrue(Utility.hasParentGameObject(child));
            }
            finally
            {
                // Cleanup
                Object.DestroyImmediate(parent); // This will also destroy child
            }
        }

        [Test]
        public void HasParentGameObject_WithoutParent_ReturnsFalse()
        {
            // Arrange
            var rootObject = new GameObject("RootObject");
            
            try
            {
                // Act & Assert
                Assert.IsFalse(Utility.hasParentGameObject(rootObject));
            }
            finally
            {
                // Cleanup
                Object.DestroyImmediate(rootObject);
            }
        }

        [Test]
        public void HasParentGameObject_WithNullInput_ReturnsFalse()
        {
            // Act & Assert
            Assert.IsFalse(Utility.hasParentGameObject(null));
        }

        [Test]
        public void HasParentGameObject_WithNestedHierarchy_ReturnsTrue()
        {
            // Arrange
            var grandParent = new GameObject("GrandParent");
            var parent = new GameObject("Parent");
            var child = new GameObject("Child");
            
            parent.transform.SetParent(grandParent.transform);
            child.transform.SetParent(parent.transform);
            
            try
            {
                // Act & Assert
                Assert.IsTrue(Utility.hasParentGameObject(child));
                Assert.IsTrue(Utility.hasParentGameObject(parent));
                Assert.IsFalse(Utility.hasParentGameObject(grandParent));
            }
            finally
            {
                // Cleanup
                Object.DestroyImmediate(grandParent); // This will destroy all children
            }
        }

        [Test]
        public void HasParentGameObject_WithDisconnectedTransform_HandlesProperly()
        {
            // Arrange
            var parent = new GameObject("Parent");
            var child = new GameObject("Child");
            child.transform.SetParent(parent.transform);
            
            // Disconnect child
            child.transform.SetParent(null);
            
            try
            {
                // Act & Assert
                Assert.IsFalse(Utility.hasParentGameObject(child));
            }
            finally
            {
                // Cleanup
                Object.DestroyImmediate(parent);
                Object.DestroyImmediate(child);
            }
        }
    }
}