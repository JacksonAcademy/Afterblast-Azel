using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
public class PlayerHitbox : MonoBehaviour
{ 
    public PlayerHealth playerHealth;

    private Collider col;

    private void Awake()
    {
        col = GetComponent<Collider>();
    }
    private void Start()
    {
        col.enabled = !playerHealth.Owner.IsLocalClient;
    }
}
