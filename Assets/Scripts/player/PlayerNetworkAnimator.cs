using Unity.Netcode.Components;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerNetworkAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative() {
        return false;
    }
}
