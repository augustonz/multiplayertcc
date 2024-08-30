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
    [SerializeField] RectTransform playerCaseContainer;
    public List<PlayerCase> playerCases;

    bool isPopupActivated;
    float popupRemainingTime;
    const float POPUP_TIME = 5f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        foreach (Transform playerCase in playerCaseContainer)
        {
            playerCases.Add(playerCase.GetComponent<PlayerCase>());
        }
    }

    public void SetPopupText(string text) {
        popupRemainingTime = POPUP_TIME;
        PopupMsg.text = text;
        isPopupActivated = true;
    }

    void Update() {
        if (isPopupActivated) {
            popupRemainingTime-=Time.deltaTime;
            if (popupRemainingTime<=0) {
                isPopupActivated=false;
                PopupMsg.text="";
            }
        }
    }

    public void UpdateUI(LobbyData data) {
        PlayersReadyMsg.text = $"{data.ReadyPlayers}/{data.PlayersInLobby} ready";
        readyPlayerButtonText.text = data.IsLocalPlayerReady()? $"Unready" : "Ready";
        UpdatePlayerCasesUI(data.playerCaseDatas);
    }
    void UpdatePlayerCasesUI(PlayerCaseData[] playerCasesDatas) {
        for (int i = 0; i < playerCasesDatas.Length; i++)
        {
            playerCases.ToArray()[i].UpdateUI(playerCasesDatas[i]);
        }
    }
}
