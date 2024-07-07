using FishNet.Object.Prediction;
using FishNet.Object;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using System;
using FishNet.Example.ColliderRollbacks;

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
    Aiming = 256
}
public struct ReplicateData : IReplicateData
{
    public MoveState moveStates;
    public float Horizontal;
    public float Vertical;
    public float XRotation;
    public float YRotation;
    public Vector3 Forward, Right;
    public ReplicateData(float horizontal, float vertical, float xrotation, float yrotation, Vector3 right, Vector3 forward, MoveState eoveState)
    {
        moveStates = eoveState;
        XRotation = xrotation;
        YRotation = yrotation;
        Horizontal = horizontal;
        Vertical = vertical;
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
    public float SlideForce;
    public Vector3 SlideDirection;
    public bool Sliding;
    public bool Sprinting;
    public bool Crouching;
    public float VerticalVelocity;
    public bool Grounded;
    public ReconcileData(Vector3 position, float verticalVelocity, float slideforce, Vector3 slideDirection, bool sliding, bool grounded, bool sprinting, bool crouching)
    {
        Position = position;
        SlideForce = slideforce;
        Sliding = sliding;
        Grounded = grounded;
        Sprinting = sprinting;
        Crouching = crouching;
        SlideDirection = slideDirection;
        VerticalVelocity = verticalVelocity;
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
    [SerializeField]
    private float _jumpForce = 15f;
    [SerializeField]
    private float _walkSpeed, _sprintSpeed, _crouchSpeed, _gravityScale, _mouseSensivity, _slopeAngleMultiplier, maxSlideAngle = 45;
    [SerializeField]
    private float _jumpReload = 0.5f, _slideReload = 1;
    [SerializeField]
    private Transform _cameraPivot;
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private PlayerAnimator _animator;

    [SerializeField] private LayerMask groundLayer;
    private controllerScale normalScale = new controllerScale(new Vector3(0, -0.1f, 0), 1.81f);

    [SerializeField] private Transform groundPosition, ceilingPosition;

    [Header("Sliding")]
    public controllerScale slideScale;
    public float _airSlideVelocity, _initialSlideForce, _slideAirFriction, _slideGroundFriction, _minSlideForce = 3;
    private float _slideForce;
    private Vector3 _slideDirection;

    public ParticleSystem runParticles, jumpParticles, slideParticles;

    [HideInInspector] MoveState moveStates;
    private Vector2 input;

    [Header("input")]
    public KeyCode slideKey = KeyCode.LeftControl;

    private float mouseX, mouseY;

    [SerializeField]
    private AudioListener _listener;

    private CharacterController _characterController;
    [HideInInspector] public bool _jumpInput, _sprintingInput, _crouchingInput, _aimingInput, _slidingInput, _slidePressed;
    public bool _grounded, _ceiling, _crouching, _sprinting, _aiming, _sliding;
    private float _verticalVelocity, _nextAllowedJumpTime, _nextAllowedSlideTime, slopeAngle;

    public PlayerAnimator playerAnimator;
    public PlayerManager playerManager;
    public PlayerShoot playerShoot;
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Jump(bool replaying)
    {
        if(_sliding)
        {
            StopSlide(false);
            _slideForce = _airSlideVelocity;
        }

        
        _verticalVelocity = _jumpForce;
        _crouching = false;
        if (!replaying)
        {
            jumpParticles.Play();
            _jumpInput = false;
            _nextAllowedJumpTime = Time.time + _jumpReload;

        }
    }

    public void StopSlide(bool removeVelocity)
    {
        _nextAllowedSlideTime = Time.time + _slideReload;
        SetControllerScale(normalScale);
        _sliding = false;
        _slideDirection.y = 0;
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

            if(Input.GetKeyDown(slideKey) && !_crouching && _slideForce > 0.1f)
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
        _cameraPivot.localEulerAngles = new Vector3(mouseY, 0, 0);
    }

    public override void OnStartNetwork()
    {
        base.TimeManager.OnTick += TimeManager_OnTick;
        Initialize();
    }
    public void Initialize()
    {
        _characterController = GetComponent<CharacterController>();
        _listener.enabled = Owner.IsLocalClient;
        _camera.enabled = Owner.IsLocalClient;
    }

    public override void OnStopNetwork()
    {
        base.TimeManager.OnTick -= TimeManager_OnTick;
    }


    private void TimeManager_OnTick()
    {
        if (!canMove)
            return;

        RunInputs(BuildMoveData());
        CreateReconcile();
    }
    public override void CreateReconcile()
    {
        ReconcileData rd = new ReconcileData(transform.position, _verticalVelocity, _slideForce, _slideDirection, _sliding, _grounded, _sprinting, _crouching);
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

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 right = _cameraPivot.right;
        Vector3 forward = transform.forward;
        ReplicateData md = new ReplicateData(horizontal, vertical, mouseX,mouseY, right, forward, moveStates);
        moveStates = MoveState.None;
        _jumpInput = false;
        _slidingInput = false;
        return md;
    }

    private bool CanJump()
    {
        if(!_grounded)
            return false;
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

        float nextAllowedSlideTime = (base.IsServerInitialized && !base.IsOwner) ? _nextAllowedSlideTime - 0.15f : _nextAllowedSlideTime;
        if (Time.time < nextAllowedSlideTime)
            return false;

        return true;
    }
    public void Slide(float deltaTime)
    {
        _slideForce -= _slideGroundFriction * deltaTime;
        _slideDirection.y = Mathf.Clamp(_slideDirection.y, -10, 10);

        if (_slideForce < _minSlideForce)
            StopSlide(true);
        if (_grounded)
        {
            float slopeAngleMultiplier = slopeAngle * _slopeAngleMultiplier;
            _slideForce += slopeAngleMultiplier * deltaTime;
        }

        _slideForce = Mathf.Clamp(_slideForce, 0, 100);
    }

    [Replicate]
    private void RunInputs(ReplicateData md, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        if (state.IsFuture()) return;
        Vector3 moveForces = Vector3.zero;

        float moveRate = _sprinting ? _sprintSpeed : _walkSpeed;
        float delta = (float)base.TimeManager.TickDelta;
        if (_crouching)
            moveRate = _crouchSpeed;

        bool groundedChanged = false;

        SetIsGrounded(out groundedChanged);
        bool jump = md.moveStates.Contains(MoveState.Jump);
        bool ceilinged = md.moveStates.Contains(MoveState.Ceiling);
        bool sliding = md.moveStates.Contains(MoveState.Sliding);

        _sprinting = md.moveStates.Contains(MoveState.Sprinting);
        _crouching = md.moveStates.Contains(MoveState.Crouching);
        _aiming = md.moveStates.Contains(MoveState.Aiming);

        _verticalVelocity -= _gravityScale * delta;

        if (_grounded && Time.time > _nextAllowedJumpTime)
            _verticalVelocity = -20;

        if (ceilinged)
            _verticalVelocity = -5;

        if (jump)
        {
             Jump(state.IsReplayed());
        }

        moveForces = md.Right * md.Horizontal + md.Forward * md.Vertical;
        moveForces.Normalize();
        moveForces *= moveRate;
        moveForces.y = _verticalVelocity;

        if (sliding)
        {
            _sliding = true;
            _slideDirection = moveForces;
            _slideForce = _initialSlideForce;
            SetControllerScale(slideScale);
            slideParticles.Play();
            if (!state.IsReplayed())
                _slidingInput = false;
        }
        if (_sliding)
            Slide(delta);
        else if(!sliding && _grounded && _verticalVelocity < 1)
           _slideForce = 0;
        if(!_sliding && !_grounded)
        {
            _slideForce -= _slideAirFriction * delta;
            _slideForce = Mathf.Clamp(_slideForce, 0, 100);
        }
       if(_sprinting)
        {
            playerShoot.canShoot = false;
        }
        else
        {
            playerShoot.canShoot = true;
        }
        if (base.IsServerInitialized || !base.IsOwner)
        {
            transform.eulerAngles = new Vector3(0, md.XRotation, 0);
            _cameraPivot.localEulerAngles = new Vector3(md.YRotation, 0, 0);
        }

        if (_aiming)
            playerAnimator.ResetAim();
        if(_grounded && (Mathf.Abs(md.Vertical) > 0||Mathf.Abs(md.Horizontal)>0))
            runParticles.Play();

        if(!_grounded)
            runParticles.Stop();

        if (!_sliding)
            slideParticles.Stop();
        moveForces += _slideDirection * _slideForce;

        _animator.UpdateAnimator(new Vector2(md.Horizontal, _sprinting ? 2 : md.Vertical), _verticalVelocity, _grounded, _aiming, _crouching, _sliding, _sprinting, delta);

        if(_characterController.enabled)
            _characterController.Move(moveForces * delta);
    }

    [Reconcile]
    private void ReconcileState(ReconcileData rd, Channel channel = Channel.Unreliable)
    {
        transform.position = rd.Position;
        _slideDirection = rd.SlideDirection;
        _slideForce = rd.SlideForce;
        _sliding = rd.Sliding;
        _grounded = rd.Grounded;
        _verticalVelocity = rd.VerticalVelocity;
    }


    private void SetIsGrounded(out bool changed)
    {
        //State before checking for ground.
        bool previousGrounded = _grounded;
        RaycastHit groundHit;
        _grounded = Physics.Raycast(groundPosition.position, Vector3.down, out groundHit, 1, groundLayer);
        slopeAngle = Vector3.Dot(groundHit.normal, transform.forward);
        slopeAngle = Mathf.Clamp(slopeAngle, -maxSlideAngle, maxSlideAngle);
        changed = (previousGrounded != _grounded);
    }

}