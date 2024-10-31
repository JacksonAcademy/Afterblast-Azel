
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
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
    private bool previousGrounded;
    public float aimTimer;
    public float aimDelay;

    private Vector2 animationVelocity;
    private Vector2 tickAnimationVelocity;
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
    }
    public void ResetAim()
    {
        aimTimer = aimDelay;
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
        if (grounded && !previousGrounded)
        {
            previousGrounded = true;
           // _animator.SetTrigger("HitGround");
        }
        if (!grounded)
            previousGrounded = false;

        if (!sprinting)
        {
            aimTimer -= delta;
        }
        else
        {
            aimTimer = 0;
        }

        aimTimer = Mathf.Clamp(aimTimer, 0, 1000);

    }
}
