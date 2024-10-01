using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game {
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerControllerOffline : MonoBehaviour, IPlayerController
    {
        [SerializeField] private ScriptableStats _stats;
        [SerializeField] private InputActionAsset iaa;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private BoxCollider2D _groundCollider;
        private BoxCollider2D _rightWallCollider;
        private BoxCollider2D _leftWallCollider;
        private FrameInputOffline _frameInput;
        private Vector2 _frameVelocity;
        public bool _limitSpeedToMax = true;
        private bool _cachedQueryStartInColliders;

        #region Interface

        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action<bool> HitWallChanged;
        public event Action<int, bool> Jumped;
        public event Action<float> GotPowerUp;
        public event Action Dashed;
        public bool IsSliding => _isSliding;
        public bool HasDoubleJump => _currentAirJumps > 0;
        public bool IsGrounded => _grounded;

        #endregion

        private float _time;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
            _groundCollider = gameObject.transform.Find("GroundCollider").GetComponent<BoxCollider2D>();
            _rightWallCollider = gameObject.transform.Find("RightWallCollider").GetComponent<BoxCollider2D>();
            _leftWallCollider = gameObject.transform.Find("LeftWallCollider").GetComponent<BoxCollider2D>();
            
            Floorfilter.SetLayerMask(_stats.FloorLayer);
            Wallfilter.SetLayerMask(_stats.FloorLayer);
            // Wallfilter.SetLayerMask(_stats.WallLayer);

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        }

        void Start() {
            FindFirstObjectByType<CameraController>().FollowPlayerOffline();
        }

        private void Update()
        {
            UpdateTimers();

            GatherInput();
        }

        void UpdateTimers() {
            dashTimer +=Time.deltaTime;
            _time += Time.deltaTime;
        }

        public void EnablePlayerInput(bool isEnabled) {
            if (isEnabled) {
                iaa.Enable();
            } else {
                iaa.Disable();
            }
        }

        private void GatherInput()
        {
            _frameInput = new FrameInputOffline
            {
                JumpDown = iaa["Player/Jump"].WasPerformedThisFrame(),
                JumpHeld = iaa["Player/Jump"].ReadValue<float>()!=0,
                Move = new Vector2(iaa["Player/Walk"].ReadValue<float>(),0),
                Dash = iaa["Player/Dash"].WasPerformedThisFrame()
            };

            if (_stats.SnapInput)
            {
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            }

            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }

            if (_frameInput.Dash) DashWasPressedOnUpdate = true;
        }

        private void FixedUpdate()
        {
            CheckCollisions();

            HandleWallJump();
            HandleJump();
            _jumpToConsume = false;

            HandleSlide();
            HandleDash();
            HandleHorizontalMovement();
            HandleFacing();
            HandleGravity();
            
            ApplyMovement();
        }

        public void Die() {
            _frameVelocity = Vector2.zero;
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0;

            transform.position = Vector2.zero;
        }

        #region Collisions
        
        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;
        private ContactFilter2D Floorfilter;
        private ContactFilter2D Wallfilter;
        private bool isFacingRight = true;

        private void CheckCollisions()
        {
            // Ground, Wall and Ceiling
            bool rightWallHit = _rightWallCollider.OverlapCollider(Wallfilter,new List<Collider2D>()) > 0;
            bool leftWallHit = _leftWallCollider.OverlapCollider(Wallfilter,new List<Collider2D>()) > 0;

            bool groundHit = _groundCollider.OverlapCollider(Floorfilter,new List<Collider2D>()) > 0;
            bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance,_stats.FloorLayer);
            
            // Hit a Ceiling
            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);
            
            // Landed on the Ground
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                _currentAirJumps = _stats.AirJumps;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            // Left the Ground
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
            }

            if ((rightWallHit || leftWallHit) && !isNextToWall) {
                isNextToWall = true;
                HitWallChanged(true);
            } else if (!rightWallHit && !leftWallHit && isNextToWall) {
                isNextToWall = false;
                HitWallChanged(false);
            }

            if (((rightWallHit && isFacingRight) || (leftWallHit && !isFacingRight)) && !IsWallJumping) {
                _frameLastRightWall = _time;
            } 
            if (((leftWallHit && isFacingRight) || (rightWallHit && !isFacingRight)) && !IsWallJumping) {
                _frameLastLeftWall = _time;
            }

            _frameLastOnWall = Mathf.Max(_frameLastRightWall,_frameLastLeftWall);

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }

        #endregion


        #region Jumping

        private int _currentAirJumps;
        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer && !IsDashing;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;
        private bool CanAirJump => !JustFinishedSliding && !_grounded && !CanUseCoyote && _currentAirJumps > 0;

        private void HandleJump()
        {
            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump) return;

            if (_grounded || CanUseCoyote || CanAirJump) {
                ExecuteJump();
            }
        }

        private void ExecuteJump()
        {
            if (CanAirJump) _currentAirJumps--;
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = _stats.JumpPower;
            Jumped?.Invoke(_currentAirJumps,false);
        }

        #endregion

        #region Wall Jumping
        bool IsWallJumping;
        bool isNextToWall;
        float _frameLastRightWall;
        float _frameLastLeftWall;
        float _frameLastOnWall;
        float _wallJumpStartTime;
        bool CanWallJump => _isSliding;

        private void HandleWallJump() {
            if (IsWallJumping && _time > _wallJumpStartTime + _stats.wallJumpDuration) IsWallJumping = false;

            if (!_jumpToConsume || !HasBufferedJump) return;

            if (JustFinishedSliding) {
                ExecuteWallJump();
            }
        }

        private void ExecuteWallJump() {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;

            int jumpDir = _frameLastRightWall < _frameLastLeftWall ? 1 : -1;

            Vector2 newVelocity = new Vector2(_stats.wallJumpHorizontalForce,_stats.wallJumpVerticalForce);
            newVelocity.x *= jumpDir;
            _frameVelocity = newVelocity;
            IsWallJumping = true;
            _wallJumpStartTime=_time;
            Jumped?.Invoke(_currentAirJumps,true);
        }

        #endregion

        #region Sliding
        float _lastTimeSliding;
        bool _isSliding;

        bool JustFinishedSliding => _time < _lastTimeSliding + _stats.slidingToWallJumpBuffer;
        bool CanSlide => !IsWallJumping && !IsDashing && !_grounded && _time <= _frameLastOnWall + 0.1f;
        void HandleSlide() {
            if (CanSlide && ((_time <= _frameLastLeftWall + 0.1f && _frameInput.Move.x < 0) || (_time <= _frameLastRightWall + 0.1f && _frameInput.Move.x > 0))) {
                _isSliding = true;
                _bufferedJumpUsable=true;
                _lastTimeSliding = _time;
            } else {
                _isSliding = false;
            }
        }
		#endregion

        #region Dashing
        private bool HasDashAvailable => dashTimer >= _stats.dashCooldown;
        public float DashFillPercentage => Mathf.Min(1,dashTimer/_stats.dashCooldown);
        private bool IsDashing;
        private bool DashWasPressedOnUpdate;
        private float dashTimer;
        private bool ShouldDash => !IsDashing && HasDashAvailable && DashWasPressedOnUpdate;
        private void HandleDash() {
            if (ShouldDash) ExecuteDash();
            DashWasPressedOnUpdate = false;
        }
        [Rpc(SendTo.Owner)]
        public void AddDashPercRpc(float perc)
        {
            dashTimer += perc * _stats.dashCooldown;
            GotPowerUp.Invoke(perc);
        }

        private void ExecuteDash() {
            Dashed.Invoke();
            Vector2 dashDirection = Vector2.zero;
            if (_frameInput.Move.x==0 || _isSliding){
                dashDirection = isFacingRight ? Vector2.right : Vector2.left;
            } else if (_frameInput.Move.x != 0) {
                dashDirection = _frameInput.Move.x > 0 ? Vector2.right : Vector2.left;
            }

			IsDashing = true;

			StartCoroutine(nameof(StartDash), dashDirection);
        }

        private IEnumerator StartDash(Vector2 dir) {
            _frameVelocity = dir * _stats.dashSpeed;
            dashTimer = 0;

            yield return new WaitForSeconds(_stats.dashDuration);

            IsDashing = false;
        }

        #endregion

        #region Horizontal

        private void HandleHorizontalMovement()
        {
            if (IsDashing) return;
            if (!_limitSpeedToMax) return;
            if (_isSliding) {
                _frameVelocity.x = 0;
                return;
            }
            if (_frameInput.Move.x == 0)
            {
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        private void HandleFacing()
        {
            if (IsDashing) return;

            if (_isSliding) {
                if (_frameLastRightWall < _frameLastLeftWall) {
                    TurnRight();
                } else if (_frameLastRightWall > _frameLastLeftWall) {
                    TurnLeft();
                }
            } else if (_frameVelocity.x > 0 && !isFacingRight) TurnRight();
            else if (_frameVelocity.x < 0 && isFacingRight) TurnLeft();
        }

        #endregion

        #region Gravity

        private void HandleGravity()
        {
            if (IsDashing) {
                _frameVelocity.y = 0;
                return;
            }
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
            } else if (_isSliding && !_grounded) 
            {
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.slidingSpeed, _stats.slidingDesaccel* Time.fixedDeltaTime);
            }
            else
            {
                var inAirGravity = _stats.FallAcceleration;
                if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            }
        }

        #endregion

        private void Turn() {
            Vector3 scale = transform.localScale; 
            scale.x *= -1;
            transform.localScale = scale;

            isFacingRight = !isFacingRight;
        }

        void TurnRight() {
            Vector3 scale = transform.localScale; 
            scale.x = 1;
            transform.localScale = scale;
            isFacingRight = true;
        }

        void TurnLeft() {
            Vector3 scale = transform.localScale; 
            scale.x = -1;
            transform.localScale = scale;
            isFacingRight = false;
        }

        private void ApplyMovement() => _rb.velocity = _frameVelocity;

    #if UNITY_EDITOR
        private void OnValidate()
        {
            if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
        }
    #endif
    }

    public struct FrameInputOffline
    {
        public bool JumpDown;
        public bool JumpHeld;
        public bool Dash;
        public Vector2 Move;
    }
}