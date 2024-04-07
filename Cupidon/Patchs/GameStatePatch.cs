using Cupidon.Extensions;
using Fusion;
using static GameState;

namespace Cupidon.Patchs
{
    internal class GameStatePatch
    {
        public static void Hook()
        {
            On.GameState.Spawned += GameState_Spawned;
        }

        public static void Unhook()
        {
            On.GameState.Spawned -= GameState_Spawned;
        }

        private static void GameState_Spawned(On.GameState.orig_Spawned orig, GameState self)
        {
            orig(self);

            var onPlayEnter = self.StateMachine[EGameState.Play].onEnter;
            self.StateMachine[EGameState.Play].onEnter = (EGameState previousState) =>
            {
                if (previousState == EGameState.Pregame)
                {
                    CupidonPlugin.Cupidon?.ResetLovers();
                    CupidonPlugin.Cupidon?.UpdateLoversWin(false);

                    onPlayEnter(previousState);

                    CupidonPlugin.Cupidon?.InitLovers();
                }
                else
                {
                    onPlayEnter(previousState);
                }
            };

            var onMeetingEnter = self.StateMachine[EGameState.Meeting].onEnter;
            self.StateMachine[EGameState.Meeting].onEnter = (EGameState previousState) =>
            {
                LoversSuicide(self.Runner);
                onMeetingEnter(previousState);
            };

            var onMeetingExit = self.StateMachine[EGameState.Meeting].onExit;
            self.StateMachine[EGameState.Meeting].onExit = (EGameState previousState) =>
            {
                LoversSuicide(self.Runner);
                onMeetingExit(previousState);
            };
        }

        private static void LoversSuicide(NetworkRunner runner)
        {
            var localPlayer = PlayerController.Local;

            if (!localPlayer.IsDead && localPlayer.IsLover() && PlayerRegistry.Any(p => p.IsLover() && p.IsDead))
            {
                GameManager.Rpc_BroadcastFollowSound(runner, "PUNCH", localPlayer.transform.position, 50f, 0.8f);
                localPlayer.Rpc_Kill(PlayerRef.None);
                GameManager.Rpc_DisplayDeadPlayers(runner);
            }
        }
    }
}