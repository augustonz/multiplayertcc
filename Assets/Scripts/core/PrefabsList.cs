using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PrefabsList : MonoBehaviour
{
    [SerializeField] NetworkObject playerPrefab;
    Dictionary<string,NetworkObject> prefabs = new Dictionary<string, NetworkObject>();
    static public PrefabsList Singleton;

    void Start() {
        if (Singleton!=null) Destroy(this);
        Singleton = this;
        prefabs = new Dictionary<string, NetworkObject>() {
            {"player",playerPrefab}
        };
    }

    public NetworkObject GetNetworkPrefab(string name) {
        return prefabs[name];
    }
}
