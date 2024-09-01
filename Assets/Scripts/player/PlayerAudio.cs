using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
    public class PlayerAudio : MonoBehaviour
    {

        [SerializeField] SerializableDict<string,AudioSource> _nameToAudioSerializable;
        Dictionary<string,AudioSource> _nameToAudio;
        private IPlayerController _player;

        public void Awake() {
            _player = GetComponent<IPlayerController>();
            _nameToAudio = _nameToAudioSerializable.ToDictionary();
        }

         private void OnEnable()
        {
            _player.Jumped += OnJumped;
            _player.Dashed += OnDashed;
            _player.GroundedChanged += OnGroundedChanged;
            _player.GotPowerUp += GotPowerUp;
            _player.HitWallChanged += OnWalledChanged;
        }

        private void OnDisable()
        {
            _player.Jumped -= OnJumped;
            _player.Dashed -= OnDashed;
            _player.GroundedChanged -= OnGroundedChanged;
            _player.GotPowerUp += GotPowerUp;
            _player.HitWallChanged -= OnWalledChanged;

        }

        private void OnJumped(int airJumpsLeft, bool isWallJump)
        {
            PlaySFX("jump");
        }

        private void OnDashed()
        {
            PlaySFX("dash");
        }

        private void OnGroundedChanged(bool grounded, float impact)
        {
            if (grounded) PlaySFX("fall");
        }

        private void GotPowerUp(float perc) {
            PlaySFX("powerup");
        }

        private void OnWalledChanged(bool isWalled) {
            if (isWalled) PlaySFX("fall");
        }

        public void Step() {
            int stepNum = UnityEngine.Random.Range(1,4);
            PlaySFX($"step{stepNum}");
        }

        public void PlaySFX(string audioName) {

            AudioSource audioSource = _nameToAudio[audioName];
            audioSource.Play();
        }
    }
}
