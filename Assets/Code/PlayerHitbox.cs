using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
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
        if (Owner.IsLocalClient)
        {
            col.enabled = false;
        }
        else
        {
            col.enabled = true;
        }
    }
}
