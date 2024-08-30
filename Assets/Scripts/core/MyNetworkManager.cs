using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Linq;
using System;

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
            SceneManager.OnLoadEventCompleted += AllClientsLoaded;
        };
    }

    void RegisterCommonCallbacks() {
        OnConnectionEvent += (manager,eventData) => {
            if (IsClient) {
                switch (eventData.EventType) {
                    case ConnectionEvent.ClientDisconnected:
                        SceneManager.LoadScene("Menu",LoadSceneMode.Single);
                        break;
                    default:
                        Debug.Log($"Unhandled {eventData.EventType} message received.");
                        break;
                }
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
        GameController.Singleton.MySceneManager.ChangeSceneRpc("Lobby");
        Debug.Log($"{(isListening ? "Server is listening" : "Error, server is not listening")}");
    }

    public void JoinServer() {
        RegisterClientCallbacks();
        base.StartClient();
    }

    public void SpawnPlayer(Vector3 position) {
        networkSpawnHelper.SpawnPlayerRpc(position);
    }

    public void Shutdown() {
        base.Shutdown();
    }

    void AllClientsLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) {
        if (sceneName=="SampleScene" && loadSceneMode==LoadSceneMode.Single) {
            GameController.Singleton.MySceneManager.AddSceneRpc("MatchGUI");
            return;
        }
        if (sceneName=="MatchGUI" && loadSceneMode==LoadSceneMode.Additive) {
            Debug.Log("All clients started the match!");
            return;
        }
        
    }

    void ServerQuit(bool shouldStopMessages) {
        if (GameController.Singleton.MySceneManager.CurrentScene.name!="Menu") {
            GameController.Singleton.MySceneManager.ChangeSceneRpc("Menu");
        }
    }
}
