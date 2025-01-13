using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
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

        internal static ImmutableDictionary<string, string> SupportedLanguageDisplayNames
            = ImmutableDictionary<string, string>.Empty
                .Add("ja-JP", "日本語")
                .Add("en-US", "English");
        
        internal static ImmutableList<string>
            SupportedLanguages = new string[] {"ja-JP", "en-US"}.ToImmutableList();

        internal static string[] DisplayNames = SupportedLanguages.Select(l =>
        {
            return SupportedLanguageDisplayNames.TryGetValue(l, out var displayName) ? displayName : l;
        }).ToArray();
        
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
        
        public static string S(string key)
        {
            if (_translations != null && _translations.TryGetValue(key, out var value))
            {
                return value;
            }
            return key; // デフォルトとしてキーを返す
        }
        
        private static string GetFilePath(string language)
        {
            return $"{LocalizationPathRoot}/{language}.json";
        }
    }
}