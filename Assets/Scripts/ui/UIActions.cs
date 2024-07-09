using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class UIActions
{
    static Dictionary<string,UnityAction> ActionsDic = new Dictionary<string, UnityAction>() {
        { "emptyAction",EmptyAction},
        { "exitGame",ExitGame},
        { "startServer",StartServer},
        { "leaveMatch",LeaveMatch},
        { "joinMatch",JoinMatch},
        { "spawnPlayer",SpawnPlayer},
    };

    static void EmptyAction() {
        Debug.Log("Button with empty action");
    }

    static void SpawnPlayer() {
        GameController.Singleton.MyNetworkManager.SpawnPlayer();
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

    public static UnityAction getActionByName(string actionName) {
        return ActionsDic[actionName];
    }
}
