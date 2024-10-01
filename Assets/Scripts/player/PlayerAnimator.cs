using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Game {

    public class PlayerAnimator : NetworkBehaviour
    {
        [SerializeField] bool IsOnline;
        private Animator _anim;
        private NetworkAnimator _networkAnim;
        private SpriteRenderer _sprite;

        [SerializeField] private IPlayerController _player;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            string color = GameController.Singleton.match.matchData.Value.GetPlayerMatchData(OwnerClientId).playerColor.Value;
            _sprite.material = Util.getPlayerMaterialFromColor(color);
        }

        private void Awake()
        {
            _player = GetComponent<IPlayerController>();
            _networkAnim = GetComponent<NetworkAnimator>();
            _anim = GetComponent<Animator>();
            _sprite = GetComponent<SpriteRenderer>();
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
            if (IsOnline) {
                _networkAnim.SetTrigger("jump");
            } else {
                _anim.SetTrigger("jump");
            }
        }

        private void OnDashed()
        {
            if (IsOnline) {
                _networkAnim.SetTrigger("dash");
            } else {
                _anim.SetTrigger("dash");
            }
        }
    }
}
