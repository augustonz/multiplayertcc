using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct InputState : INetworkSerializable, System.IEquatable<InputState> {
    public int tick;
    public Vector2 moveInput;
    public bool jumpDown;
    public bool jumpHeld;
    public bool dashDown;
    public static InputState Empty() {
        InputState inputState;
        inputState.tick = 0;
        inputState.moveInput = new Vector2();
        inputState.jumpDown = false;
        inputState.jumpHeld = false;
        inputState.dashDown = false;
        return inputState;
    }

    public InputState(int tick, Vector2 moveInput, bool jumpDown, bool jumpHeld, bool dashDown) {
        this.tick=tick;
        this.moveInput=moveInput;
        this.jumpDown=jumpDown;
        this.jumpHeld=jumpHeld;
        this.dashDown=dashDown;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out tick);
            reader.ReadValueSafe(out moveInput);
            reader.ReadValueSafe(out jumpDown);
            reader.ReadValueSafe(out jumpHeld);
            reader.ReadValueSafe(out dashDown);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(tick);
            writer.WriteValueSafe(moveInput);
            writer.WriteValueSafe(jumpDown);
            writer.WriteValueSafe(jumpHeld);
            writer.WriteValueSafe(dashDown);
        }
    }

    public bool Equals(InputState other)
    {
        return tick == other.tick && moveInput == other.moveInput && jumpDown == other.jumpDown && jumpHeld == other.jumpHeld && dashDown == other.dashDown;
    }

    public override string ToString()
    {
        string ans = $"tick: {tick}, moveInput: {moveInput},jumpDown: {jumpDown},jumpHeld: {jumpHeld},dashDOwn: {dashDown}\n";
        return ans;
    }
}