using UnityEngine;

public class PersistenceManager {


    public static float GetVolume(string sliderName) {
        return PlayerPrefs.GetFloat($"volume:{sliderName}",-30);
    }

    public static void SetVolume(string sliderName, float value) {
        PlayerPrefs.SetFloat($"volume:{sliderName}",value);
    }

}