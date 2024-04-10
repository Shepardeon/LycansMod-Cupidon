using Cupidon.Extensions;
using Cupidon.Services;
using Fusion;
using System;
using System.Linq;
using UnityEngine;

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

            CupidonPlugin.CupidonText = UIService.Instance.AddTextToMainUI("CUPIDON_LOVER_ALLY", CupidonPlugin.LoverColor);
            CupidonPlugin.CupidonText.TextGO.transform.position = new Vector3(5, 410, 0);
            CupidonPlugin.CupidonText.TextGO.transform.localScale = new Vector3(0.75f, 0.75f, 1);
            CupidonPlugin.CupidonText.TextGO.SetActive(false);
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

            var numWolves = playerAlive.Count(p => p.Role == PlayerController.PlayerRole.Wolf);
            var numLovers = playerAlive.Count(p => p.IsLover());

            var numWolfLovers = playerAlive.Count(p => p.IsLover() && p.Role == PlayerController.PlayerRole.Wolf);

            var numNonWolves = playerAlive.Count - numWolves;
            var numNonWolvesLovers = numLovers - numWolfLovers;

            Log.Debug($"Num player alive {playerAlive.Count}");
            Log.Debug($"Num lovers alive {numLovers}");
            Log.Debug($"Num lovers same team {CupidonPlugin.Cupidon.CheckLoversSameTeam()}");

            if (numLovers == 2)
            {
                if (!CupidonPlugin.Cupidon.CheckLoversSameTeam())
                {
                    if (numLovers == playerAlive.Count)
                    {
                        CupidonPlugin.Cupidon.UpdateLoversWin(true);
                        GameManager.Rpc_EndGame(self.Runner, wolfWin: false);
                    }
                    else if (numLovers >= numNonWolves && numWolves == 1)
                    {
                        CupidonPlugin.Cupidon.UpdateLoversWin(true);
                        GameManager.Rpc_EndGame(self.Runner, wolfWin: false);
                    }
                }
                else if (numWolves == 0)
                {
                    GameManager.Rpc_EndGame(self.Runner, wolfWin: false);
                }
                else if (numNonWolves == 0)
                {
                    GameManager.Rpc_EndGame(self.Runner, wolfWin: true);
                }
                else if (numWolves >= numNonWolves && GameManager.State.Current == GameState.EGameState.Meeting)
                {
                    GameManager.Rpc_EndGame(self.Runner, wolfWin: true);
                }
            }
            else
            {
                numWolves -= numWolfLovers;
                numNonWolves -= numNonWolvesLovers;

                if (numWolves <= 0)
                {
                    GameManager.Rpc_EndGame(self.Runner, wolfWin: false);
                }
                else if (numNonWolves <= 0)
                {
                    GameManager.Rpc_EndGame(self.Runner, wolfWin: true);
                }
                else if (numWolves >= numNonWolves && GameManager.State.Current == GameState.EGameState.Meeting)
                {
                    GameManager.Rpc_EndGame(self.Runner, wolfWin: true);
                }
            }
        }

        private unsafe static void GameManager_Rpc_EndGame(On.GameManager.orig_Rpc_EndGame orig, NetworkRunner runner, bool wolfWin)
        {
            Color villagerColor = GameUI.VillagerColor;
            Color wolfColor = GameUI.WolfColor;
            Color loverColor = CupidonPlugin.LoverColor;

            if (CupidonPlugin.Cupidon == null || !CupidonPlugin.Cupidon.CupidonMode || CupidonPlugin.Cupidon.CheckLoversSameTeam())
            {
                GameManager.Instance.gameUI.wolvesRecap.color = GameUI.WolfColor;
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

            bool isLover = PlayerController.Local.IsLover();
            bool isWolf = PlayerController.Local.Role == PlayerController.PlayerRole.Wolf;
            Color color;
            string clip;
            string translateKey;

            if (CupidonPlugin.Cupidon.LoversWin)
            {
                color = isLover ? villagerColor : wolfColor;
                clip = isLover ? "VICTORY" : "DEFEAT";

                string loversName = string.Join(" / ", PlayerRegistry.Where(x => x.IsLover()).Select(x => x.PlayerData.Username));
                GameManager.Instance.gameUI.wolvesRecap.color = CupidonPlugin.LoverColor;

                translateKey = "CUPIDON_LOVERS_VICTORY";
                GameManager.Instance.gameUI.UpdateWolvesRecap(loversName);
            }
            else
            {
                GameManager.Instance.gameUI.wolvesRecap.color = GameUI.WolfColor;

                if (wolfWin)
                {
                    color = !isLover && isWolf ? villagerColor : wolfColor;
                    clip = !isLover && isWolf ? "VICTORY" : "DEFEAT";
                }
                else
                {
                    color = !isLover && !isWolf ? villagerColor : wolfColor;
                    clip = !isLover && !isWolf ? "VICTORY" : "DEFEAT";
                }

                translateKey = (wolfWin ? "UI_WOLVES_WIN" : "UI_VILLAGERS_WIN");
            }

            AudioManager.Play(clip, AudioManager.MixerTarget.SFX, 0.5f);
            GameManager.Instance.gameUI.UpdateTransitionText(translateKey, color);
            GameManager.Instance.gameUI.ShowWolvesRecap(active: true);
            GameManager.Instance.gameUI.StartFade(fadeIn: true);
        }
    }
}