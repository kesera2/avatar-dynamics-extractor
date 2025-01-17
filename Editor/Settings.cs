using UnityEditor;
using UnityEngine;

namespace dev.kesera2.physbone_extractor
{
    public static class Settings
    {
        private static readonly string ToolName = "Avatar Dynamics Extractor";
        public static readonly string WindowTitle = ToolName;
        public const string AvatarDynamicsGameObjectName = "AvatarDynamics";
        public const string PhysboneGameObjectName = "PB";
        public const string PhysboneColliderGameObjectName = "PB_Colliders";
        public const string ContactsGameObjectName = "Contacts";
        public const string ContactSenderGameObjectName = "ContactSender";
        public const string ContactReceiverGameObjectName = "ContactReceiver";
        private const string ImagesFolderGuid = "02ea7ce10f74d444080eff72897e0c7e";
        private static readonly string ImagesPathRoot = AssetDatabase.GUIDToAssetPath(ImagesFolderGuid);
        private const string LogoFileName = "avatar-dynamics-extractor-logo.png";
        public static readonly Vector2 windowMinSize = new Vector2(450, 450);

        public static GUILayoutOption[] LanguageGuiLayoutOptions
        {
            get { return new[] { GUILayout.Width(100) }; }
        }

        public static GUILayoutOption[] LabelGuiLayoutOptions
        {
            get { return new[] { GUILayout.Width(250) }; }
        }

        public static string GetLogoPath()
        {
            return $"{ImagesPathRoot}/{LogoFileName}";
        }
    }
}