
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using Cinemachine;

public class CameraController: MonoBehaviour {

    [SerializeField] bool isOnline = true;
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    CinemachineBrain brain;

    public void Start()
    {
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

    public void FollowPlayerOffline() {
        virtualCamera.Follow =  GameObject.FindGameObjectWithTag("Player").transform;
    }
    void Update() {
        if (isOnline) return;
        if (virtualCamera.Follow==null) FollowPlayer(0);
    }
}