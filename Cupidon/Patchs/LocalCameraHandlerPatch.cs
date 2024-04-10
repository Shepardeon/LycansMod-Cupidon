using Cupidon.Extensions;
using System.Linq;

namespace Cupidon.Patchs
{
    internal class LocalCameraHandlerPatch
    {
        public static void Hook()
        {
            On.LocalCameraHandler.SwitchPov += LocalCameraHandler_SwitchPov;
        }

        public static void Unhook() 
        {
            On.LocalCameraHandler.SwitchPov -= LocalCameraHandler_SwitchPov;
        }

        private static void LocalCameraHandler_SwitchPov(On.LocalCameraHandler.orig_SwitchPov orig, LocalCameraHandler self, PlayerController spectatedPlayer)
        {
            orig(self, spectatedPlayer);

            if (self.PovPlayer != null && self.PovPlayer.IsLover())
            {
                var lover = PlayerRegistry.Where(p => p != self.PovPlayer && p.IsLover())
                    .Select(p => p.PlayerData.Username)
                    .FirstOrDefault();

                CupidonPlugin.UpdateLoverText(lover.ToString());
                CupidonPlugin.CupidonText?.TextGO.SetActive(true);
            }
            else
            {
                CupidonPlugin.CupidonText?.TextGO.SetActive(false);
            }
        }
    }
}