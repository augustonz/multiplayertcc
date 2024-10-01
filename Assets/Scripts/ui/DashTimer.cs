using Game;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DashTimer : MonoBehaviour
{
    Slider slider;
    PlayerControllerOffline player;
    void Awake() {
        slider = GetComponent<Slider>();
    }

    void Start() {
        player = FindObjectOfType<PlayerControllerOffline>();
    }

    void Update() {
        slider.value = player.DashFillPercentage;
    }
}
