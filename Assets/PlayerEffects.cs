using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffects : NetworkBehaviour
{
    [Header("Particle Effects")]
    public ParticleSystem sprintParticle;
    public PlayerMovement playerMovement;
    public List<TrailRenderer> jumpTrail;
    public float jumpDelay;

    public static PlayerEffects instance;
    private void Awake()
    {
        instance = this;
        sprintParticle.Stop();
    }
    private void Update()
    {
        if(base.IsOwner)
        {
            sprintParticle.Stop();

            if (playerMovement._sprinting)
                sprintParticle.Play();
            if (playerMovement._sliding)
                sprintParticle.Play();
        }
    }
    public void Jump()
    {
        StopCoroutine("JumpIE");
        StartCoroutine("JumpIE");
    }
    public IEnumerator JumpIE()
    {
        for(int i = 0; i < jumpTrail.Count; i++)
        {
            jumpTrail[i].emitting = true;
        }
        yield return new WaitForSeconds(jumpDelay);
        for (int i = 0; i < jumpTrail.Count; i++)
        {
            jumpTrail[i].emitting = false;
        }
    }
}
