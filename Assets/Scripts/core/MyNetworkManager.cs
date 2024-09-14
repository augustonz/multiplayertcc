using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Multiplay;
using Unity.Services.Core;
using System;
public class MyNetworkManager : NetworkManager
{

    public NetworkSpawnHelper networkSpawnHelper { get; set; }

    ushort _clientConnectPort;
    string _clientConnectIp;
    #if DEDICATED_SERVER
    private const ushort k_DefaultMaxPlayers = 6;
	private const string k_DefaultServerName = "Test Server";
	private const string k_DefaultGameType = "Classic Race";
	private const string k_DefaultBuildId = "MyBuildId";
	private const string k_DefaultMap = "MyMap";
	private IServerQueryHandler m_ServerQueryHandler;
    #endif

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
                        #if DEDICATED_SERVER
                        PlayerCountChanged(ConnectedClientsList.Count);
                        #endif
                        break;
                    case ConnectionEvent.ClientDisconnected:
                        #if DEDICATED_SERVER
                        PlayerCountChanged(ConnectedClientsList.Count);
                        #endif
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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    new public async void StartServer() {
        RegisterServerCallbacks();
        bool isListening = base.StartServer();        
        GameController.Singleton.MySceneManager.ChangeSceneRpc("Lobby");
        Debug.Log($"{(isListening ? "Server is listening" : "Error, server is not listening")}");
        #if DEDICATED_SERVER
        try 
            {
                await UnityServices.InitializeAsync();
            }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        StartSQP();

        var serverConfig = MultiplayService.Instance.ServerConfig;
        Debug.Log($"Server ID[{serverConfig.ServerId}]");
        Debug.Log($"AllocationID[{serverConfig.AllocationId}]");
        Debug.Log($"Port[{serverConfig.Port}]");
        Debug.Log($"QueryPort[{serverConfig.QueryPort}");
        Debug.Log($"LogDirectory[{serverConfig.ServerLogDirectory}]");

        #endif

    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously


    public void SetClientConnectIp(string clientConnectIp) {
        _clientConnectIp = clientConnectIp;
    }
    public void SetClientConnectPort(string clientConnectPort) {
        if (!ushort.TryParse(clientConnectPort, out _clientConnectPort)) {
            Debug.LogError("Invalid port number.");
        }
    }
    
    public void JoinServer() {
        if (_clientConnectIp!="" && _clientConnectPort!=0) {
            GetComponent<UnityTransport>().SetConnectionData(_clientConnectIp,_clientConnectPort);
        }

        RegisterClientCallbacks();
        base.StartClient();
    }

    public void Shutdown() {
        base.Shutdown();
    }

    void AllClientsLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) {
        if (sceneName!="Menu" && sceneName!="Lobby" && loadSceneMode==LoadSceneMode.Single) {
            GameController.Singleton.MySceneManager.AddSceneRpc("MatchGUI");
            return;
        }
        if (sceneName=="MatchGUI" && loadSceneMode==LoadSceneMode.Additive) {
            Debug.Log("All clients are ready to start the match!");
            GameController.Singleton.match.PrepareMatch();
            return;
        }
        
    }

    void ServerQuit(bool shouldStopMessages) {
        if (GameController.Singleton.MySceneManager.CurrentScene.name!="Menu") {
            GameController.Singleton.MySceneManager.ChangeSceneRpc("Menu");
        }
    }

	void Update()
	{
        #if DEDICATED_SERVER
        if (m_ServerQueryHandler!=null) {
		    m_ServerQueryHandler.UpdateServerCheck();
        }
        #endif
	}

    #if DEDICATED_SERVER
    private async void StartSQP()
	{
		m_ServerQueryHandler = await MultiplayService.Instance.StartServerQueryHandlerAsync(k_DefaultMaxPlayers, k_DefaultServerName, k_DefaultGameType, k_DefaultBuildId, k_DefaultMap);
	}

	public void ChangeQueryResponseValues(ushort maxPlayers, string serverName, string gameType, string buildId)
	{
		m_ServerQueryHandler.MaxPlayers = maxPlayers;
		m_ServerQueryHandler.ServerName = serverName;
		m_ServerQueryHandler.GameType = gameType;
		m_ServerQueryHandler.BuildId = buildId;
	}

	public void PlayerCountChanged(int newPlayerCount)
	{
		m_ServerQueryHandler.CurrentPlayers = (ushort)newPlayerCount;
	}
    #endif
}
