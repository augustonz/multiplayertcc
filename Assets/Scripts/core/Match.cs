using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Game;
using Unity.Netcode;
using UnityEngine;

public class Match : NetworkBehaviour
{
    static MatchData generatedMatchData;

    #region Common Properties
    float startRoundTimer;
    float currentRoundTimer;
    bool hasRaceStarted;
    #endregion

    #region Client Properties
    bool hasFinishedClient;
    #endregion

    #region Server Properties
    float forceEndRoundTimer;
    bool hasWinner;
    bool hasEnded;
    int nextPlayerWinPos = 1;
    int playersThatCrossedLine = 0;
    #endregion
    MatchUI matchUIInternal;
    PlayerController localPlayer;
    public MatchUI matchUI { get => matchUIInternal; set {
        matchUIInternal = value;
        matchUIInternal.UpdateUI(matchData.Value);
    } }

    Dictionary<int,int> _winPosToPointsGivenDict = new Dictionary<int, int>() {
        {1,6},
        {2,5},
        {3,4},
        {4,3},
        {5,2},
        {6,1},
    };
    public NetworkVariable<MatchData> matchData = new NetworkVariable<MatchData>(MatchData.Empty());

    override public void OnNetworkSpawn() {
        if (GameController.Singleton.match!= null) Destroy(this);
        GameController.Singleton.match = this;
        base.OnNetworkSpawn();
        if (IsServer) {
            if (matchData.Value.Equals(MatchData.Empty())) matchData.Value = generatedMatchData;
        } else {
        }
        matchData.OnValueChanged += OnMatchDataValueChange;
        DontDestroyOnLoad(this);
    }

    void SetPropertiesOnNewRound() {
        startRoundTimer = 0;
        currentRoundTimer = 0;
        forceEndRoundTimer = 0;
        hasRaceStarted = false;
        hasFinishedClient = false;
        hasWinner = false;
        hasEnded = false;
        playersThatCrossedLine = 0;
        nextPlayerWinPos = 1;
    }
    void OnMatchDataValueChange(MatchData prev,MatchData curr) {
        Debug.Log($"Previous current match {prev.currentMatchRound}, Current current match {curr.currentMatchRound}");
        if (matchUI!=null) matchUI.UpdateUI(curr);
    }

    void Update() {
        CommonUpdate();
    }

    void FixedUpdate() {
        if (IsServer) ServerUpdate();
        else if (IsClient) ClientUpdate();
    }

    public void PlayerDied(PlayerController playerController) {
        SpawnPoint spawnInScene = FindAnyObjectByType<SpawnPoint>();
        playerController.transform.position = spawnInScene.transform.position;
    }

    void CommonUpdate() {
        if (matchUI!=null) matchUI.UpdateSpeedrunTimer(currentRoundTimer);
    }

    public void PlayerCrossedLineOnServer(ulong clientId) {
        MatchData newMatchData = GetCopy(matchData.Value);
        
        PlayerMatchData currentPlayerMatchData = newMatchData.playersInMatch.First(data=>data.clientId==clientId);

        int scoreToAdd = _winPosToPointsGivenDict[nextPlayerWinPos];
        newMatchData.UpdatePlayerMatchData(clientId,currentRoundTimer: currentRoundTimer,score: currentPlayerMatchData.score+scoreToAdd, currentRoundPlacement: nextPlayerWinPos);
        
        nextPlayerWinPos+=1;

        matchData.Value = newMatchData;

        if (hasWinner==false) FirstPlayerFinished();
        
        playersThatCrossedLine +=1;
        if (playersThatCrossedLine==newMatchData.PlayersInMatch) {
            hasEnded = true;
        }

        PlayerCrossedLineRpc(RpcTarget.Single(clientId, RpcTargetUse.Temp));

    }

    public void LocalPlayerCrossedLine() {
        hasFinishedClient = true;
        FinishRound();
        AudioManager.instance.PlaySFX("win");
    }

    #region Client Code

    public void ReadyForNextRound() {
        MarkReadyRpc(GameController.Singleton.MyNetworkManager.LocalClientId);
    }

