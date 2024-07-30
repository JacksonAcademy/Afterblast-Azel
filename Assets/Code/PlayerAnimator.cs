
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimator : NetworkBehaviour
{
    public float gravity;
    public bool grounded;
    public float animationSmoothing, rigSmoothing;
    private PlayerShoot playerShoot;
    private PlayerMovement playerMovement;

    [SerializeField]
    private Animator _animator;
    public GameObject gun;
    public PlayerManager playerManager;
    private bool previousGrounded;
    public float aimTimer;
    public float aimDelay;

    public GunManager gunManager;

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

    }
    public void ResetAim()
    {
        aimTimer = aimDelay;
    }
    public void UpdateAnimator(Vector2 velocity, float gravity, bool grounded, bool aim, bool crouching, bool sliding, bool sprinting, float delta, int gunType)
    {
        tickAnimationVelocity = velocity;
        _animator.SetBool("Grounded", grounded);
        _animator.SetFloat("Gravity", gravity);
        _animator.SetBool("Aim", aim);
        _animator.SetBool("Crouch", crouching);
        _animator.SetBool("Sliding", sliding);
        _animator.SetBool("Gun", gunType == 0);

        gunManager.sprinting = sprinting;

        if (grounded && !previousGrounded)
        {
            previousGrounded = true;
           // _animator.SetTrigger("HitGround");
        }
        if (!grounded)
            previousGrounded = false;

        if (!sprinting)
        {
            _animator.SetInteger("GunType", gunType);
            aimTimer -= delta;
            if (aimTimer > 0)
                GunAim();
            else
                GunIdle();
        }
        else
        {
            gunManager.NoGun();
            aimTimer = 0;
            GunIdle();
        }



    }
    public void GunAim()
    {
        gunManager.aimWeight = 1;
        _animator.SetBool("Aim", true);
    }
    public void GunIdle()
    {
        _animator.SetBool("Aim", false);
        gunManager.aimWeight = 0;
    }
}
