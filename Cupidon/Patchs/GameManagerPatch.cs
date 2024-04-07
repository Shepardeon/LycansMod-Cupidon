using Cupidon.Services;
using UnityEngine;

namespace Cupidon.Patchs
{
    internal class GameManagerPatch
    {
        public static void Hook()
        {
            On.GameManager.Start += GameManager_Start;
            On.GameManager.Spawned += GameManager_Spawned;
        }

        public static void Unhook()
        {
            On.GameManager.Start -= GameManager_Start;
            On.GameManager.Spawned -= GameManager_Spawned;
        }

        private static void GameManager_Start(On.GameManager.orig_Start orig, GameManager self)
        {
            orig(self);

            CupidonPlugin.CupidonUI = UIService.Instance.AddToggleToGameSettings("CUPIDON_CUPIDON_MODE", (value) =>
            {
                Log.Info((value ? "Enabled" : "Disabled") + " cupidon mode");
                CupidonPlugin.Cupidon?.UpdateCupidonMode(value);
                PlayerPrefs.SetInt("CUPIDON_GAME_SETTINGS_ENABLED", value ? 1 : 0);
            });
        }

        private static void GameManager_Spawned(On.GameManager.orig_Spawned orig, GameManager self)
        {
            orig(self);

            if (self.Runner.SessionInfo.IsOpen)
            {
                Log.Debug("Session is open.");

                if (self.Runner.IsServer)
                {
                    Log.Debug("Retrieving network object...");
                    var netObjRef = NetworkObjectService.Instance.GetNetworkObject($"{CupidonPlugin.PLUGIN_GUID}.NetworkCupidon");

                    Log.Debug("Spawning networked object...");
                    CupidonPlugin.NetworkObject = self.Runner.Spawn(netObjRef);

                    Log.Debug("Setting initial value...");
                    CupidonPlugin.Cupidon?.UpdateCupidonMode(CupidonPlugin.CupidonUI?.UnityToggle.isOn ?? false);
                }
            }
        }
    }
}