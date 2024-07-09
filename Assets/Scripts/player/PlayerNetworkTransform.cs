using Unity.Netcode.Components;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerNetworkTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative() {
        return false;
    }
}
