using System.Collections.Generic;
using UnityEngine.Localization.Settings;

namespace Cupidon.Services
{
    internal class LanguageService
    {
        public static LanguageService Instance
        {
            get
            {
                _instance ??= new LanguageService();
                return _instance;
            }
        }

        private LanguageService() { }
        private static LanguageService? _instance;
        private readonly Dictionary<string, string> _translationDict = new();

        public void AddEntry(string key, string value)
        {
            _translationDict[key] = value;
        }

        public void HookLocalization()
        {
            PopulateStringDatabase();
            LocalizationSettings.SelectedLocaleChanged += SelectedLocaleChanged;
        }

        private void SelectedLocaleChanged(UnityEngine.Localization.Locale locale)
        {
            PopulateStringDatabase();
        }

        private void PopulateStringDatabase()
        {
            Log.Info("Populating string database...");

            var table = LocalizationSettings.StringDatabase.GetTable("UI Text");
            foreach (var kvp in _translationDict)
            {
                table.AddEntry(kvp.Key, kvp.Value);
            }

            Log.Info("Done!");
        }
    }
}