using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.Netcode;

public struct LobbyData : INetworkSerializable, System.IEquatable<LobbyData>
{
    public ulong localClientId;
    public List<ulong> clientIdsInLobby;
    public List<PlayerCaseData> playerCaseDatas;
    public int PlayersInLobby => clientIdsInLobby.Count;
    public int ReadyPlayers => playerCaseDatas.FindAll(data => data.isReady).Count;
    public bool IsLocalPlayerReady()
    {
        ulong clientid = localClientId;
        return playerCaseDatas.Exists(data => data.clientId == clientid && data.isReady);
    }

    public LobbyData(bool dummyParam)
    {
        localClientId = 0;
        clientIdsInLobby = new List<ulong>();
        playerCaseDatas = new List<PlayerCaseData>
        {
            new PlayerCaseData(true),
            new PlayerCaseData(true),
            new PlayerCaseData(true),
            new PlayerCaseData(true),
            new PlayerCaseData(true),
            new PlayerCaseData(true)
        };
    }

    public void addClientToPlayerCaseDatas(ulong clientId)
    {
        PlayerCaseData emptyCase = playerCaseDatas.Find(data =>
        {
            return data.clientId == 0;
        });
        emptyCase.clientId = clientId;
    }

    public void removeClientFromPlayerCaseDatas(ulong clientId)
    {
        PlayerCaseData oldCase = playerCaseDatas.Find(data =>
        {
            return data.clientId == clientId;
        });
        oldCase = new PlayerCaseData(true);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out localClientId);

            int clientIdsCount;
            reader.ReadValueSafe(out clientIdsCount);
            clientIdsInLobby = new List<ulong>();
            for (int i = 0; i < clientIdsCount; i++)
            {
                ulong newClientId;
                reader.ReadValueSafe(out newClientId);
                clientIdsInLobby.Add(newClientId);
            }

            int playerCasesCount;
            reader.ReadValueSafe(out playerCasesCount);
            playerCaseDatas = new List<PlayerCaseData>();
            for (int i = 0; i < playerCasesCount; i++)
            {
                PlayerCaseData newplayerCaseData;
                reader.ReadValueSafe(out newplayerCaseData);
                playerCaseDatas.Add(newplayerCaseData);
            }
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(localClientId);

            writer.WriteValueSafe(clientIdsInLobby.Count);
            foreach (ulong clientId in clientIdsInLobby)
            {
                writer.WriteValueSafe(clientId);
            }

            writer.WriteValueSafe(playerCaseDatas.Count);
            for (int i = 0; i < playerCaseDatas.Count; i++)
            {
                PlayerCaseData caseData = playerCaseDatas[i];
                serializer.SerializeValue(ref caseData);
            }
        }
    }

    public bool Equals(LobbyData other)
    {
        return localClientId == other.localClientId && clientIdsInLobby == other.clientIdsInLobby;
    }

    public override string ToString()
    {
        string ans = $"localId: {localClientId}, idsList: ";
        clientIdsInLobby.ForEach(id=>ans+=$"{id}, ");
        ans+=", caseData: ";
        playerCaseDatas.ForEach(data=>ans+=$"{data}");
        return ans;
    }
}