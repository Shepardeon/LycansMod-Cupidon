using BepInEx;
using Cupidon.Patchs;
using Cupidon.Services;

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

        private void Awake()
        {
            Log.Init(Logger);

            Log.Info("Initializing Cupidon...");
            
            var languageService = LanguageService.Instance;
            languageService.AddEntry("CUPIDON_CUPIDON_MODE", "Activer les amoureux");
            languageService.HookLocalization();

            GameManagerPatch.Hook();
            GameSettingsUIPatch.Hook();

            Log.Info("Initialization done!");
        }

        private void OnDestroy()
        {
            GameManagerPatch.Unhook();
            GameSettingsUIPatch.Unhook();
        }
    }
}