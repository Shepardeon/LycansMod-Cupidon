using Fusion;
using System;
using UnityEngine;

namespace Cupidon.Unity
{
    [NetworkBehaviourWeaved(1)]
    internal class NetworkedCupidon : NetworkBehaviour
    {
        [SerializeField]
        [DefaultForProperty(nameof(CupidonMode), 0, 1)]
        private NetworkBool _CupidonMode = false;

        private void Awake()
        {
            Log.Info("NetworkedObject Awake");
        }

        public override void Spawned()
        {
            Log.Info("NetworkedObject Spawned");
            if (Runner.IsClient)
            {
                Log.Info("Tu as eu ton bel objet bébou <3");
                CupidonPlugin.NetworkObject = GetComponent<NetworkObject>();
            }
        }

        public void OnDestroy()
        {
            Log.Info("NetworkedObject Destroyed");
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            Log.Info("NetworkedObject Despawned");
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
                    throw new InvalidOperationException("Error when accessing NetworkedCupidon.NetValue. Networked properties can only be accessed when Spawned() has been called.");
                }
                return *(NetworkBool*)(Ptr + 0);
            }
            private set
            {
                if (Ptr == null)
                {
                    throw new InvalidOperationException("Error when accessing NetworkedCupidon.NetValue. Networked properties can only be accessed when Spawned() has been called.");
                }
                *(NetworkBool*)(Ptr + 0) = value;
            }
        }

        public void UpdateCupidonMode(bool value)
        {
            if (Runner.IsServer && GameManager.State.Current == GameState.EGameState.Pregame)
            {
                CupidonMode = value;
            }
        }

        public void PrintCupidonMode()
        {
            Log.Info($"CupidonMode: {CupidonMode}");
        }
    }
}