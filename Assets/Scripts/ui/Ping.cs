using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using Game;
public class Ping : NetworkBehaviour
{
    [SerializeField] TMP_Text _pingText;
    [SerializeField] Image _pingImage;
    [SerializeField] Material _redPingMat;
    [SerializeField] Material _greenPingMat;
    [SerializeField] float greenCutoff;
    [SerializeField] float yellowCutoff;
    [SerializeField] float pingDelay;
    [SerializeField] float highPingGoal;
    double lastNetworkTime;
    float pingDelayTimer;
    int pingBufferIndex;
    const int buffer = 50;
    public double[] pingInstances = new double[buffer];

    public static int CurrentPing;
    public static double ArtificialWait;
    NetworkVariable<float> _time = new NetworkVariable<float>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!NetworkManager.IsListening) {
            Debug.LogError("Ping initialized when networkManager is not listening");
            Destroy(gameObject);
        }
        if (IsClient) {
            _time.OnValueChanged +=OnTimeUpdate;
        }
    }

    void OnTimeUpdate(float prev, float curr) {
        
    }
    void Update()
    {
        if (IsServer) {
            _time.Value+=Time.deltaTime;
        }
        if (IsClient) {
            PingCalc2();
        } else if (IsServer) {

        }
    }


    void UpdatePing(double newPing) {
        CurrentPing = (int)newPing;
        _pingText.text = $"{CurrentPing}ms";
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
            pingDelayTimer=0;
            lastNetworkTime = _time.Value;
            PingRpc(lastNetworkTime);
        }
    }

    [Rpc(SendTo.Server)]
    void PingRpc(double pingInstanceTime,RpcParams rpcParams = default) {
        if (Variables.hasArtificialLag) {
            double RTT = _time.Value - pingInstanceTime;
            StartCoroutine(DelayedPong(RTT, rpcParams));
        } else {
            PongRpc(0,RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
        }
    }

    IEnumerator DelayedPong(double RTT,RpcParams rparams) {
        double timeToWait = highPingGoal - RTT;
        ArtificialWait = timeToWait;
        yield return new WaitForSeconds((float)timeToWait);

        PongRpc(timeToWait,RpcTarget.Single(rparams.Receive.SenderClientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void PongRpc(double timeToWait, RpcParams rpcParams = default) {
        ArtificialWait = timeToWait;
        double newPing =  (_time.Value - lastNetworkTime) * 1000;
        pingBufferIndex ++;
        pingBufferIndex%=buffer;
        pingInstances[pingBufferIndex] = newPing;
        UpdatePing(GetTotalPing()/GetPingInstancesLength());
    }

    float GetTotalPing() {
        float sum = 0;
        foreach (float item in pingInstances)
        {
            sum+=item;
        }
        return sum;
    }

    float GetPingInstancesLength() {
        float len = 0;
        foreach (float item in pingInstances)
        {
            if (item!=0)len++;
        }
        return len;
    }
}
