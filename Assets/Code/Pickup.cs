using Demo.Scripts.Runtime.Item;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(FPSItem))]
public class Pickup : NetworkBehaviour
{
    [HideInInspector] public FPSItem fpsItem;

    public float damage, timeBetweenShots;
    public Transform shootPoint;

    [Header("Item Properties")]
    public GameObject muzzleFlash;

    private Rigidbody rb;

    [HideInInspector] public Interactable interactable;
    [HideInInspector] public Weapon weapon;

    
    private void Awake()
    {
        TryGetComponent(out weapon);
        TryGetComponent(out interactable);

        rb = GetComponent<Rigidbody>();
        fpsItem = GetComponent<FPSItem>();
    }
    public void PickupItem(NetworkObject playerObject)
    {
        weaponManager.instance.EquipWeapon(fpsItem);
    }
    public void Drop(Vector3 force)
    {
        print("Dropped item");

        transform.eulerAngles = Vector3.zero;
        NetworkObject.UnsetParent();
        rb.isKinematic = false;
        rb.AddForce(force, ForceMode.Impulse);
        if (interactable)
            interactable.Drop();
    }
}
