using FishNet.Example.ColliderRollbacks;
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
    public Rig aimRig, idleRig;
    public GameObject gun;
    private bool previousGrounded;
    private float aimTimer;
    public float aimDelay;
    float aimRigWeight;

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
    public void UpdateAnimator(Vector2 velocity, float gravity, bool grounded, bool aim, bool crouching, bool sliding, bool sprinting, float delta)
    {
        tickAnimationVelocity = velocity;
        _animator.SetBool("Grounded", grounded);
        _animator.SetFloat("Gravity", gravity);
        _animator.SetBool("Aim", aim);
        _animator.SetBool("Crouch", crouching);
        _animator.SetBool("Sliding", sliding);

        aimRigWeight = aim ? 1f : aimRig.weight;
        if(grounded && !previousGrounded)
        {
            previousGrounded = true;
           // _animator.SetTrigger("HitGround");
        }
        if (!grounded)
            previousGrounded = false;
        if (!sprinting)
        {
            if (aimTimer > 0)
                GunAim();
            else
                GunIdle();
            idleRig.weight = 1;
            gun.SetActive(true);
        }
        aimTimer -= delta;

        aimRig.weight = Mathf.Lerp(aimRig.weight, aimRigWeight, rigSmoothing * delta);
        if (sprinting)
        {
            gun.SetActive(false);
            idleRig.weight = 0;
            aimTimer = 0;
            aimRig.weight = 0;
            GunIdle();
        }

    }
    public void GunAim()
    {
        aimRigWeight = 1;
        _animator.SetBool("Aim", true);
    }
    public void GunIdle()
    {
        _animator.SetBool("Aim", false);
        aimRigWeight = 0;
    }
}
