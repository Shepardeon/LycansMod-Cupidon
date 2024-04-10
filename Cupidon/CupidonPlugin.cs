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

        public static readonly Color LoverColor = new Color(0.83f, 0.09f, 0.63f);

        internal static UIToggle? CupidonUI { get; set; }
        internal static UIText? CupidonText { get; set; }
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
            languageService.AddEntry("CUPIDON_LOVERS_VICTORY", "Amoureux unis à vie");
            languageService.AddEntry("CUPIDON_LOVER_ALLY", "Amoureux : {0}");
            languageService.HookLocalization();

            Log.Info("Hooking into game...");
            GameManagerPatch.Hook();
            GameSettingsUIPatch.Hook();
            GameStatePatch.Hook();
            LocalCameraHandlerPatch.Hook();

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

        private void OnDestroy()
        {
            GameManagerPatch.Unhook();
            GameSettingsUIPatch.Unhook();
            GameStatePatch.Unhook();
            LocalCameraHandlerPatch.Unhook();
        }

        public static void UpdateLoverText(string loverName)
        {
            if (CupidonText == null)
                return;

            CupidonText.LocalizeString.StringReference.Arguments = new object[1] { loverName };
            CupidonText.LocalizeString.RefreshString();
        }
    }
}