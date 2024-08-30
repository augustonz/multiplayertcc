using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

public class MatchUI : NetworkBehaviour
{
    [SerializeField] GameObject timer;
    [SerializeField] TMP_Text roundText;
    [SerializeField] TMP_Text timerText;
    [SerializeField] GameObject scoreBoard;
    [SerializeField] RectTransform playerMatchDataContainer;
    [SerializeField] Slider dashTimer;
    List<PlayerScoreUI> playerScoreUIs = new List<PlayerScoreUI>();



    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        foreach (Transform playerScore in playerMatchDataContainer)
        {
            playerScoreUIs.Add(playerScore.GetComponent<PlayerScoreUI>());
        }

        StartMatchUI();
        Match match = FindObjectOfType<Match>();
        match.matchUI = this;
    }

    public void UpdateDashTimer(float currPerc) {
        dashTimer.value = currPerc;
    }

    public void EndMatchUI() {
        timer.SetActive(false);
        dashTimer.gameObject.SetActive(false);
        scoreBoard.SetActive(true);
    }

    public void StartMatchUI() {
        timer.SetActive(true);
        dashTimer.gameObject.SetActive(true);
        scoreBoard.SetActive(false);
    }

    public void UpdateSpeedrunTimer(float timer) {
        timerText.text = Util.formatMatchTime(timer);
    }

    public void UpdateUI(MatchData data) {
        roundText.text = data.currentMatchRound.ToString();
        UpdatePlayerScoresUI(data.playersInMatch);
    }

    void UpdatePlayerScoresUI(List<PlayerMatchData> playerMatchDatas) {
        for (int i = 0; i < playerScoreUIs.Count; i++)
        {
            playerScoreUIs[i].UpdateUI(i<playerMatchDatas.Count ? playerMatchDatas[i] : PlayerMatchData.Empty());
        }
    }
}
