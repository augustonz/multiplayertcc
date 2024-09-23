using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[System.Serializable]
public class ScreenResolution {
    public int Height;
    public int Width;
}
public class OptionsManager: MonoBehaviour {

    private float _masterVolume; 
    private float _sfxVolume; 
    private float _musicVolume;

    [SerializeField] Slider _sfxSlider;
    [SerializeField] Slider _masterSlider;
    [SerializeField] Slider _musicSlider;
    [SerializeField] AudioMixerGroup _audioMixerGroup;
    [SerializeField] Toggle _isFullscreen;
    [SerializeField] TMP_Dropdown _resolutionDropdown;
    [SerializeField] List<ScreenResolution> _resolutions;

    bool isFirstTimeChangingValues = true;
    bool isFirstSecondTimeChangingValues = true;

    void Start() {
        _isFullscreen.isOn = PlayerPrefs.GetInt("isFullscreen",1) == 1 ;

        _resolutions.ForEach((res)=>{
            _resolutionDropdown.AddOptions(new List<TMP_Dropdown.OptionData> {
                new TMP_Dropdown.OptionData {
                    text = $"{res.Width} x {res.Height}"
                }
            });
        });

        _resolutionDropdown.value = PlayerPrefs.GetInt("resolution",0);
    }

    public void GetInitialVideoValues() {
        Screen.fullScreen = PlayerPrefs.GetInt("isFullscreen",1) == 1 ;
        ScreenResolution resolution  = _resolutions[PlayerPrefs.GetInt("resolution",0)];
        Screen.SetResolution(resolution.Width,resolution.Height,Screen.fullScreenMode);
    }

    public void GetInitialSoundValues() {
        _masterVolume = PersistenceManager.GetVolume("master");
        _sfxVolume = PersistenceManager.GetVolume("sfx");
        _musicVolume = PersistenceManager.GetVolume("music");

        OnSFXChanged(_sfxVolume);
        OnMasterChanged(_masterVolume);
        OnMusicChanged(_musicVolume);
    }

    public void UpdateFullscreen(bool newValue) {
        Screen.fullScreen = newValue;
        PlayerPrefs.SetInt("isFullscreen",newValue?1:0);
    }

    public void UpdateResolution(int newIndex) {
        PlayerPrefs.SetInt("resolution",newIndex);
        ScreenResolution resolution = _resolutions[newIndex];
        Screen.SetResolution(resolution.Width,resolution.Height,Screen.fullScreenMode);
    }

    public void OnSFXChanged(float newValue) {
        PersistenceManager.SetVolume("sfx",newValue);
        _audioMixerGroup.audioMixer.SetFloat("volumeSFX",newValue);
        _sfxSlider.value = newValue;
        if (isFirstTimeChangingValues) {
            isFirstTimeChangingValues = false;
            return;
        }
        if (isFirstSecondTimeChangingValues) {
            isFirstSecondTimeChangingValues = false;
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