using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

public class LobbyUI : NetworkBehaviour
{
    [SerializeField] TMP_Text PlayersReadyMsg;
    [SerializeField] TMP_Text PopupMsg;
    [SerializeField] TMP_Text readyPlayerButtonText;
    [SerializeField] Button startMatchButton;
    [SerializeField] Button readyPlayerButton;
    [SerializeField] Button leaveButton;
    [SerializeField] RectTransform playerCaseContainer;
    public List<PlayerCase> playerCases;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        foreach (Transform playerCase in playerCaseContainer)
        {
            playerCases.Add(playerCase.GetComponent<PlayerCase>());
        }
    }

    public void UpdateUI(LobbyData data) {
        PlayersReadyMsg.text = $"X of X players ready";
        PopupMsg.text = $"Sample pop-up txt";
        readyPlayerButtonText.text = data.IsLocalPlayerReady()? $"Unready" : "Ready";
        UpdatePlayerCasesUI(data.playerCaseDatas);
    }
    void UpdatePlayerCasesUI(List<PlayerCaseData> playerCasesDatas) {
        for (int i = 0; i < playerCasesDatas.Count; i++)
        {
            playerCases.ToArray()[i].UpdateUI(playerCasesDatas.ToArray()[i]);
        }
    }
}
