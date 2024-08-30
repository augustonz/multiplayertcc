using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreUI : NetworkBehaviour
{

    [SerializeField] TMP_Text playerLeaderBoardPosition;
    [SerializeField] TMP_Text playerName;
    [SerializeField] Image playerImg;
    [SerializeField] TMP_Text playerTime;
    [SerializeField] TMP_Text playerScore;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    void Update()
    {
        
    }

    public void UpdateUI(PlayerMatchData playerMatchData) {
        if (playerMatchData.IsEmpty()) {
            TurnOnElements(false);
            return;
        }
        TurnOnElements(true);
        playerName.text = playerMatchData.playerName.Value;
        playerTime.text = Util.formatMatchTime(playerMatchData.currentRoundTimer);
        playerScore.text = $"Score: {playerMatchData.score}";
    }

    void TurnOnElements(bool state) {
        playerImg.enabled = state;
        playerName.gameObject.SetActive(state);
        playerTime.gameObject.SetActive(state);
        playerScore.gameObject.SetActive(state);
        playerLeaderBoardPosition.gameObject.SetActive(state);
    }
}
