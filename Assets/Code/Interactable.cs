using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string itemName = "Item";
    public string rarity = "Uncommon";
    public int amount = 1;

    public int interactLayer = 8;
    public int defaultLayer = 1;
    private void Awake()
    {
        Drop();   
    }
    public void Pickup()
    {
        gameObject.layer = 2;
    }
    public void Drop()
    {
        gameObject.layer = interactLayer;
    }
}
