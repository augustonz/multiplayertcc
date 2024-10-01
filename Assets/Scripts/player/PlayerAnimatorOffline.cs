using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Game {

    public class PlayerAnimatorOffline : MonoBehaviour
    {
        private Animator _anim;

        private IPlayerController _player;

        private void Awake()
        {
            _player = GetComponent<IPlayerController>();
            _anim = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            _player.Jumped += OnJumped;
            _player.Dashed += OnDashed;
        }

        private void OnDisable()
        {
            _player.Jumped -= OnJumped;
            _player.Dashed -= OnDashed;
        }

        private void Update()
        {
            if (_player == null) return;
            _anim.SetBool("isMoving",_player.FrameInput.x!=0);
            _anim.SetBool("isGrounded",_player.IsGrounded);
            _anim.SetBool("isWall",_player.IsSliding);
            _anim.SetBool("hasDoubleJump",_player.HasDoubleJump);
        }

        private void OnJumped(int airJumpsLeft, bool isWallJump)
        {
            if (airJumpsLeft==0 || isWallJump) return;
            
            _anim.SetTrigger("jump");
            
        }

        private void OnDashed()
        {
            _anim.SetTrigger("dash");
            
        }
    }
}
