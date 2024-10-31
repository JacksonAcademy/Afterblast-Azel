using FishNet.Object.Prediction;
using FishNet.Object;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Input;

[System.Serializable]
public class controllerScale 
{
    public Vector3 center;
    public float height;
    public controllerScale(Vector3 Center, float Height)
    {
        center = Center;
        height = Height;
    }

}
public enum MoveState 
{
    None = 0,
    Idle = 1,
    Moving = 2,
    Sprinting = 4,
    Crouching = 8,
    Jump = 16,
    Grounded = 32,
    Ceiling = 64,
    Sliding = 128,
    Aiming = 256,
    WallRunning = 512
}
public struct ReplicateData : IReplicateData
{
    public MoveState moveStates;
    public Vector2 MoveInput;
    public Vector2 RotInput;
    public Vector3 Forward, Right;
    public ReplicateData(Vector2 moveInput, Vector2 rotInput, Vector3 right, Vector3 forward, MoveState eoveState)
    {
        moveStates = eoveState;
        MoveInput = moveInput;
        RotInput = rotInput;
        Forward = forward;
        Right = right;
        _tick = 0;
    }

    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}
public struct ReconcileData : IReconcileData
{
    public Vector3 Position;

    public Vector3 SlideDirection;

    public Vector3 WallRunVector;
    public float SlopeAngle;
    public bool Sprinting;
    public bool Sliding;
    public bool WallRunning;
    public bool Grounded;
    public bool Jumping;
    public float VerticalVelocity;
    public float SlideForce;

    public ReconcileData(Vector3 position, Vector3 wallRunVector, float verticalVelocity, float slideForce, float slopeAngle, Vector3 slideDirection, 
        bool sprinting, bool sliding, bool grounded, bool wallRunning, bool jumping)
    {
        Position = position;
        SlideForce = slideForce;
        VerticalVelocity = verticalVelocity;
        Grounded = grounded;
        SlopeAngle = slopeAngle;
        Jumping = jumping;
        Sliding = sliding;
        Grounded = grounded;
        SlideDirection = slideDirection;
        WallRunning = wallRunning;
        Sprinting = sprinting;
        WallRunVector = wallRunVector;
        _tick = 0;
    }

    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}

public class PlayerMovement : NetworkBehaviour
{
    [HideInInspector] public bool canMove;

    [Header("Movement Settings")]
    [SerializeField]
    [Tooltip("How high the player can jump.")]
    private float _jumpForce = 15f;
    [SerializeField]
    [Tooltip("How fast the player can walk.")]
    private float _walkSpeed;
    [SerializeField]
    [Tooltip("How fast the player can sprint.")]
    private float _sprintSpeed;
    [SerializeField]
    [Tooltip("How fast the player can move while courching.")]
    private float _crouchSpeed;
    [SerializeField]
    [Tooltip("Falling speed.")]
    private float _gravityScale;

    [SerializeField]
    [Tooltip("Mouse sensitivity scale.")]
    private float _mouseSensivity;
    [SerializeField]
    [Tooltip("How much the slope of the ground affects the sliding speed.")]
    private float _slopeAngleMultiplier;
    [SerializeField]
    [Tooltip("Max sliding angle of ground")] 
    private float maxSlideAngle = 45;
    [SerializeField]
    [Tooltip("Time before player can jump again.")]
    private float _jumpReload = 0.5f;
    [SerializeField]
    [Tooltip("Time before player can slide again.")]
    private float _slideReload = 1;

    [Header("Wall Run Settings")]
    [Tooltip("Player wall run speed.")] 
    public float wallRunSpeedMultiplier = 1;
    [Tooltip("Max time the player can wall run.")] 
    public float maxWallRunTime;
    [Tooltip("Max distance player can be from a wall.")] 
    public float wallCheckDistance;
    [Tooltip("How much friction you have when you wall jump.")] 
    public float wallJumpFriction;
    [Tooltip("How much force to stick to the wall.")] 
    public float wallStickForce;
    [Tooltip("How much gravity affects the player during wall running.")]
    [Range(0f, 1f)]
    public float wallGravityForce;

    public float wallRunTimer;

