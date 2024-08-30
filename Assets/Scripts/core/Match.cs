using System.Threading;
using Game;
using Unity.Netcode;
using UnityEngine;

public class Match : NetworkBehaviour
{
    static MatchData generatedMatchData;
    [SerializeField] CameraController cameraController;
    [SerializeField] SpawnPoint playersSpawnPoint;
    float startRoundTimer;
    float currentRoundTimer;
    float forceEndRoundTimer;
    bool hasWinner;
    bool hasFinished;
    MatchUI matchUIInternal;
    PlayerController localPlayer;
    public MatchUI matchUI { get => matchUIInternal; set {
        matchUIInternal = value;
        matchUIInternal.UpdateUI(matchData.Value);
    } }
    public NetworkVariable<MatchData> matchData = new NetworkVariable<MatchData>(MatchData.Empty());
    override public void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        if (IsServer) {
            matchData.Value = generatedMatchData;
        } else {
        }
        GameController.Singleton.match = this;
        matchData.OnValueChanged += OnMatchDataValueChange;
        startRoundTimer = 0;
        currentRoundTimer = startRoundTimer;
    }

    void OnMatchDataValueChange(MatchData prev,MatchData curr) {
        Debug.Log($"Match data value changed to : {curr}");
        if (matchUI!=null) matchUI.UpdateUI(curr);
    }

    void Update() {
        if (IsServer) ServerUpdate();
        else if (IsClient) ClientUpdate();
        
        CommonUpdate();
    }

    public void PlayerDied(PlayerController playerController) {
        playerController.transform.position = playersSpawnPoint.transform.position;
    }

    void ServerUpdate() {
        currentRoundTimer+=Time.deltaTime;
        if (hasWinner && currentRoundTimer>forceEndRoundTimer) ForceEndRound();
    }

    public 

    void ClientUpdate() {
        if (!hasFinished) currentRoundTimer+=Time.deltaTime;
        if (localPlayer!=null) {
            matchUI.UpdateDashTimer(localPlayer.DashFillPercentage);
        }
    }

    void CommonUpdate() {
        if (matchUI!=null) matchUI.UpdateSpeedrunTimer(currentRoundTimer);

    }

    public void SpawnedLocalPlayer(PlayerController playerObject) {
        localPlayer = playerObject;

        PlayerUI playerUI = playerObject.transform.GetComponentInChildren<PlayerUI>();
        playerUI.SetName(matchData.Value.GetPlayerMatchData(playerObject.OwnerClientId).playerName.Value);
        
        cameraController.FollowPlayer(localPlayer.OwnerClientId);
    }

    public void PlayerCrossedLine(ulong clientId) {
        if (hasWinner==false) FirstPlayerFinished();

        CrossedLineRpc(clientId);
        if (clientId == MyNetworkManager.Singleton.LocalClientId) {
            LocalPlayerCrossedLine();
        }
    }

    void ForceEndRound() {
        Debug.Log("Round forced to end");
        FinishRound();
    }

    void FirstPlayerFinished() {
        hasWinner = true;
        forceEndRoundTimer = currentRoundTimer + 30;
    }

    void LocalPlayerCrossedLine() {
        hasFinished = true;
    }

    public void FinishRound() {
        localPlayer.EnablePlayerInput(false);
        matchUI.EndMatchUI();
    }

    #region ClientRPCs
    [Rpc(SendTo.NotServer)]
    void NotifyRoundEndedRpc() {
        FinishRound();
    }

    #endregion

    #region ServerRPCs
    [Rpc(SendTo.Server)]
    void CrossedLineRpc(ulong clientId) {
        MatchData newMatchData = GetCopy(matchData.Value);
        
        newMatchData.UpdatePlayerMatchData(clientId,currentRoundTimer: currentRoundTimer);

        matchData.Value = newMatchData;

        NotifyRoundEndedRpc();
    }

    #endregion

    #region MatchDataHelper

    public static void GenerateMatchData(LobbyData lobbyData) {
        generatedMatchData = MatchData.Empty();

        foreach (var item in lobbyData.playerCaseDatas)
        {   

            if (item.clientId!=0) generatedMatchData.playersInMatch.Add(new PlayerMatchData {
                clientId = item.clientId,
                playerName = item.playerName,
                playerColor = item.playerColor,
                score = 0,
                currentRoundPlacement = 0,
                currentRoundTimer = 0
            });
        } 
    }

    MatchData GetCopy(MatchData original) {
        MatchData newCopy = MatchData.Empty();
        for (int i = 0; i < original.playersInMatch.Count; i++)
        {
            newCopy.playersInMatch.Add(new PlayerMatchData(
                original.playersInMatch[i].clientId,
                original.playersInMatch[i].playerName.Value,
                original.playersInMatch[i].playerColor.Value,
                original.playersInMatch[i].score,
                original.playersInMatch[i].currentRoundPlacement,
                original.playersInMatch[i].currentRoundTimer
            ));
        }

        newCopy.currentMatchRound = original.currentMatchRound;
        newCopy.maxMatchRounds = original.maxMatchRounds;
        newCopy.stageId = original.stageId;

        return newCopy;
    }

    #endregion

}
