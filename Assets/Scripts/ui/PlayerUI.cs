using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] TMP_Text playerName;
    public void SetName(string name) {
        playerName.text = name;
    }
}
