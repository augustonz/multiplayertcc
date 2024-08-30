
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using Cinemachine;

public class CameraController: NetworkBehaviour {

    [SerializeField] bool isOnline = true;
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    CinemachineBrain brain;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer) Destroy(this);
        Camera.main.TryGetComponent<CinemachineBrain>(out var brain);
        this.brain = brain;
    }


    public void FollowPlayer(ulong clientId) {
        if (!isOnline) {
            virtualCamera.Follow =  GameObject.FindGameObjectWithTag("Player").transform;
            return;
        }

        if (!MyNetworkManager.Singleton.ConnectedClientsIds.Contains(clientId)) {
            Debug.LogError("Can't follow that player, it doesn't exist.");
            return;
        }

        foreach (var item in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (item.GetComponent<NetworkObject>().OwnerClientId==clientId) virtualCamera.Follow = item.transform;
        } 
    }

    void Update() {
        if (isOnline) return;
        if (virtualCamera.Follow==null) FollowPlayer(0);
    }
}