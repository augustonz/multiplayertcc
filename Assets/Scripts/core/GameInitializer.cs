using Game;
using UnityEngine;

public class GameInitializer : MonoBehaviour {

    [SerializeField] MyNetworkManager networkManager;
    [SerializeField] MySceneManager scenesManager;
    [SerializeField] OptionsManager optionsManager;
    [SerializeField] bool artificialLatency;
    [SerializeField] bool clientPrediction;
    [SerializeField] bool serverReconciliation;
    [SerializeField] bool entityInterpolation;

    void Awake() {
        if (GameController.Singleton!=null) {
            Destroy(networkManager.gameObject);
            Destroy(scenesManager.gameObject);
            Destroy(gameObject);
        }
        GameController gc = new GameController(networkManager,scenesManager);
        gc.createSingleton();
        optionsManager.GetInitialVideoValues();

        Variables.hasArtificialLag = artificialLatency;
        Variables.hasClientSidePrediction = clientPrediction;
        Variables.hasServerReconciliation = serverReconciliation;
        Variables.hasEntityInterpolation = entityInterpolation;
    }
}
