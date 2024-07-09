using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCase : NetworkBehaviour
{
    [SerializeField] TMP_Text isReadyMsg;
    [SerializeField] Toggle isReadyToggle;
    Image isReadyBackground;
    [SerializeField] Image playerChar;
    [SerializeField] Image ownerTag;
    [SerializeField] TMP_InputField playerName;
    [SerializeField] Button confirmPlayerNameBtn;
    [SerializeField] ToggleGroup chooseColorGroup;

    PlayerCaseData playerCaseData = new PlayerCaseData();

    public void SetPlayerData(PlayerCaseData data) {
        playerCaseData = data;
    }
    void Awake()
    {
        isReadyBackground = isReadyToggle.transform.GetChild(0).GetComponent<Image>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        UpdateUI(playerCaseData);
    }

    void Update()
    {
        
    }

    public void UpdateUI(PlayerCaseData data) {
        if (data.IsEmpty()) {
            TurnOnElements(false);
            return;
        }
        TurnOnElements(true);

        playerName.text = data.playerName;
        isReadyToggle.isOn = data.isReady;
        ownerTag.enabled = data.clientId == NetworkManager.Singleton.LocalClientId;
    }

    void TurnOnElements(bool state) {
        isReadyToggle.gameObject.SetActive(state);
        playerName.gameObject.SetActive(state);
        chooseColorGroup.gameObject.SetActive(state);
        playerChar.enabled = state;
    }
}