    public Vector3 wallRunJumpForce;
    private RaycastHit leftWallhit, rightWallhit;
    private bool wallLeft, wallRight, _wallRunInput;
    public float wallJumpDelay = 0.1f;
    public float wallJumpTimer;

    public Vector3 _wallRunVector;


    [SerializeField] private LayerMask groundLayer;
    private controllerScale normalScale = new controllerScale(new Vector3(0, -0.1f, 0), 1.81f);

    [Header("Movement Settings")]
    [SerializeField] private Transform groundPosition, ceilingPosition;

    public sound jump, land, footstep, slide;

    [Header("Slide Settings")]
    public controllerScale slideScale;
    public float _airSlideVelocity, _initialSlideForce, _slideAirFriction, _slideGroundFriction, _minSlideForce = 3;


    [Header("Particles")]
    public ParticleSystem runParticles, jumpParticles, slideParticles;

    [HideInInspector] MoveState moveStates;
    private Vector2 input;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;

    private float mouseX, mouseY;

    [HideInInspector] public bool _jumpInput, _sprintingInput, _crouchingInput, _aimingInput, _slidingInput, _slidePressed;
    public bool _grounded, _ceiling, _crouching, _sprinting, _aiming, _sliding, _wallRunning, _jumping;
    private float _verticalVelocity, _nextAllowedJumpTime, _nextAllowedLandTime, _nextAllowedSlideTime, _slopeAngle, _slideForce;
    private Vector3 _slideDirection;

    private bool previousGrounded;

    [Header("References")]
    public PlayerAnimator playerAnimator;
    public PlayerManager playerManager;
    public PlayerShoot playerShoot;
    public PlayerAnimator _animator;
    public Camera _camera;
    public CharacterController _characterController;
    public UserInputController _userInput;

