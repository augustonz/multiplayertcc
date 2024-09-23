using Game;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null) {
                playerController.Die();
                return;
            }

            PlayerControllerOffline playerControllerOffline = other.GetComponent<PlayerControllerOffline>();
            if (playerControllerOffline != null) {
                playerControllerOffline.Die();
                return;
            }
        }
    }
}