    void ClientUpdate() {
        if (!hasRaceStarted) return;
        if (!hasFinishedClient) currentRoundTimer+=Time.deltaTime;
        if (localPlayer!=null) {
            matchUI.UpdateDashTimer(localPlayer.DashFillPercentage);
        }
    }

    public void SpawnedLocalPlayer(PlayerController playerObject) {
        localPlayer = playerObject;

        // PlayerUI playerUI = localPlayer.transform.GetComponentInChildren<PlayerUI>();
        // playerUI.SetName(matchData.Value.GetPlayerMatchData(localPlayer.OwnerClientId).playerName.Value);
        localPlayer.EnablePlayerInput(false);
    }

    void StartRaceClient() {
        localPlayer.EnablePlayerInput(true);
        startRoundTimer = 0;
        currentRoundTimer = startRoundTimer;
        hasRaceStarted = true;
    }
    public void FinishRound() {
        localPlayer.EnablePlayerInput(false);
        matchUI.EndMatchUI();
    }

    void SetPlayerToSpawnPointClient(Vector2 spawnPos) {
        localPlayer.transform.position = spawnPos;
        
    }
    #endregion

    #region Server Code
    //This code runs after every player has loaded both the main scene and UI scene
    //This might be more accurately called prepareRace
    public void PrepareMatch() {
        StartCoroutine(nameof(PrepareRace));
    }

    void StartRace() {
        startRoundTimer = 0;
        currentRoundTimer = startRoundTimer;
        hasRaceStarted = true;
        StartRaceRpc();
    }

    IEnumerator PrepareRace() {
        SetLatencyCompensationVariables();

        yield return new WaitForSeconds(5f);
        
        SpawnAllPlayersInMatch();

        matchUIInternal.PlayerStartRaceAnimation();
        StartServerAnimationRpc();
        yield return new WaitForSeconds(8f);

        StartRace();
    }

    void SetLatencyCompensationVariables() {
        ulong currentRound = matchData.Value.currentMatchRound;
        ulong leftover = currentRound%4;

        // Variables.hasClientSidePrediction = leftover == 2 || leftover == 3;
        Variables.hasClientSidePrediction = false;
        // Variables.hasServerReconciliation = leftover == 3;
        Variables.hasServerReconciliation = false;
        Variables.hasEntityInterpolation = leftover == 0 || leftover == 2;
        // Variables.hasEntityInterpolation = true;
        // Variables.hasArtificialLag = currentRound>4;
        Variables.hasArtificialLag = currentRound>2;


        Debug.Log($"client side rendering: {Variables.hasClientSidePrediction}, servefr reconciliation {Variables.hasServerReconciliation}, entity interpolation {Variables.hasEntityInterpolation}, artificial Lag {Variables.hasArtificialLag}");
        SetLatencyCompensationVariablesRpc(Variables.hasClientSidePrediction,Variables.hasServerReconciliation,Variables.hasEntityInterpolation,Variables.hasArtificialLag);
    }

    void SpawnAllPlayersInMatch() {
        SpawnPoint spawnInScene = FindAnyObjectByType<SpawnPoint>();
        Vector2 spawnPos =  spawnInScene.transform.position;
        foreach (PlayerMatchData item in matchData.Value.playersInMatch)
        {
            GameController.Singleton.MyNetworkManager.networkSpawnHelper.SpawnPlayer(spawnPos,item.clientId);
        } 
        
    }
    void ServerUpdate() {
        if (!hasRaceStarted) return;
        currentRoundTimer+=Time.deltaTime;

        if (hasWinner && !hasEnded && currentRoundTimer>forceEndRoundTimer) ForceEndRound();

        if (hasEnded) CheckAllPlayersReady();
    }

    void CheckAllPlayersReady() {
        if (matchData.Value.PlayersInMatch==matchData.Value.PlayersReadyForNext) {
            hasRaceStarted=false; //Currently this line mostly serves to stop the server update from running
            StartCoroutine(nameof(GoNextRound));
        }
    }

    IEnumerator GoNextRound() {
        yield return new WaitForSeconds(3);
        SetPropertiesOnNewRound();
        SetPropertiesOnNewRoundRpc();
        MatchData newMatchData = GenerateNextRoundMatchData();
        matchData.Value = newMatchData;
        
        if (newMatchData.currentMatchRound<=newMatchData.maxMatchRounds) {
            string newSceneName = GameController.Singleton.MySceneManager.GetLevelById(newMatchData.stageId);
            GameController.Singleton.MySceneManager.ChangeSceneRpc(newSceneName);
        } else {
            GameController.Singleton.MySceneManager.ChangeSceneRpc("Menu");
        }

    }

