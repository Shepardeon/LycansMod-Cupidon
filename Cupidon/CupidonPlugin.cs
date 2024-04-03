using BepInEx;
using Helpers.Collections;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace LycansModTemplate
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInProcess("Lycans.exe")]
    public class CupidonPlugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = $"fr.shepardeon.plugins.cupidon";
        public const string PLUGIN_AUTHOR = "Shepardeon";
        public const string PLUGIN_NAME = "Cupidon";
        public const string PLUGIN_VERSION = "1.0.0";

        private void Awake()
        {
            Log.Init(Logger);

            On.GameManager.Start += GameManager_Start;
        }

        private void GameManager_Start(On.GameManager.orig_Start orig, GameManager self)
        {
            orig(self);

            var ui = self.gameUI.gameSettingsMenu;
            var bRoyale = ui.gameObject.transform.Find("LayoutGroup/Body/TaskPanel/Holder/LayoutGroup/BattleRoyaleSetting");

            // TODO : à déplacer à un endroit où sont charger les traductions
            var table = LocalizationSettings.StringDatabase.GetTable("UI Text");
            table.AddEntry("CUPIDON_CUPIDON_MODE", "Cupidon Mode");

            if (bRoyale != null)
            {
                var clone = Instantiate(bRoyale.gameObject, bRoyale.parent.transform);
                var text = clone.transform.Find("LayoutGroup/SettingNameText").GetComponent<LocalizeStringEvent>();
                Log.Info($"{text != null} => {text}");
                text.SetEntry("CUPIDON_CUPIDON_MODE");

                var toggle = clone.transform.Find("LayoutGroup/ToggleContainer/Toggle").GetComponent<Toggle>();
                Log.Info($"{toggle != null} => {toggle}");

                toggle.GetComponents(typeof(Component)).ForEach(c => Log.Info(c.ToString()));

                Log.Info("Clone ------------------");
                clone.GetComponents(typeof(Component)).ForEach(c => Log.Info(c.ToString()));

                // Disable default behaviour
                for (int i = 0; i < toggle.onValueChanged.GetPersistentEventCount(); i++)
                {
                    toggle.onValueChanged.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
                }


                toggle.onValueChanged.AddListener((val) =>
                {
                    Log.Info($"Cupidon is {val}");
                });
            }
        }
    }
}