using UnityEngine;

public class GameInitializer : MonoBehaviour {

    [SerializeField] MyNetworkManager networkManager;
    [SerializeField] MySceneManager scenesManager;

    void Start() {
        if (GameController.Singleton!=null) {
            Destroy(networkManager.gameObject);
            Destroy(scenesManager.gameObject);
            Destroy(gameObject);
        }
        GameController gc = new GameController(networkManager,scenesManager);
        gc.createSingleton();
    }
}
