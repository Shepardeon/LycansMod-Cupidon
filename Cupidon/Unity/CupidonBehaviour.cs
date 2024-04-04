using Cupidon.Services;
using UnityEngine;
using Fusion;
using System;

namespace Cupidon.Unity
{
    [NetworkBehaviourWeaved(1)]
    internal class CupidonBehaviour : NetworkBehaviour
    {
        [SerializeField]
        [DefaultForProperty(nameof(CupidonMode), 0, 1)]
        private NetworkBool _CupidonMode = false;

        [Networked]
        [NetworkedWeaved(0, 1)]
        public unsafe NetworkBool CupidonMode
        {
            get
            {
                if (Ptr == null)
                {
                    throw new InvalidOperationException("Error when accessing GameManager.CupidonMode. Networked properties can only be accessed when Spawned() has been called.");
                }
                return *(NetworkBool*)(Ptr + 1);
            }
            private set
            {
                if (Ptr == null)
                {
                    throw new InvalidOperationException("Error when accessing GameManager.CupidonMode. Networked properties can only be accessed when Spawned() has been called.");
                }
                *(NetworkBool*)(Ptr + 0) = value;
            }
        }

        public UIToggle? CupidonToggle { get; set; }

        public void Awake()
        {
            Log.Info("Cupidon NetworkBehavior awakened!");
        }

        public void UpdateCupidonMode(bool cupidonMode)
        {
            //if (Runner.IsServer && GameManager.State.Current == GameState.EGameState.Pregame)
            //{
                Log.Info($"Cupidon mode is {cupidonMode}");
                CupidonMode = cupidonMode;
            //}
        }

        public override void CopyBackingFieldsToState(bool firstTime)
        {
            CupidonMode = _CupidonMode;
        }

        public override void CopyStateToBackingFields()
        {
            _CupidonMode = CupidonMode;
        }
    }
}