using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
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

    List<Toggle> _toggles = new List<Toggle>();

    PlayerCaseData playerCaseData = PlayerCaseData.Empty();

    public void SetPlayerData(PlayerCaseData data) {
        playerCaseData = data;
        playerName.interactable = data.clientId == GameController.Singleton.MyNetworkManager.LocalClientId;
        _toggles.ForEach(toggle=>toggle.interactable = data.clientId == GameController.Singleton.MyNetworkManager.LocalClientId);
    }
    void Awake()
    {
        isReadyBackground = isReadyToggle.transform.GetChild(0).GetComponent<Image>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        UpdateUI(playerCaseData);
        foreach (Transform item in chooseColorGroup.transform)
        {
            Toggle toggle = item.GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(toggleSelected);
            _toggles.Add(toggle);
        }
    }

    void toggleSelected(bool newValue) {
        Toggle toggle = chooseColorGroup.ActiveToggles().Count() == 0 ? null : chooseColorGroup.ActiveToggles().ToArray()[0];
        if (toggle == null) return;
        if (!newValue) {
            toggle.SetIsOnWithoutNotify(true);
            return;
        }
        Color color = toggle.transform.GetChild(0).GetComponent<Image>().material.GetColor("_ToColor");
        GameController.Singleton.lobby.ChangePlayerColor(Util.ColorToString(color));
    }

    public void UpdateUI(PlayerCaseData data) {
        SetPlayerData(data);
        if (data.IsEmpty()) {
            TurnOnElements(false);
            return;
        }
        TurnOnElements(true);

        playerName.text = data.playerName.Value;
        isReadyToggle.isOn = data.isReady;
        chooseColorGroup.SetAllTogglesOff();
        chooseColorGroup.GetComponentsInChildren<Toggle>()[ColorToToggleIndex(data.playerColor.Value)].SetIsOnWithoutNotify(true);
        playerChar.material = Util.getPlayerMaterialFromColor(data.playerColor.Value);
        ownerTag.enabled = data.clientId == GameController.Singleton.MyNetworkManager.LocalClientId;
    }

    int ColorToToggleIndex(string color) {
        for (int i = 0; i < chooseColorGroup.transform.childCount; i++)
        {
            switch(color) {
                case "blue":
                    return 0;
                case "red":
                    return 1;
                case "green":
                    return 2;
                case "yellow":
                    return 3;
                case "magenta":
                    return 4;
                case "cyan":
                    return 5;
            }
        }
        return 0;
    } 

    void TurnOnElements(bool state) {
        isReadyToggle.gameObject.SetActive(state);
        playerName.gameObject.SetActive(state);
        chooseColorGroup.gameObject.SetActive(state);
        playerChar.enabled = state;
        ownerTag.enabled = state;
    }
}
