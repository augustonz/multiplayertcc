using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static Unity.Netcode.NetworkSceneManager;

public class MySceneManager : NetworkBehaviour
{

    public Scene CurrentScene { get => SceneManager.GetActiveScene(); }

    Dictionary<string,OnEventCompletedDelegateHandler> enterEvents = new Dictionary<string,OnEventCompletedDelegateHandler>();
    Dictionary<string,OnEventCompletedDelegateHandler> exitEvents = new Dictionary<string,OnEventCompletedDelegateHandler>();

    public void Start() {
        DontDestroyOnLoad(this);
    }

    [Rpc(SendTo.Server)]
    public void ChangeSceneRpc(string sceneName) {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName,LoadSceneMode.Single);
    }

    [Rpc(SendTo.Server)]
    public void AddSceneRpc(string sceneName) {
        SceneEventProgressStatus s =  NetworkManager.Singleton.SceneManager.LoadScene(sceneName,LoadSceneMode.Additive);
    }
   public void AddExitSceneCallback(string sceneName,UnityAction callback) {
        OnEventCompletedDelegateHandler cb = (scene,loadSceneMode,clientsCompleted,clientsTimedout) => {
            if (scene==sceneName) callback();
        };
        
        if (exitEvents.ContainsKey(sceneName)) {
            NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted -= exitEvents[sceneName];
        }

        exitEvents[sceneName] = cb;
        NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted += cb;
    }

    public void AddEnterSceneCallback(string sceneName,UnityAction callback) {
        OnEventCompletedDelegateHandler cb = (scene,loadSceneMode,clientsCompleted,clientsTimedout) => {
            if (scene==sceneName) callback();
        };
        
        if (enterEvents.ContainsKey(sceneName)) {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= enterEvents[sceneName];
        }

        enterEvents[sceneName] = cb;
       NetworkManager.Singleton. SceneManager.OnLoadEventCompleted += cb;
    } 

    public void ExitGame() {
        Application.Quit();
    }
    
}
