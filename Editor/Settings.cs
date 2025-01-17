using UnityEditor;
using UnityEngine;

namespace dev.kesera2.physbone_extractor
{
    public static class Settings
    {
        public static readonly string ToolName = "PhysBone Extractor";
        public static readonly string WindowTitle = "Physbone Extractor";
        public static readonly string AvatarDynamicsGameObjectName = "AvatarDynamics";
        public static readonly string PhysboneGameObjectName = "PB";
        public static readonly string PhysboneColliderGameObjectName = "PB_Colliders";
        public static readonly string ContactsGameObjectName = "Contacts";
        public static readonly string ContactSenderGameObjectName = "ContactSender";
        public static readonly string ContactReceiverGameObjectName = "ContactReceiver";
        private static readonly string ImagesFolderGuid = "02ea7ce10f74d444080eff72897e0c7e";
        public static readonly string ImagesPathRoot = AssetDatabase.GUIDToAssetPath(ImagesFolderGuid);
        public static readonly string logoFileName = "pb-extractor-logo.png";
        public static readonly Vector2 windowMinSize = new Vector2(450, 300);
        public static GUILayoutOption[] LanguagGuiLayoutOptions
        {
            get { return new [] { GUILayout.Width(100) } ; }
        }
        public static GUILayoutOption[] LabelGuiLayoutOptions
        {
            get { return new [] { GUILayout.Width(250) } ; }
        }
        
        public static string GetLogoPath()
        {
            return $"{ImagesPathRoot}/{logoFileName}";
        }
    }
}