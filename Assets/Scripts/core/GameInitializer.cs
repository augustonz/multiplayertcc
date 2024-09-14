using UnityEngine;

public class GameInitializer : MonoBehaviour {

    [SerializeField] MyNetworkManager networkManager;
    [SerializeField] MySceneManager scenesManager;
    [SerializeField] OptionsManager optionsManager;

    void Awake() {
        if (GameController.Singleton!=null) {
            Destroy(networkManager.gameObject);
            Destroy(scenesManager.gameObject);
            Destroy(gameObject);
        }
        GameController gc = new GameController(networkManager,scenesManager);
        gc.createSingleton();

        optionsManager.GetInitialSoundValues();
    }
}
