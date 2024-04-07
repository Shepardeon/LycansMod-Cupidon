using Cupidon.Extensions;
using Cupidon.Services;
using Fusion;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cupidon.Patchs
{
    internal class GameManagerPatch
    {
        public static void Hook()
        {
            On.GameManager.Start += GameManager_Start;
            On.GameManager.Spawned += GameManager_Spawned;
            On.GameManager.CheckForEndGame += GameManager_CheckForEndGame;
            On.GameManager.Rpc_EndGame += GameManager_Rpc_EndGame;
        }

        public static void Unhook()
        {
            On.GameManager.Start -= GameManager_Start;
            On.GameManager.Spawned -= GameManager_Spawned;
            On.GameManager.CheckForEndGame -= GameManager_CheckForEndGame;
            On.GameManager.Rpc_EndGame -= GameManager_Rpc_EndGame;
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

        private static void GameManager_CheckForEndGame(On.GameManager.orig_CheckForEndGame orig, GameManager self)
        {
            if (CupidonPlugin.Cupidon != null)
            {
                Log.Debug($"Cupidon mode : {CupidonPlugin.Cupidon.CupidonMode}");
            }

            // Si en battle royale ou sans le mode cupidon, on effectue la routine de victoire habituelle
            if (CupidonPlugin.Cupidon == null || !CupidonPlugin.Cupidon.CupidonMode || self.BattleRoyale)
            {
                Log.Debug($"Cupidon null : {CupidonPlugin.Cupidon == null}");
                Log.Debug($"Cupidon mode : {CupidonPlugin.Cupidon?.CupidonMode ?? false}");
                Log.Debug($"Battle royale : {self.BattleRoyale}");

                orig(self);
                return;
            }

            if (!self.IsStarted || self.IsFinished)
            {
                return;
            }

            var playerAlive = PlayerRegistry.Where(p => !p.IsDead).ToList();
            var lovers = playerAlive.Where(p => p.IsLover()).ToList();

            Log.Debug($"Num player alive {playerAlive.Count}");
            Log.Debug($"Num lovers alive {lovers.Count}");
            Log.Debug($"Num lovers same team {CupidonPlugin.Cupidon.CheckLoversSameTeam()}");

            // Les amoureux sont les seuls gagnants si :
            // - Ce sont les deux derniers survivants
            // - Les survivants sont un loup et un villageois
            // TODO : Revoir potentiellement les règles de victoire
            if (lovers.Count == 2 && playerAlive.Count == lovers.Count && !CupidonPlugin.Cupidon.CheckLoversSameTeam())
            {
                CupidonPlugin.Cupidon.UpdateLoversWin(true);
                GameManager.Rpc_EndGame(self.Runner, wolfWin: false);
            }

            // Dans le cas contraire, on effectue la routine habituelle
            orig(self);
        }

        private unsafe static void GameManager_Rpc_EndGame(On.GameManager.orig_Rpc_EndGame orig, NetworkRunner runner, bool wolfWin)
        {
            if (CupidonPlugin.Cupidon == null || !CupidonPlugin.Cupidon.CupidonMode || !CupidonPlugin.Cupidon.LoversWin)
            {
                orig(runner, wolfWin);
                return;
            }

            #region Required Boilerplate

            if (NetworkBehaviourUtils.InvokeRpc)
            {
                NetworkBehaviourUtils.InvokeRpc = false;
            }
            else
            {
                if (runner == null)
                {
                    throw new ArgumentNullException("runner");
                }
                if (runner.Stage == SimulationStages.Resimulate)
                {
                    return;
                }
                if (runner.HasAnyActiveConnections())
                {
                    int num = 8;
                    num += 4;
                    SimulationMessage* ptr = SimulationMessage.Allocate(runner.Simulation, num);
                    byte* data = SimulationMessage.GetData(ptr);
                    int num2 = RpcHeader.Write(RpcHeader.Create(NetworkBehaviourUtils.GetRpcStaticIndexOrThrow("System.Void GameManager::Rpc_EndGame(Fusion.NetworkRunner,System.Boolean)")), data);
                    ReadWriteUtilsForWeaver.WriteBoolean((int*)(data + num2), wolfWin);
                    num2 += 4;
                    ptr->Offset = num2 * 8;
                    ptr->SetStatic();
                    runner.SendRpc(ptr);
                }
            }

            if (runner.IsServer)
            {
                GameManager.Instance.IsFinished = true;
                PlayerRegistry.ForEach(delegate (PlayerController pObj)
                {
                    pObj.CanMove = false;
                });
                GameManager.State.Server_DelaySetState(GameState.EGameState.EndGame, 3f);
            }
            if (!runner.IsPlayer)
            {
                return;
            }

            #endregion Required Boilerplate

            Color villagerColor = GameUI.VillagerColor;
            Color wolfColor = GameUI.WolfColor;
            bool isLover = PlayerController.Local.IsLover();

            Color color = isLover ? villagerColor : wolfColor;
            string clip = isLover ? "VICTORY" : "DEFEAT";

            AudioManager.Play(clip, AudioManager.MixerTarget.SFX, 0.5f);
            string translateKey = "CUPIDON_LOVERS_VICTORY";
            GameManager.Instance.gameUI.UpdateTransitionText(translateKey, color);
            GameManager.Instance.gameUI.ShowWolvesRecap(active: true);
            GameManager.Instance.gameUI.StartFade(fadeIn: true);
        }
    }
}