using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System;

public class Lobby : NetworkBehaviour
{
    LobbyUI UIController;
    NetworkVariable<LobbyData> lobbyData = new NetworkVariable<LobbyData>(LobbyData.Empty(),writePerm:NetworkVariableWritePermission.Server,readPerm:NetworkVariableReadPermission.Everyone);
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer) {
            LobbyData val = LobbyData.Empty();
            lobbyData.Value = val;
        } else  {
        }

        lobbyData.OnValueChanged += OnLobbyDataChanged;
        StartLobby();
        GameController.Singleton.StartedLobby();
        GameController.Singleton.lobby = this;        

        UIController = GetComponent<LobbyUI>();
        UpdateLobbyUI(lobbyData.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        lobbyData.OnValueChanged -= OnLobbyDataChanged;
    }

    void OnLobbyDataChanged(LobbyData prev,LobbyData curr) {
        UpdateLobbyUI(curr);
    }

    void UpdateLobbyUI(LobbyData lobbyData) {
        UIController.UpdateUI(lobbyData);
    }

    bool IsLobbyFull() {
        return lobbyData.Value.PlayersInLobby==6;
    }

    bool IsLobbyEmpty() {
        return lobbyData.Value.PlayersInLobby==0;
    }

    public void LeaveLobby() {
        LeaveLobbyRpc();
    }

    public void ChangeReadyStatus() {
        if (lobbyData.Value.playerCaseDatas.Any(caseData=>caseData.playerName.Value.Trim() == "" && caseData.clientId==GameController.Singleton.MyNetworkManager.LocalClientId)) {
            UIController.SetPopupText("Can't ready, invalid name");
            return;
        }
        ChangeReadyStatusRpc();
    }

    public void ChangePlayerName(string newName) {
        if (lobbyData.Value.playerCaseDatas.Any(caseData=>caseData.playerName.Value==newName && caseData.clientId!=GameController.Singleton.MyNetworkManager.LocalClientId)) {
            UIController.SetPopupText("Invalid name, no repeat names");
            UpdateLobbyUI(lobbyData.Value);
            return;
        }
        if (newName.Trim()=="") {
            UIController.SetPopupText("Invalid name, no blank names");
            UpdateLobbyUI(lobbyData.Value);
            return;
        }
        ChangePlayerNameRpc(newName);
    }

    public void ChangePlayerColor(string newColor) {
        Debug.Log($"Changing color to {newColor}");
        ChangePlayerColorRpc(newColor);
    }

    public void StartMatch() {
        // Commented for testing purposes
        // if (lobbyData.Value.PlayersInLobby<=1) {
        //     UIController.SetPopupText("Can't start match, not enough players");
        //     return; 
        // }
        if (lobbyData.Value.PlayersInLobby!=lobbyData.Value.ReadyPlayers) {
            UIController.SetPopupText("Can't start match, players aren't ready");
            return;
        }
        StartMatchRpc();
    }

    [Rpc(SendTo.Server)]
    public void LeaveLobbyRpc(RpcParams rpcParams = default) {
        RemoveFromLobby(rpcParams.Receive.SenderClientId);
        GameController.Singleton.MyNetworkManager.DisconnectClient(rpcParams.Receive.SenderClientId);
    }

    [Rpc(SendTo.Server)]
    public void ChangeReadyStatusRpc(RpcParams rpcParams = default) {
        LobbyData val = GetCopy(lobbyData.Value);
        val.changeReadyStatus(out val,rpcParams.Receive.SenderClientId);
        lobbyData.Value = val;
    }

    [Rpc(SendTo.Server)]
    public void ChangePlayerNameRpc(string newName,RpcParams rpcParams = default) {
        LobbyData val = GetCopy(lobbyData.Value);
        val.changePlayerCaseName(out val,rpcParams.Receive.SenderClientId,newName);
        lobbyData.Value = val;
    }

    [Rpc(SendTo.Server)]
    public void ChangePlayerColorRpc(string newColor,RpcParams rpcParams = default) {
        LobbyData val = GetCopy(lobbyData.Value);
        val.changePlayerColor(out val,rpcParams.Receive.SenderClientId,newColor);
        lobbyData.Value = val;
    }

    [Rpc(SendTo.Server)]
    public void StartMatchRpc() {
        GameController.Singleton.StartMatch(lobbyData.Value);
    }

    public void AddToPlayersInLobby(ulong clientId) {
        if (IsLobbyFull()) {
            Debug.LogError("Can't enter lobby, lobby is full");
            return;
        }
        LobbyData val = GetCopy(lobbyData.Value);
        val.clientIdsInLobby[Array.FindIndex(val.clientIdsInLobby,val=>val==0)] = clientId;
        val.addClientToPlayerCaseDatas(out val,clientId);
        lobbyData.Value = val;
    }

    public void RemoveFromLobby(ulong clientId) {
        if (IsLobbyEmpty()) {
            Debug.LogError("Can't exit lobby, lobby is empty");
            return;
        }
        LobbyData val = GetCopy(lobbyData.Value);
        val.clientIdsInLobby[Array.FindIndex(val.clientIdsInLobby,val=>val==clientId)] = 0;
        val.removeClientFromPlayerCaseDatas(out val,clientId);
        lobbyData.Value = val;
    }

    public void StartLobby() {
    }

    LobbyData GetCopy(LobbyData original) {
        LobbyData newCopy = LobbyData.Empty();

        for (int i = 0; i < original.clientIdsInLobby.Length; i++)
        {
            newCopy.clientIdsInLobby[i] = original.clientIdsInLobby[i];
        }

        for (int i = 0; i < original.playerCaseDatas.Length; i++)
        {
            newCopy.playerCaseDatas[i] = new PlayerCaseData(
                original.playerCaseDatas[i].clientId,
                original.playerCaseDatas[i].playerName.Value,
                original.playerCaseDatas[i].isReady,
                original.playerCaseDatas[i].playerColor.Value
            );
        }

        return newCopy;
    }


}