    void ForceEndRound() {
        hasRaceStarted=false;
        hasEnded = true;
        NotifyRoundEndedRpc();
        StartCoroutine(nameof(GoNextRound));

    }
    
    void FirstPlayerFinished() {
        hasWinner = true;
        forceEndRoundTimer = currentRoundTimer + 30;
    }

    #endregion

    #region ClientRPCs
    [Rpc(SendTo.NotServer)]
    void NotifyRoundEndedRpc() {
        FinishRound();
    }

    [Rpc(SendTo.NotServer)]
    void SetLatencyCompensationVariablesRpc(bool clientPrediction, bool serverReconcilation, bool entityInterpolation, bool artificialLag) {
        Variables.hasClientSidePrediction=clientPrediction;
        Variables.hasServerReconciliation=serverReconcilation;
        Variables.hasEntityInterpolation=entityInterpolation;
        Variables.hasArtificialLag=artificialLag;
    }

    [Rpc(SendTo.NotServer)]
    void SetPropertiesOnNewRoundRpc() {
        SetPropertiesOnNewRound();
    }

    [Rpc(SendTo.NotServer)]
    void StartRaceRpc() {
        StartRaceClient();
    }

    [Rpc(SendTo.NotServer)]
    void SetAllPlayersToSpawnPointRpc(Vector2 spawnPos) {
        SetPlayerToSpawnPointClient(spawnPos);
    }

    [Rpc(SendTo.NotServer)]
    void StartServerAnimationRpc() {
       matchUIInternal.PlayerStartRaceAnimation();
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void PlayerCrossedLineRpc(RpcParams rpcParams = default) {
       LocalPlayerCrossedLine();
    }

    #endregion

    #region ServerRPCs
    [Rpc(SendTo.Server)]
    void CrossedLineRpc(ulong clientId) {
        MatchData newMatchData = GetCopy(matchData.Value);
        
        PlayerMatchData currentPlayerMatchData = newMatchData.playersInMatch.First(data=>data.clientId==clientId);

        int scoreToAdd = _winPosToPointsGivenDict[nextPlayerWinPos];
        newMatchData.UpdatePlayerMatchData(clientId,currentRoundTimer: currentRoundTimer,score: currentPlayerMatchData.score+scoreToAdd, currentRoundPlacement: nextPlayerWinPos);
        
        nextPlayerWinPos+=1;

        matchData.Value = newMatchData;

        if (hasWinner==false) FirstPlayerFinished();
        
        playersThatCrossedLine +=1;
        if (playersThatCrossedLine==newMatchData.PlayersInMatch) {
            hasEnded = true;
        }
    }

    [Rpc(SendTo.Server)]
    public void MarkReadyRpc(ulong clientId) {
        MatchData newMatchData = GetCopy(matchData.Value);

        newMatchData.UpdatePlayerMatchData(clientId,isPlayerReady: 1);

        matchData.Value = newMatchData;
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
                currentRoundTimer = 0,
                readyForNext = false
            });
        } 
    
        generatedMatchData.maxMatchRounds = 8;
        generatedMatchData.currentMatchRound = 1;
        generatedMatchData.stageId = 1;
    }

    public MatchData GenerateNextRoundMatchData() {
        MatchData newRoundData = GetCopy(matchData.Value);

        matchData.Value.playersInMatch.ForEach(p=>
        {
            newRoundData.UpdatePlayerMatchData(p.clientId,isPlayerReady: 0,currentRoundTimer: 0);
        });

        newRoundData.currentMatchRound+=1;
        newRoundData.stageId+=1;

        return newRoundData;
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
                original.playersInMatch[i].currentRoundTimer,
                original.playersInMatch[i].readyForNext
            ));
        }

        newCopy.currentMatchRound = original.currentMatchRound;
        newCopy.maxMatchRounds = original.maxMatchRounds;
        newCopy.stageId = original.stageId;

        return newCopy;
    }

    #endregion

}
