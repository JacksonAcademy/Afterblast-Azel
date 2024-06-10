using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitbox : NetworkBehaviour
{ 
    public PlayerHealth playerHealth;

    private Collider col;

    private void Awake()
    {
        col = GetComponent<Collider>();
    }
    private void Start()
    {
        //if(Owner.IsLocalClient)
        //{
        //    col.enabled = false;
        //}
        //else
        //{
        //    col.enabled = true;
        //}
    }
    public void Damage(int amount, PlayerManager playerDamaged)
    {
        playerHealth.TakeDamage(amount, playerDamaged);
    }
    public float GetHealth(PlayerManager whoRequested, float damage)
    {
        if(whoRequested)
        {
            return playerHealth.health + damage;
        }
        else
        {
            return 0;
        }
    }
}