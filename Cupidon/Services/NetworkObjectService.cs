using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cupidon.Services
{
    internal class NetworkObjectService
    {
        public static NetworkObjectService Instance
        {
            get
            {
                _instance ??= new NetworkObjectService();
                return _instance;
            }
        }

        private NetworkObjectService()
        {
            _prefabs = new();
        }

        private static NetworkObjectService? _instance;
        private Dictionary<string, NetworkPrefabId> _prefabs;

        public void RegisterNetworkObject(GameObject prefab, string uniqueKey)
        {
            if (!prefab.TryGetComponent<NetworkObject>(out var netObj))
            {
                throw new MissingComponentException("Prefab is missing 'NetworkObject' component!");
            }

            netObj.NetworkedBehaviours = prefab.GetComponents<NetworkBehaviour>();
            netObj.NetworkGuid = new NetworkObjectGuid(Guid.NewGuid().ToString());

            var source = new NetworkPrefabSourceStatic()
            {
                PrefabReference = netObj
            };

            if (!NetworkProjectConfig.Global.PrefabTable.TryAdd(netObj.NetworkGuid, source, out var id))
            {
                throw new ApplicationException("Could not add prefab to registered prefabs");
            }

            if (_prefabs.ContainsKey(uniqueKey))
            {
                Log.Warning($"Key '{uniqueKey}' existed alreadly and was replaced!");
                _prefabs[uniqueKey] = id;
                return;
            }

            _prefabs.Add(uniqueKey, id);
        }

        public NetworkPrefabId GetNetworkObject(string uniqueKey)
        {
            if (_prefabs.TryGetValue(uniqueKey, out var result))
            {
                return result;
            }

            throw new KeyNotFoundException($"No prefab registered with '{uniqueKey}' as identifier!");
        }
    }
}