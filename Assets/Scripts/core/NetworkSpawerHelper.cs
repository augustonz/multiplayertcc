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
    [Rpc(SendTo.Server)]
    public void SpawnPlayerRpc(Vector3 position, ulong clientId = 100000, RpcParams rpcParams = default) {
        if (!enabled) return;
        ulong clientIdToSpawn = clientId;
        if (clientId ==100000) clientIdToSpawn = rpcParams.Receive.SenderClientId;
        
        NetworkObject playerPrefab = PrefabsList.Singleton.GetNetworkPrefab("player");

        NetworkObject instantiated = Instantiate(playerPrefab, position, Quaternion.identity);

        instantiated.SpawnAsPlayerObject(clientIdToSpawn);
    }
}