    private Vector2 lastMouseInput;
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Jump(bool replaying)
    {
        _jumping = true;
        if (_sliding)
        {
            StopSlide(replaying, false);
            _slideForce = _airSlideVelocity;
        }
        if (!_wallRunning)
        {
            Debug.DrawRay(transform.position + Vector3.up * 5, Vector3.right * 10, Color.yellow, 10);
            _verticalVelocity = _jumpForce;
        }
        else
        {
            StopWallRun();
            _wallRunning = false;
            Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
            Vector3 force = transform.up * wallRunJumpForce.y + wallNormal * wallRunJumpForce.x;

            _wallRunVector.y = 0;
            _wallRunVector = force;
            _verticalVelocity = force.y;

            wallJumpTimer = wallJumpDelay;
        }
        if (!replaying)
        {
            jump.Play(transform.position);
            jumpParticles.Play();
            _nextAllowedJumpTime = Time.time + _jumpReload;
            _jumpInput = false;
            _crouching = false;
        }
    }
    public void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, _camera.transform.right, out rightWallhit, wallCheckDistance, groundLayer);
        wallLeft = Physics.Raycast(transform.position, -_camera.transform.right, out leftWallhit, wallCheckDistance, groundLayer);
    }
    public void StopSlide(bool replaying, bool removeVelocity)
    {
        SetControllerScale(normalScale);
        _sliding = false;

        slideParticles.Stop();
        if (removeVelocity)
            _slideForce = 0;
    }
    private void SetControllerScale(controllerScale scale)
    {
        _characterController.height = scale.height;
        _characterController.center = scale.center;
    }
    private void CheckJumpInput()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        if (!CanJump())
            return;

        _jumpInput = true;
    }
    private void CheckSlideInput()
    {
        if (!Input.GetKeyDown(slideKey))
            return;
        if (Mathf.Abs(input.x) == 0 && Mathf.Abs(input.y) == 0)
            return;
        if (!CanSlide())
            return;

        _slidingInput = true;
    }
    private void Update()
    {
        if (!canMove)
        {
            _characterController.enabled = false;
            return;
        }
        else
        {
            _characterController.enabled = true;
        }
        if (base.IsOwner)
        {           
            _ceiling = Physics.Raycast(ceilingPosition.position, Vector3.up, 1, groundLayer);
            _sprintingInput = Input.GetKey(KeyCode.LeftShift) && Input.GetAxisRaw("Vertical") > 0;

            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            _aimingInput = Input.GetMouseButton(1);

            CheckJumpInput();
            CheckSlideInput();
            CheckForWall();

            if(Input.GetKeyDown(slideKey) && !_crouching)
                _slidePressed = true;

            if (_grounded && _slidePressed)
            {
                _slidingInput = true;
                _slidePressed = false;
            }
            if (_crouching)
                _slidePressed = false;

            Look();

            if (Input.GetKeyDown(KeyCode.LeftControl) && input.x == 0 && input.y == 0)
                _crouchingInput = !_crouchingInput;
        }
    }
    public void Look()
    {
        mouseX += Input.GetAxis("Mouse X") * _mouseSensivity;
        mouseY -= Input.GetAxis("Mouse Y") * _mouseSensivity;

        mouseY = Mathf.Clamp(mouseY, -90, 90);

        transform.eulerAngles = new Vector3(0, mouseX, 0);

        _userInput.SetValue(FPSANames.MouseInput, new Vector4(mouseX, mouseY));
        _userInput.SetValue(FPSANames.MouseDeltaInput, new Vector4(Input.GetAxis("Mouse X") * _mouseSensivity, Input.GetAxis("Mouse Y") * _mouseSensivity));
    }
    public override void OnStartNetwork()
    {
        base.TimeManager.OnTick += TimeManager_OnTick;
    }
    public override void OnStopNetwork()
    {
        base.TimeManager.OnTick -= TimeManager_OnTick;
    }
    private void TimeManager_OnTick()
    {
        if (!canMove)
            return;

        Replicate(BuildMoveData());
        CreateReconcile();
    }
    public override void CreateReconcile()
    {
        ReconcileData rd = new ReconcileData(transform.position, _wallRunVector, _verticalVelocity, _slideForce, _slopeAngle, _slideDirection, _sprinting, _sliding, _grounded, _wallRunning, _jumping);
        ReconcileState(rd);
    }
    private ReplicateData BuildMoveData()
    {
        if (!base.IsOwner)
            return default;

        if (_grounded)
            moveStates |= (MoveState.Grounded);
        if (_sprintingInput)
            moveStates |= (MoveState.Sprinting);
        if (_jumpInput)
            moveStates |= (MoveState.Jump);
        if (_ceiling)
            moveStates |= (MoveState.Ceiling);
        if (_crouchingInput)
            moveStates |= MoveState.Crouching;
        if (_slidingInput)
            moveStates |= MoveState.Sliding;
        if (_aimingInput)
            moveStates |= MoveState.Aiming;
        if(_wallRunInput)
            moveStates |= MoveState.WallRunning;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 right = _camera.transform.right;
        Vector3 forward = transform.forward;

        ReplicateData md = new ReplicateData(new Vector2(horizontal, vertical), new Vector2(mouseX,mouseY), right, forward, moveStates);
        moveStates = MoveState.None;
        _jumpInput = false;
        _slidingInput = false;
        _wallRunInput = false;
        return md;
    }

    private bool CanJump()
    {
        if (!_wallRunning)
        {
            if (!_grounded)
                return false;
        }
        else
        {
            if (_grounded)
                return false;
        }

        if (_jumpInput)
            return false;

        float nextAllowedJumpTime = (base.IsServerInitialized && !base.IsOwner) ? _nextAllowedJumpTime - 0.15f : _nextAllowedJumpTime;
        if (Time.time < nextAllowedJumpTime)
            return false;

        return true;
    }
    private bool CanSlide()
    {
        if (!_grounded)
            return false;
        if (_slidingInput)
            return false;
        if (_jumping)
            return false;

        float nextAllowedSlideTime = (base.IsServerInitialized && !base.IsOwner) ? _nextAllowedSlideTime - 0.15f : _nextAllowedSlideTime;
        if (Time.time < nextAllowedSlideTime)
            return false;

        return true;
    }
    public void Slide(bool replaying, float deltaTime)
    {
        _slideForce -= _slideGroundFriction * deltaTime;
        _slideDirection.y = Mathf.Clamp(_slideDirection.y, -10, 10);

        if (_slideForce < _minSlideForce)
            StopSlide(replaying, true);

        if (_grounded)
        {
            float slopeAngleMultiplier = _slopeAngle * _slopeAngleMultiplier;
            _slideForce += slopeAngleMultiplier * deltaTime;
        }

        _slideForce = Mathf.Clamp(_slideForce, 0, 100);
    }
    public void WallRunMovement(float delta, Vector3 moveInput, float speed)
    {
        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((_camera.transform.forward - wallForward).magnitude > (_camera.transform.forward - -wallForward).magnitude)
            wallForward = -wallForward;
        _wallRunVector = wallForward * wallRunSpeedMultiplier * speed;

        if (!(wallLeft && moveInput.x > 0) && !(wallRight && moveInput.x < 0) && wallRunTimer < 0.5f) 
            _wallRunVector += -wallNormal * wallStickForce;
    }
    private void StopWallRun()
    {
        wallRunTimer = 0;
        _sliding = false;
        _wallRunning = false;
        _slidingInput = false;
    }
    public void WallRunning(Vector3 input, float moveRate, float delta, Vector3 moveForce, bool replay, out Vector3 modifiedMoveForce)
    {
        //Check for a wall
        CheckForWall();

        bool wall = false;
        if (wallRight || wallLeft)
            wall = true;

        modifiedMoveForce = moveForce;
        wallRunTimer -= delta;

        //If ran out of wall run time
        if (wallRunTimer <= 0 && _wallRunning)
        {
            print("Ran out of wall time");
            StopWallRun();
            _wallRunning = false;
        }

        //If ran out of wall
        if (!wall && _wallRunning)
        {
            print("Ran out of wall");
            StopWallRun();
            _wallRunning = false;
            _wallRunVector = Vector3.zero;
        }
        //if statred wall runnning
        if (wall && !_jumping && input.y > 0.9f && !_grounded)
        {
            _wallRunning = true;
            //Set verticalVelocity to zero on first frame of wallRunning
            if (wallRunTimer == 0)
            {
                _verticalVelocity = 0;
                wallRunTimer = maxWallRunTime;
            }

            WallRunMovement(delta, input, moveRate);
            //Wall run gravity
            modifiedMoveForce.y = _verticalVelocity * wallGravityForce;
            print("Sticking to wall");
        }
        //If in the air
        if (!_wallRunning && !_grounded)
        {
            _wallRunVector = Vector3.Lerp(_wallRunVector, Vector3.zero, wallJumpFriction * delta);
            StopWallRun();
        }
        if (_grounded)
            _wallRunVector = Vector3.zero;
    }
    public void StartSliding(Vector3 moveForces, bool replay)
    {
        _sliding = true;
        _slideDirection = moveForces;
        _slideForce = _initialSlideForce;
        slide.Play(transform.position, 1, transform);
        SetControllerScale(slideScale);
        slideParticles.Play();

        if (!replay)
        {
            _nextAllowedSlideTime = Time.time + _slideReload;
            _slidingInput = false;
        }
    }
    public void Sliding(bool slideFirstInput, bool replay, Vector3 moveForces, float delta)
    {
        //First Slide Frame
        if (slideFirstInput)
            StartSliding(moveForces, replay);

        //Slide Update
        if (_sliding)
            Slide(replay, delta);
        else if (_grounded && !_jumping) //End Sliding
        {
            _slideForce = 0;
            slide.Stop();
        }
        //Sliding in Air
        if (!_sliding && !_grounded)
        {
            _slideForce -= _slideAirFriction * delta;
            _slideForce = Mathf.Clamp(_slideForce, 0, 100);
            slide.Stop();
        }
    }
    void Jumping(bool jump, bool replay)
    {
        //Check if can jump as server to maintain server authority.
        if (jump)
        {
            Jump(replay);
        }
        if (_grounded && !previousGrounded && Time.time > _nextAllowedLandTime && !replay)
        {
            _nextAllowedLandTime = Time.time + .2f;
            land.Play(transform.position);
            previousGrounded = true;
        }
    }
    [Replicate]
    private void Replicate(ReplicateData md, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        if (!state.IsCreated() || state.IsFuture()) return;

        Vector3 moveForces = Vector3.zero;

        float moveRate = _sprinting ? _sprintSpeed : _walkSpeed;
        float delta = (float)base.TimeManager.TickDelta;
        if (_crouching)
            moveRate = _crouchSpeed;


        bool groundedChanged = false;
        SetIsGrounded(out groundedChanged);

        bool ceilinged = md.moveStates.Contains(MoveState.Ceiling);

        _sprinting = md.moveStates.Contains(MoveState.Sprinting);
        _crouching = md.moveStates.Contains(MoveState.Crouching);
        _aiming = md.moveStates.Contains(MoveState.Aiming);

        _verticalVelocity -= _gravityScale * delta;

        if (_grounded && Time.time > _nextAllowedJumpTime)
            _verticalVelocity = -20;

        if (ceilinged)
            _verticalVelocity = -5;

        //SPrinting
        if (_sprinting)
        {
            playerManager.weaponManager.StartSprint();
            playerShoot.canShoot = false;
        }
        else
        {
            playerManager.weaponManager.StopSprint();
            playerShoot.canShoot = true;
        }

        moveForces = md.Right * md.MoveInput.x + md.Forward * md.MoveInput.y;
        moveForces.Normalize();
        moveForces *= moveRate;

        // Player Wall Running
       // WallRunning(md.MoveInput, moveRate, delta, moveForces, state.IsReplayed(), out moveForces);

        //Player Sliding
        Sliding(md.moveStates.Contains(MoveState.Sliding), state.IsReplayed(), moveForces, delta);

        //Player jumping
        Jumping(md.moveStates.Contains(MoveState.Jump), state.IsReplayed());


        moveForces.y = _verticalVelocity;

        if (_verticalVelocity < 0)
            _jumping = false;

        //Update Rotation and Camera Input for other clients
        if (base.IsServerInitialized || !base.IsOwner) 
        {
            transform.eulerAngles = new Vector3(0, md.RotInput.x, 0);

            Vector2 mouseDelta = (md.RotInput - lastMouseInput);

            lastMouseInput = md.RotInput;

            _userInput.SetValue(FPSANames.MouseInput, new Vector4(md.RotInput.x, md.RotInput.y));
            _userInput.SetValue(FPSANames.MouseDeltaInput, new Vector4(mouseDelta.x / _mouseSensivity, mouseDelta.y / _mouseSensivity));
            //_camera.transform.localEulerAngles = new Vector3(md.RotInput.y, 0, 0);
        }

        if (_aiming)
            playerAnimator.ResetAim();

        if(_grounded && (Mathf.Abs(md.MoveInput.y) > 0||Mathf.Abs(md.MoveInput.x)>0))
            runParticles.Play();

        if(!_grounded)
            runParticles.Stop();

        if (!_sliding)
            slideParticles.Stop();

        moveForces += _slideDirection * _slideForce;
        moveForces += _wallRunVector;

        float wallRunSide = wallRight ? 1 : -1;

        _animator.UpdateAnimator(new Vector2(md.MoveInput.x, _sprinting ? 2 : md.MoveInput.y), _verticalVelocity, _grounded, _aiming, _crouching, _sliding, _sprinting, _wallRunning, wallRunSide, delta);

        if(_characterController.enabled)
            _characterController.Move(moveForces * delta);
    }
    [Reconcile]
    private void ReconcileState(ReconcileData rd, Channel channel = Channel.Unreliable)
    {
        transform.position = rd.Position;
        _slideForce = rd.SlideForce;
        _verticalVelocity = rd.VerticalVelocity;
        _grounded = rd.Grounded;
        _slopeAngle = rd.SlopeAngle;
        _sliding = rd.Sliding;
        _slideDirection = rd.SlideDirection;
        _wallRunning = rd.WallRunning;
        _sprinting = rd.Sprinting;
        _jumping = rd.Jumping;
        _wallRunVector = rd.WallRunVector;
    }
    private void SetIsGrounded(out bool changed)
    {
        //State before checking for ground.
        bool previousGrounded = _grounded;
        RaycastHit groundHit;
        _grounded = Physics.Raycast(groundPosition.position, Vector3.down, out groundHit, 1, groundLayer);
        _slopeAngle = Vector3.Dot(groundHit.normal, transform.forward);
        _slopeAngle = Mathf.Clamp(_slopeAngle, -maxSlideAngle, maxSlideAngle);
        changed = (previousGrounded != _grounded);
    }
}