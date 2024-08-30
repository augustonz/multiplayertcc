using Game;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class KillZone : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            playerController.Die();
        }
    }
}