
using FishNet.Object;
using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Input;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimator : NetworkBehaviour
{
    [HideInInspector] public float gravity;
    [HideInInspector] public bool grounded;
    public float animationSmoothing, rigSmoothing;
    private PlayerShoot playerShoot;
    private PlayerMovement playerMovement;

    public Animator _animator;
    public PlayerManager playerManager;

    public UserInputController _userInput;
    public FPSAnimator fpsAnimator;

    public Vector2 animationVelocity;
    public Vector2 tickAnimationVelocity;

    public bool aiming;
    private void Awake()
    {
        playerShoot = GetComponent<PlayerShoot>();
        playerMovement = GetComponent<PlayerMovement>();
    }
    private void Start()
    {

    }
    public void Hit()
    {
        _animator.SetTrigger("Hit");
    }
    private void Update()
    {
        animationVelocity = Vector2.Lerp(animationVelocity, tickAnimationVelocity, animationSmoothing * Time.deltaTime);
        _animator.SetFloat("XVelocity", animationVelocity.x);
        _animator.SetFloat("YVelocity", animationVelocity.y);
        float velocity = animationVelocity.SqrMagnitude();
        _animator.SetFloat("Velocity", velocity);
            _userInput.SetValue(FPSANames.IsAiming, aiming);
    }
    public void UpdateAnimator(Vector2 velocity, float gravity, bool grounded, bool aim, bool crouching, bool sliding, bool sprinting, bool wallRunning, float wallRunSide, float delta)
    {
        tickAnimationVelocity = velocity;
        _animator.SetBool("Grounded", grounded);
        _animator.SetFloat("Gravity", gravity);
        _animator.SetBool("Aim", aim);
        _animator.SetBool("Crouch", crouching);
        _animator.SetBool("Sliding", sliding);
        _animator.SetBool("WallRun", wallRunning);
        _animator.SetFloat("WallRunSide", wallRunSide);
    }
}
