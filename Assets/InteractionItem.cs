using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionItem : MonoBehaviour
{
    public TextMeshProUGUI rarityText, itemNameText, amountText;
    public Transform interactObject;

    private Camera cam;
    public void SetCam(Camera camera)
    {
        cam = camera;
    }
    public void SetText(string rarity, string itemname, int amount)
    {
        rarityText.text = rarity;
        itemNameText.text = itemname;
        amountText.text = amount.ToString();
    }
    public void SetPosition(Vector3 position)
    {
        transform.position = position; 
    }
}
