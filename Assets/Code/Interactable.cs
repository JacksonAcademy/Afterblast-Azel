using Demo.Scripts.Runtime.Item;
using FishNet.Object;
using Highlighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : NetworkBehaviour
{
    public string itemName = "Item";
    public string rarity = "Uncommon";
    public int amount = 1;

    public int interactLayer = 8;
    public int defaultLayer = 1;
    public GameObject animator;
    private Highlighter highlighter;
    private Rigidbody rb;
    private FPSItem fpsItem;

    public void Start()
    {
        TryGetComponent(out highlighter);
        TryGetComponent(out rb);
        TryGetComponent(out fpsItem);

        UnHighlight();
    }
    private void Awake()
    {
        Drop();
    }
    public void PickupItem()
    {
            weaponManager.instance.EquipWeapon(NetworkObject);
            if(rb)
                rb.isKinematic = true;
    }
    public void Drop(Vector3 force)
    {
        print("Dropped item");

        transform.eulerAngles = Vector3.zero;
        if(rb)
        {
            rb.isKinematic = false;
            rb.AddForce(force, ForceMode.Impulse);
        }

    }
    public void Highlight()
    {
        if (!highlighter)
            return;

        highlighter.enabled = true;
    }
    public void UnHighlight()
    {
        if (!highlighter)
            return;

        highlighter.enabled = false;
    }
    public void Pickup()
    {
        UnHighlight();
        gameObject.layer = 2;
    }
    public void Drop()
    {
        gameObject.layer = interactLayer;
    }
}
