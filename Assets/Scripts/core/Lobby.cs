using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Lobby : NetworkBehaviour
{
    LobbyUI UIController;
    LobbyData lobbyData;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        StartLobby();
        GameController.Singleton.StartedLobby();
        GameController.Singleton.lobby = this;        

        UIController = GetComponent<LobbyUI>();
        lobbyData = new LobbyData();

        UpdateLobbyUI(lobbyData);
    }

    void UpdateLobbyUI(LobbyData lobbyData) {
        Debug.Log($"Updated UI to this lobby data: {lobbyData}");
        UIController.UpdateUI(lobbyData);
    }

    bool IsLobbyFull() {
        return lobbyData.PlayersInLobby==6;
    }

    bool IsLobbyEmpty() {
        return lobbyData.PlayersInLobby==0;
    }

    public void AddToPlayersInLobby(ulong clientId) {
        if (IsLobbyFull()) {
            Debug.LogError("Can't entre lobby, lobby is full");
            return;
        }
        lobbyData.clientIdsInLobby.Add(clientId);
        lobbyData.addClientToPlayerCaseDatas(clientId);
        UpdateLobbyUI(lobbyData);
    }

    public void RemoveFromLobby(ulong clientId) {
        if (IsLobbyEmpty()) {
            Debug.LogError("Can't exit lobby, lobby is empty");
            return;
        }
        lobbyData.clientIdsInLobby.Remove(clientId);
        lobbyData.removeClientFromPlayerCaseDatas(clientId);
        UpdateLobbyUI(lobbyData);
    }

    public void StartLobby() {
        Debug.Log("Lobby started!");
    }


}
