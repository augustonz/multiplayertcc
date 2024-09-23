using Unity.Netcode;
using UnityEngine;

public class NetworkSpawnHelper : NetworkBehaviour
{

    new bool enabled = true;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) enabled = false;
        GameController.Singleton.MyNetworkManager.networkSpawnHelper = this;
        DontDestroyOnLoad(this);
        base.OnNetworkSpawn();
    }
    public void SpawnPlayer(Vector3 position, ulong clientId) {
        if (!enabled) return;
        ulong clientIdToSpawn = clientId;
        
        // NetworkObject playerPrefab = PrefabsList.Singleton.GetNetworkPrefab("player");
        NetworkObject playerPrefab = PrefabsList.Singleton.GetNetworkPrefab("predictivePlayer");

        NetworkObject instantiated = Instantiate(playerPrefab, position, Quaternion.identity);

        instantiated.SpawnAsPlayerObject(clientIdToSpawn,true);
    }
}
