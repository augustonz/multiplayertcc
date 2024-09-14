using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AudioManager : NetworkBehaviour
{
    public static AudioManager instance;
    [SerializeField] SerializableDict<string,AudioSource> _nameToAudioSerializable;
    Dictionary<string,AudioSource> _nameToAudio;
    public void Awake() {
        if (instance!=null) {
            Destroy(gameObject);
        }
        instance = this;
        DontDestroyOnLoad(this);
        _nameToAudio = _nameToAudioSerializable.ToDictionary();
    }

    public void PlaySFX(string audioName) {
        AudioSource audioSource = _nameToAudio[audioName];
        audioSource.Play();
    }

    public void PlayerSFXWithoutOverlap(string audioName) {
        AudioSource audioSource = _nameToAudio[audioName];
        if (!audioSource.isPlaying) audioSource.Play();
    }
}