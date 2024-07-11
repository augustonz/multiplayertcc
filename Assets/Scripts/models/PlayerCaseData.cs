using Unity.Netcode;
using UnityEngine;

public struct PlayerCaseData : INetworkSerializable, System.IEquatable<PlayerCaseData> {
    public ulong clientId;
    public string playerName;
    public bool isReady;
    public Color playerColor;
    
    public PlayerCaseData(bool dummyParam) {
        clientId = 0;
        playerName = "";
        isReady = false;
        playerColor = Color.black;
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
        return clientId == other.clientId;
    }

    public override string ToString()
    {
        string ans = $"clientId: {clientId}, playerName: {playerName},isReady: {isReady},plaeyrColor: {playerColor}\n";
        return ans;
    }
}