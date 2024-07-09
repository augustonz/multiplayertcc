using Unity.Netcode;
using UnityEngine;

public class MatchController : NetworkBehaviour
{

    MatchBuilder gameController;

    override public void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        if (IsClient) {
            // GameController.Singleton.MySceneManager.AddExitSceneCallback("SampleScene",ExitMatch);
        } else {
            GameController.Singleton.MySceneManager.AddSceneRpc("matchGUI");
        };
        
    }

}
