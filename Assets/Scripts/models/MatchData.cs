using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

public struct MatchData : INetworkSerializable, System.IEquatable<MatchData>
{
    public ulong stageId;
    public List<PlayerMatchData> playersInMatch;
    public ulong currentMatchRound;
    public ulong maxMatchRounds;
    public int PlayersInMatch => playersInMatch.Count();

    public bool Equals(MatchData other)
    {
        if (other.playersInMatch==null || playersInMatch == null) return false;
        
        return Util.IsEqualLists(playersInMatch,other.playersInMatch) && stageId==other.stageId && currentMatchRound==other.currentMatchRound && maxMatchRounds==other.maxMatchRounds;
    }
    public int PlayersReadyForNext => playersInMatch.Where(data=>data.readyForNext).Count(); 

    public void UpdatePlayerMatchData(ulong clientId, string playername = "", string playerColor = "blue", int score = -1, int currentRoundPlacement = -1, float currentRoundTimer = -1, int isPlayerReady = -1) {
        int index = playersInMatch.FindIndex(matchData=>matchData.clientId==clientId);
        PlayerMatchData interm = playersInMatch[index];

        if (playername!="") interm.playerName = playername;
        if (playerColor!="blue") interm.playerColor = playerColor;
        if (score!=-1) interm.score = score;
        if (currentRoundPlacement!=-1) interm.currentRoundPlacement = currentRoundPlacement;
        if (currentRoundTimer!=-1) interm.currentRoundTimer = currentRoundTimer;
        if (isPlayerReady!=-1) interm.readyForNext = isPlayerReady==1;
        
        playersInMatch[index] = interm;
    }

    public PlayerMatchData GetPlayerMatchData(ulong clientId) {
        return playersInMatch.Find(matchData=>matchData.clientId==clientId);
    }

    public static MatchData Empty() {
        MatchData empty = new MatchData
        {
            playersInMatch = new List<PlayerMatchData>(),
            currentMatchRound = 0,
            maxMatchRounds = 0,
            stageId = 0
        };
        return empty;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();

            reader.ReadValueSafe(out stageId);
            reader.ReadValueSafe(out currentMatchRound);
            reader.ReadValueSafe(out maxMatchRounds);

            int playersCount;
            reader.ReadValueSafe(out playersCount);
            List<PlayerMatchData> playersList = new List<PlayerMatchData>();
            for (int i = 0; i < playersCount; i++)
            {
                PlayerMatchData newPlayer;
                reader.ReadValueSafe(out newPlayer);
                playersList.Add(newPlayer);
            }
            playersInMatch = playersList;
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();

            writer.WriteValueSafe(stageId);
            writer.WriteValueSafe(currentMatchRound);
            writer.WriteValueSafe(maxMatchRounds);

            writer.WriteValueSafe(playersInMatch.Count);
            foreach (PlayerMatchData player in playersInMatch)
            {
                writer.WriteValueSafe(player);
            }
        }
    }

    public override string ToString()
    {
        string ans = $"playersList: \n";
        foreach (var item in playersInMatch)
        {
            ans+=$"{item}, ";
        }
        return ans;
    }
}