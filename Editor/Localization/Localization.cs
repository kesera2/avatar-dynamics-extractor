using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace dev.kesera2.physbone_extractor
{
    public static class Localization
    {
        private const string LocalizationPathGuid = "603be7a353df47e0b812ae8f1f1b2a28";
        private static readonly string LocalizationPathRoot = AssetDatabase.GUIDToAssetPath(LocalizationPathGuid);
        private static Dictionary<string, string> _translations;
        private static string _selectedLanguage; // デフォルト言語

        public static void LoadLocalization(string language)
        {
            Debug.Log("language: " + language);
            string filePath = GetFilePath(language);

            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                _translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);
                _selectedLanguage = language;
            }
            else
            {
                Debug.LogError($"Localization file not found: {filePath}");
            }
        }
        
        public static string GetTranslation(string key)
        {
            if (_translations != null && _translations.TryGetValue(key, out var value))
            {
                return value;
            }
            return key; // デフォルトとしてキーを返す
        }
        
        public static void SetLanguage(string language)
        {
            LoadLocalization(language);
        }
        
        private static string GetFilePath(string language)
        {
            return $"{LocalizationPathRoot}/{language}.json";
        }
    }
}