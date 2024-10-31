using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionItem : MonoBehaviour
{
    public TextMeshProUGUI rarityText, itemNameText, amountText;
    public Transform interactObject;
    public void SetText(string rarity, string itemname, int amount)
    {
        rarityText.text = rarity;
        itemNameText.text = itemname;
        amountText.text = amount.ToString();
    }
}
