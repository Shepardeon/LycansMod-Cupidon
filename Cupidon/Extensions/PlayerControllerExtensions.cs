using System.Linq;

namespace Cupidon.Extensions
{
    internal static class PlayerControllerExtensions
    {
        public static bool IsLover(this PlayerController player)
        {
            if (CupidonPlugin.Cupidon == null || CupidonPlugin.Cupidon.CupidonLovers.Count == 0)
                return false;

            var playerRef = PlayerRegistry.Instance.ObjectByRef.FirstOrDefault(o => o.Value == player).Key;
            return CupidonPlugin.Cupidon.CheckLover(playerRef);
        }

        public static void SetLover(this PlayerController player)
        {
            if (CupidonPlugin.Cupidon == null || CupidonPlugin.Cupidon.CupidonLovers.Count >= 2)
            {
                Log.Error("Tried to add a new user to lovers but there can be only Two!");
                return;
            }

            var playerRef = PlayerRegistry.Instance.ObjectByRef.FirstOrDefault(o => o.Value == player).Key;
            CupidonPlugin.Cupidon.AddLover(playerRef);
            Log.Debug($"Added {player.PlayerData.Username} as lover");
        }
    }
}