using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Lobby : NetworkBehaviour
{
    LobbyUI UIController;
    // LobbyData lobbyData;
    NetworkVariable<LobbyData> lobbyData = new NetworkVariable<LobbyData>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer) {
            lobbyData.Value = new LobbyData(true);
            GameController.Singleton.StartedLobby();
        } else  {
            lobbyData.OnValueChanged += OnLobbyDataChanged;
        }
        StartLobby();
        GameController.Singleton.lobby = this;        

        UIController = GetComponent<LobbyUI>();
        UpdateLobbyUI(lobbyData.Value);
    }

    void OnLobbyDataChanged(LobbyData prev,LobbyData curr) {
        UpdateLobbyUI(curr);
    }

    void UpdateLobbyUI(LobbyData lobbyData) {
        Debug.Log($"Updated UI to this lobby data: {lobbyData}");
        UIController.UpdateUI(lobbyData);
    }

    public void TestChangValue1() {
        LobbyData val = lobbyData.Value;
        val.clientIdsInLobby.Add(1);
        lobbyData.Value = val;
    }

    public void TestChangValue2() {
        lobbyData.Value.clientIdsInLobby.Add(2);
    }

    bool IsLobbyFull() {
        return lobbyData.Value.PlayersInLobby==6;
    }

    bool IsLobbyEmpty() {
        return lobbyData.Value.PlayersInLobby==0;
    }

    public void AddToPlayersInLobby(ulong clientId) {
        if (IsLobbyFull()) {
            Debug.LogError("Can't entre lobby, lobby is full");
            return;
        }
        LobbyData val = lobbyData.Value;
        val.clientIdsInLobby.Add(clientId);
        val.addClientToPlayerCaseDatas(clientId);
        lobbyData.Value = val;
        UpdateLobbyUI(val);
    }

    public void RemoveFromLobby(ulong clientId) {
        if (IsLobbyEmpty()) {
            Debug.LogError("Can't exit lobby, lobby is empty");
            return;
        }
        lobbyData.Value.clientIdsInLobby.Remove(clientId);
        lobbyData.Value.removeClientFromPlayerCaseDatas(clientId);
        UpdateLobbyUI(lobbyData.Value);
    }

    public void StartLobby() {
        if (IsServer) {
            Debug.Log("Lobby started on server!");
        } else {
            Debug.Log("Lobby started on client!");
        }
    }


}
