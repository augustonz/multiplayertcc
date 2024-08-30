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
        { "spawnPlayer",SpawnPlayer},
        { "endMatch",EndMatch},
        { "crossLine",CrossLine},
    };

    static void EmptyAction() {
        Debug.Log("Button with empty action");
    }

    static void SpawnPlayer() {
        Vector3 spawnPos =  new Vector3(0,1,0);
        SpawnPoint spawnInScene = Object.FindAnyObjectByType<SpawnPoint>();
        if (spawnInScene != null) {
            spawnPos =  spawnInScene.transform.position;
        }
        GameController.Singleton.MyNetworkManager.SpawnPlayer(spawnPos);
    }

    static void EndMatch() {
        GameController.Singleton.match.FinishRound();
    }

    static void CrossLine() {
        GameController.Singleton.match.PlayerCrossedLine(MyNetworkManager.Singleton.LocalClientId);
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
