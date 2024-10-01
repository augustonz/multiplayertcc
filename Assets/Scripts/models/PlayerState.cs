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
    public bool inputMoved;

    public Vector2 lastInputDirection;
    public bool isSliding;
    public bool isGrounded;
    public int airJumps;

    public bool shouldTriggerHitWall;
    public bool isEnteringWall;

    public bool shouldTriggerJumped;
    public bool isWallJump;

    public bool shouldTriggerGroundChange;

    public PlayerState(int tick, Vector3 finalPos, Quaternion finalRot, Vector3 finalSpeed, bool hasDashed, bool inputMoved,
    Vector2 lastInputDirection, bool isSliding, bool isGrounded, int airJumps, bool shouldTriggerGroundChange, bool shouldTriggerHitWall, bool shouldTriggerJumped, bool isEnteringWall, bool isWallJump) {
        this.tick=tick;
        this.finalPos=finalPos;
        this.finalRot=finalRot;
        this.finalSpeed=finalSpeed;
        this.hasDashed=hasDashed;
        this.inputMoved=inputMoved;

        this.lastInputDirection = lastInputDirection;
        this.isSliding = isSliding;
        this.isGrounded = isGrounded;
        this.airJumps = airJumps;
        this.shouldTriggerHitWall = shouldTriggerHitWall;
        this.isEnteringWall = isEnteringWall;
        this.shouldTriggerJumped = shouldTriggerJumped;
        this.isWallJump = isWallJump;
        this.shouldTriggerGroundChange = shouldTriggerGroundChange;
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
            reader.ReadValueSafe(out inputMoved);

            reader.ReadValueSafe(out lastInputDirection);
            reader.ReadValueSafe(out isSliding);
            reader.ReadValueSafe(out isGrounded);
            reader.ReadValueSafe(out airJumps);
            reader.ReadValueSafe(out shouldTriggerHitWall);
            reader.ReadValueSafe(out isEnteringWall);
            reader.ReadValueSafe(out shouldTriggerJumped);
            reader.ReadValueSafe(out isWallJump);
            reader.ReadValueSafe(out shouldTriggerGroundChange);

        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(tick);
            writer.WriteValueSafe(finalPos);
            writer.WriteValueSafe(finalRot);
            writer.WriteValueSafe(finalSpeed);
            writer.WriteValueSafe(hasDashed);
            writer.WriteValueSafe(inputMoved);

            writer.WriteValueSafe(lastInputDirection);
            writer.WriteValueSafe(isSliding);
            writer.WriteValueSafe(isGrounded);
            writer.WriteValueSafe(airJumps);
            writer.WriteValueSafe(shouldTriggerHitWall);
            writer.WriteValueSafe(isEnteringWall);
            writer.WriteValueSafe(shouldTriggerJumped);
            writer.WriteValueSafe(isWallJump);
            writer.WriteValueSafe(shouldTriggerGroundChange);
            
        }
    }

    public bool Equals(PlayerState other)
    {
        return finalPos == other.finalPos && finalRot == other.finalRot && finalSpeed == other.finalSpeed && inputMoved == other.inputMoved;
    }

    public override string ToString()
    {
        string ans = $"tick: {tick}, finalPos: {finalPos},finalRot: {finalRot}\n";
        return ans;
    }
}