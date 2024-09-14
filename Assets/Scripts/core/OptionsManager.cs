using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class OptionsManager: MonoBehaviour {

    private float _masterVolume; 
    private float _sfxVolume; 
    private float _musicVolume;

    [SerializeField] Slider _sfxSlider;
    [SerializeField] Slider _masterSlider;
    [SerializeField] Slider _musicSlider;
    [SerializeField] AudioMixerGroup _audioMixerGroup;

    bool isFirstTimeChangingValues = true;

    public void GetInitialSoundValues() {
        _masterVolume = PersistenceManager.GetVolume("master");
        _sfxVolume = PersistenceManager.GetVolume("sfx");
        _musicVolume = PersistenceManager.GetVolume("music");

        OnSFXChanged(_sfxVolume);
        OnMasterChanged(_masterVolume);
        OnMusicChanged(_musicVolume);
    }

    public void OnSFXChanged(float newValue) {
        PersistenceManager.SetVolume("sfx",newValue);
        _audioMixerGroup.audioMixer.SetFloat("volumeSFX",newValue);
        _sfxSlider.value = newValue;
        if (isFirstTimeChangingValues) {
            isFirstTimeChangingValues = false;
            return;
        }
        TestSFXVolume();
    }

    public void TestSFXVolume() {
        AudioManager.instance.PlayerSFXWithoutOverlap("jumpTest");
    }

    public void OnMasterChanged(float newValue) {
        PersistenceManager.SetVolume("master",newValue);
        _audioMixerGroup.audioMixer.SetFloat("volumeMaster",newValue);
        _masterSlider.value = newValue;
    }

    public void OnMusicChanged(float newValue) {
        PersistenceManager.SetVolume("music",newValue);
        _audioMixerGroup.audioMixer.SetFloat("volumeMusic",newValue);
        _musicSlider.value = newValue;
    }
}