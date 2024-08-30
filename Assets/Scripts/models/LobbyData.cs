using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

public struct LobbyData : INetworkSerializable, System.IEquatable<LobbyData>
{
    public ulong[] clientIdsInLobby;
    public PlayerCaseData[] playerCaseDatas;
    public int PlayersInLobby => clientIdsInLobby.Where(id=>id!=0).Count();
    public int ReadyPlayers => playerCaseDatas.Where(data => data.isReady).Count();
    public bool IsLocalPlayerReady()
    {
        return playerCaseDatas.Any(data => data.clientId == GameController.Singleton.MyNetworkManager.LocalClientId && data.isReady);
    }

    public static LobbyData Empty() {
        LobbyData lobby;
        lobby.clientIdsInLobby = new ulong[6];
        lobby.playerCaseDatas = new PlayerCaseData[] {
            PlayerCaseData.Empty(),
            PlayerCaseData.Empty(),
            PlayerCaseData.Empty(),
            PlayerCaseData.Empty(),
            PlayerCaseData.Empty(),
            PlayerCaseData.Empty()
        };
        return lobby;
    }

    public void addClientToPlayerCaseDatas(out LobbyData lobbyData,ulong clientId)
    {
        int index = Array.FindIndex(playerCaseDatas,caseData=>caseData.clientId==0);
        PlayerCaseData interm = playerCaseDatas[index];
        interm.clientId = clientId;
        playerCaseDatas[index] = interm;
        lobbyData = this;
    }

    public void changeReadyStatus(out LobbyData lobbyData,ulong clientId)
    {
        int index = Array.FindIndex(playerCaseDatas,caseData=>caseData.clientId==clientId);
        PlayerCaseData interm = playerCaseDatas[index];
        interm.isReady = !interm.isReady;
        playerCaseDatas[index] = interm;
        lobbyData = this;
    }

    public void changePlayerCaseName(out LobbyData lobbyData,ulong clientId,string name)
    {
        int index = Array.FindIndex(playerCaseDatas,caseData=>caseData.clientId==clientId);
        PlayerCaseData interm = playerCaseDatas[index];
        interm.playerName = name;
        playerCaseDatas[index] = interm;
        lobbyData = this;
    }

        public void changePlayerColor(out LobbyData lobbyData,ulong clientId,string color)
    {
        int index = Array.FindIndex(playerCaseDatas,caseData=>caseData.clientId==clientId);
        PlayerCaseData interm = playerCaseDatas[index];
        interm.playerColor = color;
        playerCaseDatas[index] = interm;
        lobbyData = this;
    }

    public void removeClientFromPlayerCaseDatas(out LobbyData lobbyData,ulong clientId)
    {
        int index = Array.FindIndex(playerCaseDatas,caseData=>caseData.clientId==clientId);
        playerCaseDatas[index] = PlayerCaseData.Empty();
        lobbyData = this;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();

            int clientIdsCount;
            reader.ReadValueSafe(out clientIdsCount);
            List<ulong> clientsList = new List<ulong>();
            for (int i = 0; i < clientIdsCount; i++)
            {
                ulong newClientId;
                reader.ReadValueSafe(out newClientId);
                clientsList.Add(newClientId);
            }
            clientIdsInLobby = clientsList.ToArray();

            int playerCasesCount;
            reader.ReadValueSafe(out playerCasesCount);
            List<PlayerCaseData> playerCasesList = new List<PlayerCaseData>();
            for (int i = 0; i < playerCasesCount; i++)
            {
                PlayerCaseData newplayerCaseData;
                reader.ReadValueSafe(out newplayerCaseData);
                playerCasesList.Add(newplayerCaseData);
            playerCaseDatas = playerCasesList.ToArray();
            }
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();

            writer.WriteValueSafe(clientIdsInLobby.Length);
            foreach (ulong clientId in clientIdsInLobby)
            {
                writer.WriteValueSafe(clientId);
            }

            writer.WriteValueSafe(playerCaseDatas.Length);
            for (int i = 0; i < playerCaseDatas.Length; i++)
            {
                PlayerCaseData caseData = playerCaseDatas[i];
                serializer.SerializeValue(ref caseData);
            }
        }
    }

    public bool Equals(LobbyData other)
    {
        if (other.clientIdsInLobby==null || other.playerCaseDatas==null || clientIdsInLobby == null || playerCaseDatas == null) return false;
        
        return Util.IsEqualArrays(clientIdsInLobby,other.clientIdsInLobby) && Util.IsEqualArrays(playerCaseDatas,other.playerCaseDatas);
    }

    public override string ToString()
    {
        string ans = $"idsList: ";
        foreach (var item in clientIdsInLobby)
        {
            ans+=$"{item}, ";
        } 
        ans+=", caseDatas: ";
        foreach (var item in playerCaseDatas)
        {
            ans+=$"{item}, ";
        } 
        return ans;
    }
}