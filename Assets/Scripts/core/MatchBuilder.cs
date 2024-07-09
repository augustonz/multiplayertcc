using UnityEngine;

public class MatchBuilder
{
    MyNetworkManager MyNetworkManager { get; set; }
    MySceneManager MySceneManager { get; set; }

    MatchBuilder gameController;

    public MatchBuilder() {
        gameController = null;
    }

}
