using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    NetworkVariable<Vector3> _netPlayerPos = new(writePerm: NetworkVariableWritePermission.Owner);

    void Update() {
        if (IsOwner) {
            _netPlayerPos.Value = transform.position;
        } else {
            transform.position = _netPlayerPos.Value;
        }
    }
}
