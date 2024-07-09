using UnityEngine;

public class PlayerCaseData {
    public ulong clientId {get; set;}
    public string playerName {get; set;}
    public bool isReady {get; set;}
    public Color playerColor {get; set;}
    
    public PlayerCaseData() {
        clientId = 0;
        playerName = "";
        isReady = false;
        playerColor = Color.black;
    }

    public bool IsEmpty() {
        return clientId == 0;
    }
}