using System.Collections;
using System.Collections.Generic;
using Game;
using Unity.Netcode;
using UnityEngine;

[ExecuteInEditMode]
public class FinishLine : NetworkBehaviour
{

    [SerializeField] bool isHorizontalDirection;
    [SerializeField] int size;

    SpriteRenderer spriteRenderer;
    BoxCollider2D boxCollider;

    const float WIDTH = 0.625f;
    const float COLLIDER_WIDTH = 0.34375f;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        if (IsServer) {
            Destroy(this);
        }
    }

    void OnValidate() {
        if (isHorizontalDirection) transform.rotation = Quaternion.Euler(0,0,90);
        else transform.rotation = Quaternion.Euler(0,0,0);

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.size = new Vector2(WIDTH, size * WIDTH);

        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(COLLIDER_WIDTH, size * WIDTH);
    }

    [Rpc(SendTo.Server)]
    void CheckOnServerRpc(RpcParams rpcParams = default) {
        GameController.Singleton.match.PlayerCrossedLine(rpcParams.Receive.SenderClientId);
    }

    void CheckLocally() {
        GameController.Singleton.match.PlayerCrossedLine(NetworkManager.Singleton.LocalClientId);
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            PlayerController playerController = other.GetComponent<PlayerController>();
            CheckLocally();
        }
    }
}
