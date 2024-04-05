using UnityEngine;

namespace Cupidon.Patchs
{
    internal class GameSettingsUIPatch
    {
        public static void Hook()
        {
            On.GameSettingsUI.Start += GameSettingsUI_Start;
            On.GameSettingsUI.ResetSettings += GameSettingsUI_ResetSettings;
        }

        public static void Unhook()
        {
            On.GameSettingsUI.Start -= GameSettingsUI_Start;
            On.GameSettingsUI.ResetSettings -= GameSettingsUI_ResetSettings;
        }

        private static void GameSettingsUI_Start(On.GameSettingsUI.orig_Start orig, GameSettingsUI self)
        {
            orig(self);

            if (PlayerPrefs.HasKey("CUPIDON_GAME_SETTINGS_ENABLED"))
            {
                var toggle = CupidonPlugin.CupidonUI;
                if (toggle == null) return;
                toggle.UnityToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("CUPIDON_GAME_SETTINGS_ENABLED") == 1);
            }
        }

        private static void GameSettingsUI_ResetSettings(On.GameSettingsUI.orig_ResetSettings orig, GameSettingsUI self)
        {
            orig(self);

            var toggle = CupidonPlugin.CupidonUI;
            if (toggle == null) return;
            toggle.UnityToggle.isOn = false;
        }
    }
}