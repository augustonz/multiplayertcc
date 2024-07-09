using System.Collections.Generic;
using System.Linq;

public class LobbyData {
    public ulong localClientId { get; set; }
    public List<ulong> clientIdsInLobby { get; set; }
    public List<PlayerCaseData> playerCaseDatas {get; set;}
    public int PlayersInLobby => clientIdsInLobby.Count;
    public int ReadyPlayers => playerCaseDatas.FindAll(data=>data.isReady).Count;
    public bool IsLocalPlayerReady => playerCaseDatas.Exists(data=>data.clientId==localClientId && data.isReady);

    public LobbyData() {
        clientIdsInLobby = new List<ulong>();
        playerCaseDatas = new List<PlayerCaseData>
        {
            new PlayerCaseData(),
            new PlayerCaseData(),
            new PlayerCaseData(),
            new PlayerCaseData(),
            new PlayerCaseData(),
            new PlayerCaseData()
        };
    }

    public void addClientToPlayerCaseDatas(ulong clientId) {
        PlayerCaseData emptyCase = playerCaseDatas.Find(data=>{
            return data.clientId==0;
        });
        emptyCase.clientId = clientId;
    }

    public void removeClientFromPlayerCaseDatas(ulong clientId) {
        PlayerCaseData oldCase = playerCaseDatas.Find(data=>{
            return data.clientId==clientId;
        });
        oldCase = new PlayerCaseData();
    }
}