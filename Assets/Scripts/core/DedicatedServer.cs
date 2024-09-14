using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ServerStartUp : MonoBehaviour {

    ushort serverPort = 7777;
    string internalServerIp = "0.0.0.0";
    int targetFPS = 60;

    void Start() {
        bool server = false;
        var args = System.Environment.GetCommandLineArgs();

        for (int i=0;i<args.Length;i++) {
            if (args[i]=="-dedicatedServer") {
                server = true;
            } else if (args[i]=="-port" && (i + 1 < args.Length)) {
                serverPort = ushort.Parse(args[i+1]);
            }
        }

        if (server) {
            StartCoroutine(nameof(StartServer));
        }
    }

    IEnumerator StartServer() {
        yield return new WaitForSeconds(3);
        Debug.Log("This is a dedicated server instance");
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(internalServerIp,serverPort,"0.0.0.0"); 
        GameController.Singleton.MyNetworkManager.StartServer();
    }
}