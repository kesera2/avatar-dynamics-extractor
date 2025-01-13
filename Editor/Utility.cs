using UnityEngine;

namespace dev.kesera2.physbone_extractor
{
    public static class Utility
    {
        public static bool hasParentGameObject(GameObject gameObject)
        {
            return gameObject.transform.parent != null;
        }
    }
}