using System;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Networking
{
    public sealed class NetworkWorldSpawner : MonoBehaviour
    {
        [Serializable]
        public struct Entry
        {
            public NetworkObject Prefab;
            public Transform Location;
            public bool DevelopmentOnly;
        }

        [SerializeField] private NetworkManager _manager;
        [SerializeField] private Entry[] _entries;
        private void Awake() => _manager.OnServerStarted += Spawn;
        private void Spawn()
        {
            foreach (var entry in _entries)
            {
                if (entry.DevelopmentOnly && !Application.isEditor && !Debug.isDebugBuild) continue;
                var instance = Instantiate(entry.Prefab, entry.Location.position, entry.Location.rotation);
                instance.Spawn();
            }
        }
        private void OnDestroy()
        {
            if (_manager != null) _manager.OnServerStarted -= Spawn;
        }
    }
}
