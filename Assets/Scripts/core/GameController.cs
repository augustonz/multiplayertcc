using UnityEngine;

public class GameController
{
    public MyNetworkManager MyNetworkManager { get; }
    public MySceneManager MySceneManager { get; }
    public Lobby lobby { get; set; }
    public static GameController Singleton;
    public bool IsMatch { get; private set; }
    public bool IsLobby { get; private set; }
    public GameController(MyNetworkManager networkManager, MySceneManager sceneManager) {
        MyNetworkManager = networkManager;
        MySceneManager = sceneManager;
    }

    public void StartedLobby() {
        IsLobby = true;
        IsMatch = false;
    }

    public void createSingleton() {
        if (Singleton!= null) return;
        Singleton = this;
    }
}
