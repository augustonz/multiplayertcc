using Game;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class StaminaPickUp : NetworkBehaviour
{
    [SerializeField] private bool isBigPickup;
    
    private NetworkAnimator networkAnim;
    private Animator anim;
    private CapsuleCollider2D coll;
    
    [Header("Changeable vars")]
    [SerializeField] private float smallStaminaRecovery;
    [SerializeField] private float bigStaminaRecovery;
    [SerializeField] private float respawnTimer;

    private bool isDead;
    private float isDeadTimer;
    private float staminaRecovery;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
            coll.enabled = false;
        }
        
        anim.SetBool("isBig",isBigPickup);

        if (isBigPickup)
        {
            coll.size = new Vector2(0.44f, 0.44f);
            staminaRecovery = bigStaminaRecovery;
        } else
        {
            coll.size = new Vector2(0.22f, 0.22f);
            staminaRecovery = smallStaminaRecovery;
        }
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        coll = GetComponent<CapsuleCollider2D>();
        networkAnim = GetComponent<NetworkAnimator>();
    }

    [Rpc(SendTo.Server)]
    public void GetPickedUpRpc(RpcParams rpcParams = default)
    {
        if (IsClient) return;
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkClient))
        {
            PlayerController playerController = networkClient.PlayerObject.GetComponent<PlayerController>();
            playerController.AddDashPercRpc(staminaRecovery);
        }
        isDead = true;
        networkAnim.SetTrigger("getPickedUp");
        isDeadTimer = 0;
    }

    public void Reset()
    {
        isDead = false;
        networkAnim.SetTrigger("respawn");
    }

    void Update()
    {
        if (isDead && isDeadTimer > respawnTimer)
        {
            Reset();
            return;
        }

        isDeadTimer += Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.gameObject.tag != "Player") return;
        Debug.Log("Collision called on "  + (NetworkManager.IsServer ? "Server":"Client"));
        PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
        if (!IsServer) GetPickedUpRpc();
    }
}
