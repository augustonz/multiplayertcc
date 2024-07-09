using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class MyNetworkManager : NetworkManager
{

    public NetworkSpawnHelper networkSpawnHelper { get; set; }

    void Start() {
        RegisterCommonCallbacks();
    }

    void RegisterServerCallbacks() {
        OnServerStarted += () => {
            if (!IsServer) return;
            GameController.Singleton.MySceneManager.AddEnterSceneCallback("Menu",GameController.Singleton.MyNetworkManager.Shutdown);
            GameController.Singleton.MySceneManager.ChangeSceneRpc("Lobby");
        };
    }

    void RegisterCommonCallbacks() {
        OnConnectionEvent += (manager,eventData) => {
            if (IsClient) {

            } else if (IsServer) {
                switch (eventData.EventType) {
                    case ConnectionEvent.ClientConnected:
                        GameController.Singleton.lobby.AddToPlayersInLobby(eventData.ClientId);
                        break;
                    default:
                        Debug.Log($"Unhandled {eventData.EventType} message received.");
                        break;
                }
            } else if (IsHost) {
                Debug.LogError("There shouldn't be a host.");
            }
        };
    }

    void RegisterClientCallbacks() {
        OnServerStopped += ServerQuit;
    }

    new public void StartServer() {
        RegisterServerCallbacks();
        bool isListening = base.StartServer();
        Debug.Log($"{(isListening ? "Server is listening" : "Error, server is not listening")}");
    }

    public void JoinServer() {
        RegisterClientCallbacks();
        base.StartClient();
    }

    public void SpawnPlayer() {
        networkSpawnHelper.SpawnPlayerRpc();
    }

    public void Shutdown() {
        base.Shutdown();
    }

    void ServerQuit(bool shouldStopMessages) {
        if (GameController.Singleton.MySceneManager.CurrentScene.name!="Menu") {
            GameController.Singleton.MySceneManager.ChangeSceneRpc("Menu");
        }
    }
}
