using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct PlayerState : INetworkSerializable, System.IEquatable<PlayerState> {
    public int tick;
    public Vector3 finalPos;
    public Quaternion finalRot;
    public Vector3 finalSpeed;
    public bool hasDashed;
    public static PlayerState Empty() {
        PlayerState playerState;
        playerState.tick = 0;
        playerState.finalPos = new Vector3();
        playerState.finalRot = new Quaternion();
        playerState.finalSpeed = new Vector3();
        playerState.hasDashed = false;
        return playerState;
    }

    public PlayerState(int tick, Vector3 finalPos, Quaternion finalRot, Vector3 finalSpeed, bool hasDashed) {
        this.tick=tick;
        this.finalPos=finalPos;
        this.finalRot=finalRot;
        this.finalSpeed=finalSpeed;
        this.hasDashed=hasDashed;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out tick);
            reader.ReadValueSafe(out finalPos);
            reader.ReadValueSafe(out finalRot);
            reader.ReadValueSafe(out finalSpeed);
            reader.ReadValueSafe(out hasDashed);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(tick);
            writer.WriteValueSafe(finalPos);
            writer.WriteValueSafe(finalRot);
            writer.WriteValueSafe(finalSpeed);
            writer.WriteValueSafe(hasDashed);
        }
    }

    public bool Equals(PlayerState other)
    {
        return finalPos == other.finalPos && finalRot == other.finalRot && finalSpeed == other.finalSpeed;
    }

    public override string ToString()
    {
        string ans = $"tick: {tick}, finalPos: {finalPos},finalRot: {finalRot}\n";
        return ans;
    }
}