using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerMatchData : INetworkSerializable, System.IEquatable<PlayerMatchData> {
    public ulong clientId;
    public FixedString128Bytes playerName;
    public FixedString128Bytes playerColor;
    public int score;
    public int currentRoundPlacement;
    public float currentRoundTimer;
    //Maybe create a list of round placements and timers per round. Like a history of past rounds

    public static PlayerMatchData Empty() {
        PlayerMatchData player = new PlayerMatchData {
            clientId = 0,
            score = 0,
            currentRoundPlacement = 0,
            currentRoundTimer = 0,
            playerColor = "blue",
            playerName = "",
        };
        return player;
    }

    public PlayerMatchData(ulong clientId, string playerName, string playerColor,int score,int currentRoundPlacement,float currentRoundTimer) {
        this.clientId = clientId;
        this.playerName = playerName;
        this.playerColor = playerColor;
        this.score = score;
        this.currentRoundPlacement = currentRoundPlacement;
        this.currentRoundTimer = currentRoundTimer;
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
            reader.ReadValueSafe(out playerColor);
            reader.ReadValueSafe(out score);
            reader.ReadValueSafe(out currentRoundPlacement);
            reader.ReadValueSafe(out currentRoundTimer);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(clientId);
            writer.WriteValueSafe(playerName);
            writer.WriteValueSafe(playerColor);
            writer.WriteValueSafe(score);
            writer.WriteValueSafe(currentRoundPlacement);
            writer.WriteValueSafe(currentRoundTimer);
        }
    }

    public bool Equals(PlayerMatchData other)
    {
        return clientId == other.clientId && playerName == other.playerName && playerColor == other.playerColor && score == other.score && currentRoundPlacement == other.currentRoundPlacement && currentRoundTimer == other.currentRoundTimer;
    }

    public override string ToString()
    {
        string ans = $"clientId: {clientId}, playerName: {playerName},score: {score},playerColor: {playerColor},currentRoundPlacement: {currentRoundPlacement},currentRoundTimer: {currentRoundTimer}\n";
        return ans;
    }
}