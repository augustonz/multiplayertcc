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
    public void SpawnPlayerRpc(RpcParams rpcParams = default) {
        if (!enabled) return;
        NetworkObject playerPrefab = PrefabsList.Singleton.GetNetworkPrefab("player");
        
        NetworkObject instantiated = Instantiate(playerPrefab);
        instantiated.SpawnAsPlayerObject(rpcParams.Receive.SenderClientId);

        // NetworkManager.SpawnManager.InstantiateAndSpawn(playerPrefab,rpcParams.Receive.SenderClientId,false,true);
    }
}
