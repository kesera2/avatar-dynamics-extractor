using NUnit.Framework;
using dev.kesera2.physbone_extractor;

namespace dev.kesera2.physbone_extractor
{
    public class LocalizationTests
    {
        [Test]
        public void SupportedLanguageDisplayNames_ContainsExpectedLanguages()
        {
            var displayNames = Localization.SupportedLanguageDisplayNames;

            Assert.IsNotNull(displayNames);
            Assert.IsTrue(displayNames.ContainsKey("ja-JP"));
            Assert.IsTrue(displayNames.ContainsKey("en-US"));
            Assert.AreEqual("日本語", displayNames["ja-JP"]);
            Assert.AreEqual("English", displayNames["en-US"]);
        }

        [Test]
        public void SupportedLanguages_ContainsExpectedLanguages()
        {
            var languages = Localization.SupportedLanguages;

            Assert.IsNotNull(languages);
            Assert.Greater(languages.Count, 0);
            Assert.Contains("ja-JP", languages);
            Assert.Contains("en-US", languages);
        }

        [Test]
        public void DisplayNames_IsNotNullOrEmpty()
        {
            var displayNames = Localization.DisplayNames;

            Assert.IsNotNull(displayNames);
            Assert.Greater(displayNames.Length, 0);

            foreach (var displayName in displayNames)
            {
                Assert.IsNotNull(displayName);
                Assert.IsNotEmpty(displayName);
            }
        }

        [Test]
        public void DisplayNames_CountMatchesSupportedLanguages()
        {
            var displayNames = Localization.DisplayNames;
            var languages = Localization.SupportedLanguages;

            Assert.AreEqual(languages.Count, displayNames.Length);
        }

        [Test]
        public void S_WithNonexistentKey_ReturnsKey()
        {
            // This test assumes that if localization isn't loaded or key doesn't exist,
            // the method returns the key itself as fallback
            var nonexistentKey = "nonexistent.key.test";
            var result = Localization.S(nonexistentKey);

            Assert.AreEqual(nonexistentKey, result);
        }

        [Test]
        public void S_WithNullKey_HandlesGracefully()
        {
            // Test null input handling
            var result = Localization.S(null);

            // The method should handle null gracefully (either return null or empty string)
            // This test verifies it doesn't throw an exception
            Assert.DoesNotThrow(() => Localization.S(null));
        }

        [Test]
        public void LoadLocalization_WithValidIndex_DoesNotThrow()
        {
            // Test loading localization by index
            Assert.DoesNotThrow(() => Localization.LoadLocalization(0));

            // Test with different valid index if available
            if (Localization.SupportedLanguages.Count > 1)
            {
                Assert.DoesNotThrow(() => Localization.LoadLocalization(1));
            }
        }

        [Test]
        public void LoadLocalization_WithValidLanguageCode_DoesNotThrow()
        {
            // Test loading localization by language code
            Assert.DoesNotThrow(() => Localization.LoadLocalization("ja-JP"));
            Assert.DoesNotThrow(() => Localization.LoadLocalization("en-US"));
        }
    }
}
