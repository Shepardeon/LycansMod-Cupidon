using BepInEx;
using Cupidon.Patchs;
using Cupidon.Services;
using Cupidon.Unity;
using Fusion;
using UnityEngine;

namespace Cupidon
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInProcess("Lycans.exe")]
    public class CupidonPlugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = $"fr.shepardeon.plugins.cupidon";
        public const string PLUGIN_AUTHOR = "Shepardeon";
        public const string PLUGIN_NAME = "Cupidon";
        public const string PLUGIN_VERSION = "1.0.0";

        internal static UIToggle? CupidonUI { get; set; }

        internal static NetworkObject? NetworkObject { get; set; }
        internal static NetworkedCupidon? Cupidon
        {
            get
            {
                if (NetworkObject == null)
                    return null;

                return NetworkObject.GetComponent<NetworkedCupidon>();
            }
        }

        private void Awake()
        {
            Log.Init(Logger);

            Log.Info("Initializing Cupidon...");

            Log.Info("Registering language entries...");
            var languageService = LanguageService.Instance;
            languageService.AddEntry("CUPIDON_CUPIDON_MODE", "Activer les amoureux");
            languageService.HookLocalization();

            Log.Info("Hooking into game...");
            GameManagerPatch.Hook();
            GameSettingsUIPatch.Hook();

            On.GameManager.Spawned += GameManager_Spawned;

            Log.Info("Initialization done!");
        }

        private void Start()
        {
            Log.Info("Registering networked objetcs...");
            var prefab = new GameObject("NetworkCupidon");
            prefab.AddComponent<NetworkObject>();
            prefab.AddComponent<NetworkedCupidon>();
            NetworkObjectService.Instance.RegisterNetworkObject(prefab, $"{PLUGIN_GUID}.NetworkCupidon");
            DontDestroyOnLoad(prefab);
        }

        private void GameManager_Spawned(On.GameManager.orig_Spawned orig, GameManager self)
        {
            orig(self);

            if (self.Runner.SessionInfo.IsOpen)
            {
                Log.Info("Session is open.");

                if (self.Runner.IsServer)
                {
                    Log.Info("Retrieving network object...");
                    var netObjRef = NetworkObjectService.Instance.GetNetworkObject($"{PLUGIN_GUID}.NetworkCupidon");

                    Log.Info("Spawning networked object...");
                    NetworkObject = self.Runner.Spawn(netObjRef);
                }
                else
                {
                    Log.Info("Inchallah tu vas reçevoir un objet, un jour...");
                }
            }
            else
            {
                Log.Info("Session was closed :(");
            }
        }

        private void Update()
        {
            if (Cupidon == null) return;

            if (Input.GetKeyDown(KeyCode.P))
            {
                Log.Info("Trying to print CupidonMode...");
                Cupidon.PrintCupidonMode();
            }
        }

        private void OnDestroy()
        {
            GameManagerPatch.Unhook();
            GameSettingsUIPatch.Unhook();
        }
    }
}