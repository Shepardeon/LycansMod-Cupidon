
using Fusion;
using System;
using UnityEngine;
using UnityEngine.Scripting;

public class patch_GameManager : GameManager
{
    [Networked]
    bool CupidonMode { get; set; }

    public void UpdateCupidonMode(bool value)
    {
        if (Runner.IsServer && State.Current == GameState.EGameState.Pregame)
        {
            CupidonMode = value;
        }
        Debug.Log($"Cupidon mode is {CupidonMode}");
        Rpc_Testing(Runner, CupidonMode);
    }

    [Rpc]
    public unsafe static void Rpc_Testing(NetworkRunner runner, bool cupidon)
    {
        Debug.Log($"InvokeRpc is {NetworkBehaviourUtils.InvokeRpc}");

        if (NetworkBehaviourUtils.InvokeRpc)
        {
            NetworkBehaviourUtils.InvokeRpc = false;
        }
        else
        {
            if ((object)runner == null)
            {
                throw new ArgumentNullException("runner");
            }
            if (runner.Stage == SimulationStages.Resimulate)
            {
                return;
            }
            if (runner.HasAnyActiveConnections())
            {
                int capacity = 8;
                capacity += 4;
                SimulationMessage* ptr = SimulationMessage.Allocate(runner.Simulation, capacity);
                byte* data = SimulationMessage.GetData(ptr);
                int offset = RpcHeader.Write(RpcHeader.Create(NetworkBehaviourUtils.GetRpcStaticIndexOrThrow("System.Void GameManager::Rpc_Testing(Fusion.NetworkRunner,System.Boolean)")), data);
                ReadWriteUtilsForWeaver.WriteBoolean((int*)(data + offset), cupidon);
                offset += 4;
                ptr->Offset = offset * 8;
                ptr->SetStatic();
                runner.SendRpc(ptr);
            }
        }
        Debug.Log($"My cupidon is {cupidon}");
    }

    [NetworkRpcStaticWeavedInvoker("System.Void GameManager::Rpc_Testing(Fusion.NetworkRunner,System.Boolean)")]
    [Preserve]
    protected unsafe static void Rpc_Testing_Invoker(NetworkRunner runner, SimulationMessage* message)
    {
        byte* data = SimulationMessage.GetData(message);
        int num = (RpcHeader.ReadSize(data) + 3) & -4;
        bool num2 = ReadWriteUtilsForWeaver.ReadBoolean((int*)(data + num));
        num += 4;
        bool cupidon = num2;
        NetworkBehaviourUtils.InvokeRpc = true;
        Rpc_Testing(runner, cupidon);
    }
}