using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionItem : MonoBehaviour
{
    public TextMeshProUGUI rarityText, itemNameText, amountText;
    public Transform interactObject;
    public Transform trueCenter;

    private Camera cam;
    public void SetCam(Camera camera, Transform trueCenterObject)
    {
        cam = camera;
        trueCenter = trueCenterObject;
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
