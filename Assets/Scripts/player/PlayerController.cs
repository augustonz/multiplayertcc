using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game {
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : NetworkBehaviour, IPlayerController
    {
        [SerializeField] private ScriptableStats _stats;
        [SerializeField] private InputActionAsset iaa;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private BoxCollider2D _groundCollider;
        private BoxCollider2D _rightWallCollider;
        private BoxCollider2D _leftWallCollider;
        private FrameInput _frameInput;
        private Vector2 _frameVelocity;
        public bool _limitSpeedToMax = true;
        private bool _cachedQueryStartInColliders;
        [SerializeField] private GameObject _reconciliationGhostClient;
        [SerializeField] private GameObject _reconciliationGhostServer;
        [SerializeField] private bool _seeReconciliationGhosts;
        [SerializeField] private float serverTimeStep = 0.1f;
        float interpolationTimer;
        Vector3 interpolationPosTarget;
        Vector3 interpolationPosOrigin;
        private NetworkTimer tickTimer;
        private float tickRate = 50f;
        private const int buffer = 1024;
        [SerializeField] private float maxPosError;

        public InputState[] _inputStates = new InputState[buffer];
        private PlayerState[] _playerStates = new PlayerState[buffer];
        private Queue<InputState> _serverInputQueue = new ();
        public NetworkVariable<PlayerState> currentServerPlayerState = new NetworkVariable<PlayerState>();
        private PlayerState lastServerState;
        private PlayerState beforeLastServerState;
        private PlayerState lastProcessedState;

        #region Interface

        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action<bool> HitWallChanged;
        public event Action<int, bool> Jumped;
        bool ShouldGroundChanged;

        bool ShouldHitWallChanged;
        bool HitWallChangedIsEnterWall;

        bool ShouldJumped;
        bool JumpedIsWallJump;

        public event Action<float> GotPowerUp;
        public event Action Dashed;
        public bool IsSliding => _isSliding;
        public bool HasDoubleJump => _currentAirJumps > 0;
        public bool IsGrounded => _grounded;

        #endregion

        private float _time;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (IsOwner && !IsServer) {
                GameController.Singleton.match.SpawnedLocalPlayer(this);
                FindFirstObjectByType<CameraController>().FollowPlayer(OwnerClientId);
                currentServerPlayerState.OnValueChanged += OnServerStateChange;
            } else if (IsClient && !IsOwner) {
                currentServerPlayerState.OnValueChanged += OnServerStateChange;
            }
            interpolationPosTarget = transform.position;
            interpolationPosOrigin = transform.position;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
            _groundCollider = gameObject.transform.Find("GroundCollider").GetComponent<BoxCollider2D>();
            _rightWallCollider = gameObject.transform.Find("RightWallCollider").GetComponent<BoxCollider2D>();
            _leftWallCollider = gameObject.transform.Find("LeftWallCollider").GetComponent<BoxCollider2D>();
            
            Floorfilter.SetLayerMask(_stats.FloorLayer);
            Wallfilter.SetLayerMask(_stats.FloorLayer);

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
            tickTimer = new NetworkTimer(tickRate);
            Physics2D.simulationMode = SimulationMode2D.Script;

            _reconciliationGhostClient.SetActive(_seeReconciliationGhosts);
            _reconciliationGhostServer.SetActive(_seeReconciliationGhosts);
        }

        void OnServerStateChange(PlayerState prev,PlayerState curr) {
            lastServerState = curr;

            if (IsClient && IsLocalPlayer) {
                // if (!Variables.hasEntityInterpolation &&  receivedInput.moveInput == Vector2.zero && receivedInput.jumpDown == false && receivedInput.jumpHeld==false && receivedInput.dashDown==false) {
                //     Debug.Log($"Nothing received on tick {tick}");      
                // } else {
                // }

                if (!Variables.hasServerReconciliation) {
                    if (Variables.hasClientSidePrediction && !curr.inputMoved) return;

                    transform.position = curr.finalPos;
                    transform.rotation = curr.finalRot;
                    _rb.velocity = curr.finalSpeed;
                }

                if (!Variables.hasClientSidePrediction) {
                    if (currentServerPlayerState.Value.hasDashed) {
                        dashTimer = 0;
                    }
                }
            } else if (IsClient && !IsLocalPlayer) {
                lastServerState = curr;
                beforeLastServerState = prev;
            }

        }
        private void Update()
        {
            UpdateTimers();

            GatherInput();

            if (Variables.hasEntityInterpolation) {
                if (IsClient && !IsOwner) {
                    float interpolValue = interpolationTimer/serverTimeStep;
                    transform.position = Vector3.Lerp(interpolationPosOrigin,interpolationPosTarget,interpolValue);
                    if (interpolationTimer>serverTimeStep) {
                        interpolationPosTarget = lastServerState.finalPos;
                        interpolationPosOrigin = transform.position;
                        interpolationTimer-=serverTimeStep;
                    }
                }
            }
        }

        void UpdateTimers() {
            dashTimer +=Time.deltaTime;
            _time += Time.deltaTime;
            interpolationTimer += Time.deltaTime;
        }

        public void EnablePlayerInput(bool isEnabled) {
            if (!IsOwner) {
                iaa.Disable();
            }
            if (isEnabled) {
                iaa.Enable();
            } else {
                iaa.Disable();
            }
        }

        private void GatherInput()
        {
            _frameInput = new FrameInput
            {
                JumpDown = iaa["Player/Jump"].WasPerformedThisFrame(),
                JumpHeld = iaa["Player/Jump"].ReadValue<float>()!=0,
                Move = new Vector2(iaa["Player/Walk"].ReadValue<float>(),0),
                DashDown = iaa["Player/Dash"].WasPerformedThisFrame()
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

            if (_frameInput.DashDown) DashWasPressedOnUpdate = true;
        }

        public void HandleLocalClientTick() {

            int bufferIndex = tickTimer.CurrentTick % buffer;

            InputState currentInput = new() {
                tick = tickTimer.CurrentTick,
                moveInput = _frameInput.Move,
                jumpDown = _jumpToConsume,
                jumpHeld = _frameInput.JumpHeld,
                dashDown = DashWasPressedOnUpdate
            };

            _inputStates[bufferIndex] = currentInput;
            MoveOnServerRpc(currentInput);

            if (Variables.hasClientSidePrediction) {
                Move();
                Physics2D.Simulate(Time.fixedDeltaTime);
            }

            RemoveOldInputs();

            PlayerState currentPlayerState = new() {
                tick = tickTimer.CurrentTick,
                finalPos = transform.position,
                finalRot = transform.rotation,
                finalSpeed = _rb.velocity,
                inputMoved = !isTrivialInput(currentInput),
            };
            
            _playerStates[bufferIndex] = currentPlayerState;

            if (Variables.hasServerReconciliation) {

                HandleServerReconciliation();
            }
        }

        void HandleServerReconciliation() {
            if (!ShouldReconcile()) return;

            int bufferIndex = lastServerState.tick % buffer;
            PlayerState rewindState =  lastServerState;

            if (bufferIndex-1 < 0) return;
            
            float positionError = Vector3.Distance(rewindState.finalPos,_playerStates[bufferIndex].finalPos);

            _reconciliationGhostClient.transform.position = _playerStates[bufferIndex].finalPos;
            _reconciliationGhostServer.transform.position = rewindState.finalPos;

            if (positionError > maxPosError) {
                ReconcileState(rewindState);
            }

            lastProcessedState = lastServerState;
        }

        bool ShouldReconcile() {
            bool isNewServerState = !lastServerState.Equals(default);
            bool isLastStateUndefinedOrDifferent = lastProcessedState.Equals(default) || !lastProcessedState.Equals(lastServerState);
            return isLastStateUndefinedOrDifferent && isNewServerState;
        }

        void ReconcileState(PlayerState rewindState) {
            transform.position = rewindState.finalPos;
            transform.rotation = rewindState.finalRot;
            _rb.velocity = rewindState.finalSpeed;

            int bufferIndex = rewindState.tick % 1024;
            _playerStates[bufferIndex] = rewindState;

            int tickToReplay = rewindState.tick;
            while(tickToReplay < tickTimer.CurrentTick) {
                bufferIndex = tickToReplay % 1024;
                PlayerState newPlayerState = SimulateNewStateFromInput(_inputStates[bufferIndex]);
                _playerStates[bufferIndex] = newPlayerState;
                tickToReplay++;
            }
        }

        private void Move() {
            CheckCollisions();

            HandleWallJump();
            HandleJump();

            HandleSlide();
            HandleDash();
            HandleHorizontalMovement();
            HandleFacing();
            HandleGravity();
            
            ApplyMovement();
        }

        void RemoveOldInputs() {
            _jumpToConsume = false;
            DashWasPressedOnUpdate = false;
        }

        int lastReceivedTick;

        [Rpc(SendTo.Server)]
        private void MoveOnServerRpc(InputState receivedInput) {
            // if (Variables.hasEntityInterpolation) {
            //     _serverInputQueue.Enqueue(receivedInput);
            //     return;
            // }

            if (lastReceivedTick + 1 != receivedInput.tick) {
                Debug.LogError("Tick Received out of order");
            }
            lastReceivedTick = receivedInput.tick;

            int bufferIndex = receivedInput.tick % buffer;
            _inputStates[bufferIndex] = receivedInput;
            
            PlayerState currentPlayerState = ProcessNewStateFromInput(receivedInput);
            if(OwnerClientId==1) Physics2D.Simulate(Time.fixedDeltaTime);
            
            _playerStates[bufferIndex] = currentPlayerState;

            if (Variables.hasArtificialLag) {
                StartCoroutine(delayState(Ping.ArtificialWait,currentPlayerState));
            } else {
                lastServerState = currentServerPlayerState.Value;
                currentServerPlayerState.Value = currentPlayerState;
            }
            
        }

        IEnumerator delayState(double timeToWait,PlayerState newPlayerState) {
            yield return new WaitForSeconds((float)timeToWait);
            lastServerState = currentServerPlayerState.Value;
            currentServerPlayerState.Value = newPlayerState;
        }

        PlayerState ProcessNewStateFromInput(InputState input) {
            if (input.jumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }

            if (input.dashDown) DashWasPressedOnUpdate = true;

            _frameInput.Move = input.moveInput;
            _frameInput.JumpDown = input.jumpDown;
            _frameInput.JumpHeld = input.jumpHeld;
            _frameInput.DashDown = input.dashDown;

            Move();

            PlayerState newPlayerState = new() {
                tick = input.tick,
                finalPos = transform.position,
                finalRot = transform.rotation,
                finalSpeed = _rb.velocity,
                hasDashed = DashWasPressedOnUpdate && IsDashing,
                inputMoved = !isTrivialInput(input),

                lastInputDirection = input.moveInput,
                isSliding = _isSliding,
                isGrounded = _grounded,
                airJumps = _currentAirJumps,

                shouldTriggerGroundChange = ShouldGroundChanged,
                shouldTriggerHitWall = ShouldHitWallChanged,
                isEnteringWall = HitWallChangedIsEnterWall,
                shouldTriggerJumped = ShouldJumped,
                isWallJump = JumpedIsWallJump
            };

            RemoveOldInputs();

            return newPlayerState;
        }

        PlayerState SimulateNewStateFromInput(InputState input) {
            if (input.jumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }

            if (input.dashDown) DashWasPressedOnUpdate = true;

            _frameInput.Move = input.moveInput;
            _frameInput.JumpDown = input.jumpDown;
            _frameInput.JumpHeld = input.jumpHeld;
            _frameInput.DashDown = input.dashDown;

            Move();
            Physics2D.Simulate(Time.fixedDeltaTime);

            PlayerState newPlayerState = new() {
                tick = input.tick,
                finalPos = transform.position,
                finalRot = transform.rotation,
                finalSpeed = _rb.velocity,
                hasDashed = DashWasPressedOnUpdate && IsDashing,
                inputMoved = !isTrivialInput(input),
            };

            RemoveOldInputs();

            return newPlayerState;
        }

        bool isTrivialInput(InputState input) {
            return  input.moveInput == Vector2.zero && input.jumpDown == false && input.jumpHeld==false && input.dashDown==false;
        }

        // void HandleServerTick() {
        //     int bufferIndex = -1;
        //     InputState inputPayload = default;
        //     while (_serverInputQueue.Count > 0) {
        //         inputPayload = _serverInputQueue.Dequeue();
                
        //         bufferIndex = inputPayload.tick % buffer;
                
        //         PlayerState statePayload = SimulateNewStateFromInput(inputPayload);
        //         _playerStates[bufferIndex] = statePayload;
        //     }
            
        //     if (bufferIndex == -1) return;
        //     currentServerPlayerState.Value = _playerStates[bufferIndex];
        // }

        public void SimulateOtherClient() {
            if (!Variables.hasEntityInterpolation) {
                transform.position = currentServerPlayerState.Value.finalPos;
                transform.rotation = currentServerPlayerState.Value.finalRot;
                _rb.velocity = currentServerPlayerState.Value.finalSpeed;
                
                TriggerAnimations(currentServerPlayerState.Value);

                if (!Variables.hasClientSidePrediction) Physics2D.Simulate(Time.fixedDeltaTime);
            }
        }

        void TriggerAnimations(PlayerState state) {
            if (state.hasDashed) Dashed.Invoke();
            _frameInput.Move = state.lastInputDirection;
            _isSliding = state.isSliding;
            _grounded = state.isGrounded;
            _currentAirJumps = state.airJumps;

            Jumped.Invoke(_currentAirJumps,state.isWallJump);
            HitWallChanged.Invoke(state.isEnteringWall);
            GroundedChanged.Invoke(_grounded,0);
        }

        private void FixedUpdate()
        {
            tickTimer.Update(Time.fixedDeltaTime);

            while(tickTimer.ShouldTick()) {
                if (IsServer) {
                    if (Variables.hasEntityInterpolation) {
                        // HandleServerTick();
                    }
                }
                else if (IsClient && IsLocalPlayer) {
                    HandleLocalClientTick();
                } else if (IsClient){
                    SimulateOtherClient();
                }
            }
        }
        public void Die() {
            _frameVelocity = Vector2.zero;
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0;

            GameController.Singleton.match.PlayerDied(this);
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
            
            ShouldGroundChanged = false;
            // Landed on the Ground
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                _currentAirJumps = _stats.AirJumps;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
                ShouldGroundChanged = true;
            }
            // Left the Ground
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
                ShouldGroundChanged = true;
            }

            ShouldHitWallChanged = false;

            if ((rightWallHit || leftWallHit) && !isNextToWall) {
                isNextToWall = true;
                HitWallChanged(true);
                ShouldHitWallChanged = true;
                HitWallChangedIsEnterWall = true;
            } else if (!rightWallHit && !leftWallHit && isNextToWall) {
                isNextToWall = false;
                HitWallChanged(false);
                ShouldHitWallChanged = true;
                HitWallChangedIsEnterWall = false;
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
            ShouldJumped = false;
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
            ShouldJumped = true;
            JumpedIsWallJump = false;
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
            ShouldJumped = true;
            JumpedIsWallJump = true;
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
    }

    

    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public bool DashDown;
        public Vector2 Move;
    }

    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;
        public event Action<bool> HitWallChanged;
        public event Action<int, bool> Jumped;
        public event Action Dashed;
        public event Action<float> GotPowerUp;
        public Vector2 FrameInput { get;}
        public bool IsGrounded { get;}
        public bool IsSliding { get;}
        public bool HasDoubleJump { get;}
    }
}