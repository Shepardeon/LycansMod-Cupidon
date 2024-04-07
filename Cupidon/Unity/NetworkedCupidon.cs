using Cupidon.Extensions;
using Fusion;
using Helpers.Collections;
using System;
using System.Linq;
using UnityEngine;

namespace Cupidon.Unity
{
    [NetworkBehaviourWeaved(11)]
    internal class NetworkedCupidon : NetworkBehaviour
    {
        [DefaultForProperty(nameof(CupidonMode), 0, 1)]
        [SerializeField]
        private bool _CupidonMode;

        [DefaultForProperty(nameof(LoversWin), 1, 1)]
        [SerializeField]
        private bool _LoversWin;

        [SerializeField]
        [DefaultForProperty(nameof(CupidonLovers), 2, 9)]
        private PlayerRef[] _CupidonLovers;

        private void Awake()
        {
            Log.Debug("NetworkedObject Awake");
        }

        public override void Spawned()
        {
            Log.Debug("NetworkedObject Spawned");
            if (Runner.IsClient)
            {
                CupidonPlugin.NetworkObject = GetComponent<NetworkObject>();
            }
        }

        public void OnDestroy()
        {
            Log.Debug("NetworkedObject Destroyed");
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            Log.Debug("NetworkedObject Despawned");
            Destroy(gameObject);
        }

        [Networked]
        [NetworkedWeaved(0, 1)]
        public unsafe bool CupidonMode
        {
            get
            {
                if (Ptr == null)
                {
                    throw new InvalidOperationException("Error when accessing NetworkedCupidon.CupidonMode. Networked properties can only be accessed when Spawned() has been called.");
                }
                return ReadWriteUtilsForWeaver.ReadBoolean((int*)((byte*)Ptr + 0));
            }
            private set
            {
                if (Ptr == null)
                {
                    throw new InvalidOperationException("Error when accessing NetworkedCupidon.CupidonMode. Networked properties can only be accessed when Spawned() has been called.");
                }
                ReadWriteUtilsForWeaver.WriteBoolean((int*)((byte*)Ptr + 0), value);
            }
        }

        [NetworkedWeaved(1, 1)]
        [Networked]
        public unsafe bool LoversWin
        {
            get
            {
                if (Ptr == null)
                {
                    throw new InvalidOperationException("Error when accessing NetworkedCupidon.LoversWin. Networked properties can only be accessed when Spawned() has been called.");
                }
                return ReadWriteUtilsForWeaver.ReadBoolean(Ptr + 1);
            }
            private set
            {
                if (Ptr == null)
                {
                    throw new InvalidOperationException("Error when accessing NetworkedCupidon.LoversWin. Networked properties can only be accessed when Spawned() has been called.");
                }
                ReadWriteUtilsForWeaver.WriteBoolean(Ptr + 1, value);
            }
        }

        [Networked]
        [Capacity(2)]
        [NetworkedWeaved(2, 9)]
        public unsafe NetworkLinkedList<PlayerRef> CupidonLovers
        {
            get
            {
                if (Ptr == null)
                {
                    throw new InvalidOperationException("Error when accessing NetworkedCupidon.CupidonLovers. Networked properties can only be accessed when Spawned() has been called.");
                }
                return new NetworkLinkedList<PlayerRef>((byte*)(Ptr + 2), 2, PlayerRefReaderWriter.GetInstance());
            }
        }

        public void UpdateCupidonMode(bool value)
        {
            if (Runner.IsServer && GameManager.State.Current == GameState.EGameState.Pregame)
            {
                CupidonMode = value;
                Log.Debug($"Cupidon value update: {CupidonMode}");
            }
        }

        public void UpdateLoversWin(bool value)
        {
            if (Runner.IsServer)
            {
                LoversWin = value;
            }
        }

        public bool CheckLoversSameTeam()
        {
            if (!CupidonMode || CupidonLovers.Count == 0)
                return true;

            var p1 = PlayerRegistry.GetPlayer(CupidonLovers.ElementAt(0));
            var p2 = PlayerRegistry.GetPlayer(CupidonLovers.ElementAt(1));

            return !(p1.IsWolf ^ p2.IsWolf);
        }

        public bool CheckLover(PlayerRef player)
        {
            return CupidonLovers.Contains(player);
        }

        public void InitLovers()
        {
            if (!CupidonMode)
            {
                Log.Debug("Cupidon mode disabled, skipping selection");
                return;
            }

            if (PlayerRegistry.Count < 2)
            {
                Log.Debug("Not enough player for Cupidon, skipping selection");
                return;
            }

            Log.Debug("Selecting lovers...");
            PlayerRegistry.GetRandom(2).ForEach(p => p.SetLover());
        }

        public void AddLover(PlayerRef player)
        {
            if (CupidonLovers.Contains(player))
            {
                Log.Warning("Tried to select the same player as lovers!");
                return;
            }

            CupidonLovers.Add(player);
        }

        public void ResetLovers()
        {
            CupidonLovers.Clear();
        }

        public override void CopyBackingFieldsToState(bool firstTime)
        {
            CupidonMode = _CupidonMode;
            LoversWin = _LoversWin;
            NetworkBehaviourUtils.InitializeNetworkList(CupidonLovers, _CupidonLovers, nameof(CupidonLovers));
        }

        public override void CopyStateToBackingFields()
        {
            _CupidonMode = CupidonMode;
            _LoversWin = LoversWin;
            NetworkBehaviourUtils.CopyFromNetworkList(CupidonLovers, ref _CupidonLovers);
        }
    }
}