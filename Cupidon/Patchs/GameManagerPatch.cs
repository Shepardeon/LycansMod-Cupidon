using Cupidon.Services;
using Cupidon.Unity;
using Fusion;
using System.Linq;
using UnityEngine;

namespace Cupidon.Patchs
{
    internal class GameManagerPatch
    {
        public static void Hook()
        {
            On.GameManager.Start += GameManager_Start;
        }

        public static void Unhook()
        {
            On.GameManager.Start -= GameManager_Start;
        }

        private static void GameManager_Start(On.GameManager.orig_Start orig, GameManager self)
        {
            orig(self);

            var cupidon = CupidonPlugin.Cupidon;
            if (cupidon == null) return;
            cupidon.CupidonToggle = UIService.Instance.AddToggleToGameSettings("CUPIDON_CUPIDON_MODE", (value) =>
            {
                Log.Info((value ? "Enabled" : "Disabled") + " cupidon mode");
                cupidon.UpdateCupidonMode(value);
                PlayerPrefs.SetInt("CUPIDON_GAME_SETTINGS_ENABLED", value ? 1 : 0);
            });
        }
    }
}