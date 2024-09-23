using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Game;
public static class UIActions
{
    static Dictionary<string,UnityAction> ActionsDic = new Dictionary<string, UnityAction>() {
        { "emptyAction",EmptyAction},
        { "exitGame",ExitGame},
        { "startServer",StartServer},
        { "leaveMatch",LeaveMatch},
        { "joinMatch",JoinMatch},
        { "endMatch",EndMatch},
        { "nextRaceReady",ReadyForNextRace},
        { "joinOffline",JoinOffline},
        { "leaveOffline",LeaveOffline},
    };

    static void EmptyAction() {
        Debug.Log("Button with empty action");
    }

    static void EndMatch() {
        GameController.Singleton.match.FinishRound();
    }


    static void StartServer() {
        GameController.Singleton.MyNetworkManager.StartServer();
    }

    static void ExitGame() {
        GameController.Singleton.MySceneManager.ExitGame();
    }

    static void JoinMatch() {
        GameController.Singleton.MyNetworkManager.JoinServer();
        GameController.Singleton.MySceneManager.AddEnterSceneCallback("Menu",GameController.Singleton.MyNetworkManager.Shutdown);
    }

    static void LeaveMatch() {
        GameController.Singleton.MySceneManager.ChangeSceneRpc("Menu");
    }

    static void JoinOffline() {
        GameController.Singleton.MySceneManager.ChangeSceneOffline("PhysiscTest");
    }

    static void LeaveOffline() {
        GameController.Singleton.MySceneManager.ChangeSceneOffline("Menu");
    }

    static void ReadyForNextRace() {
        GameController.Singleton.match.ReadyForNextRound();
    }

    public static UnityAction getActionByName(string actionName) {
        return ActionsDic[actionName];
    }
}
