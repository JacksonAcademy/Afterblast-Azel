using Demo.Scripts.Runtime.Item;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(FPSItem))]
public class Item : NetworkBehaviour
{
    [HideInInspector] public FPSItem fpsItem;
    public weaponSounds sounds;
    public float damage, timeBetweenShots;
    public Transform shootPoint;

    [Header("Item Properties")]
    public GameObject muzzleFlash;

    [HideInInspector] public Interactable interactable;
    [HideInInspector] public Weapon weapon;

    
    private void Awake()
    {
        TryGetComponent(out weapon);
        TryGetComponent(out interactable);

        fpsItem = GetComponent<FPSItem>();
    }

}
