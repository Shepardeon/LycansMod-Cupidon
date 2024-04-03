using BepInEx;
using TMPro;
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

            if (bRoyale != null)
            {
                var clone = Instantiate(bRoyale.gameObject, bRoyale.parent.transform);
                var text = clone.transform.Find("LayoutGroup/SettingNameText").GetComponent<TextMeshProUGUI>();
                text.text = "Mode Cupidon";

                var toggle = clone.transform.Find("LayoutGroup/ToggleContainer/Toggle").GetComponent<Toggle>();
                Log.Info($"{toggle != null} => {toggle}");
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((val) =>
                {
                    Log.Info($"Cupidon is {val}");
                });
            }
        }
    }
}