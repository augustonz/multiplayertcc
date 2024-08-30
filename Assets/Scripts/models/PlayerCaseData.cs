using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerCaseData : INetworkSerializable, System.IEquatable<PlayerCaseData> {
    public ulong clientId;
    public FixedString128Bytes playerName;
    public bool isReady;
    public FixedString128Bytes playerColor;

    public static PlayerCaseData Empty() {
        PlayerCaseData playerCase;
        playerCase.clientId = 0;
        playerCase.playerName = "";
        playerCase.isReady = false;
        playerCase.playerColor = "blue";
        return playerCase;
    }

    public PlayerCaseData(ulong clientId, string playerName, bool isReady, string playerColor) {
        this.clientId=clientId;
        this.playerName=playerName;
        this.isReady=isReady;
        this.playerColor=playerColor;
    }

    public bool IsEmpty() {
        return clientId == 0;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out clientId);
            reader.ReadValueSafe(out playerName);
            reader.ReadValueSafe(out isReady);
            reader.ReadValueSafe(out playerColor);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(clientId);
            writer.WriteValueSafe(playerName);
            writer.WriteValueSafe(isReady);
            writer.WriteValueSafe(playerColor);
        }
    }

    public bool Equals(PlayerCaseData other)
    {
        return clientId == other.clientId && playerName == other.playerName && playerColor == other.playerColor && isReady == other.isReady;
    }

    public override string ToString()
    {
        string ans = $"clientId: {clientId}, playerName: {playerName},isReady: {isReady},playerColor: {playerColor}\n";
        return ans;
    }
}