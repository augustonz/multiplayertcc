using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;

public class Ping : NetworkBehaviour
{
    [SerializeField] TMP_Text _pingText;
    [SerializeField] Image _pingImage;
    [SerializeField] Material _redPingMat;
    [SerializeField] Material _greenPingMat;
    [SerializeField] float greenCutoff;
    [SerializeField] float yellowCutoff;
    [SerializeField] float pingDelay;
    NetworkTime lastNetworkTime;
    float pingDelayTimer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!NetworkManager.IsListening) {
            Debug.LogError("Ping initialized when networkManager is not listening");
            Destroy(gameObject);
        }
    }
    void Update()
    {
        if (IsClient) {
            // float newPing = PingCalc1();
            // UpdatePing(newPing);
            PingCalc2();
        } else if (IsServer) {

        }
    }

    float PingCalc1() {
        return (NetworkManager.LocalTime - NetworkManager.ServerTime).TimeAsFloat*1000;
        
    }

    void UpdatePing(float newPing) {
        _pingText.text = $"{(int)newPing}ms";
        if (newPing > yellowCutoff) {
            _pingImage.material = _redPingMat;
        } else if (newPing < greenCutoff) {
            _pingImage.material = _greenPingMat;
        } else {
            _pingImage.material = null;
        }
    }

    void PingCalc2() {
        pingDelayTimer += Time.deltaTime;
        if (pingDelayTimer>pingDelay) {
            lastNetworkTime = NetworkManager.LocalTime;
            pingDelayTimer=0;
            PingRpc();
        }
    }

    [Rpc(SendTo.Server)]
    void PingRpc(RpcParams rpcParams = default) {
        PongRpc(RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void PongRpc(RpcParams rpcParams = default) {
        float newPing =  (NetworkManager.LocalTime - lastNetworkTime).TimeAsFloat*1000;
        UpdatePing(newPing);
    }
}
