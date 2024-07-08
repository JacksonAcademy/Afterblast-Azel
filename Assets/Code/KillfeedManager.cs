using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KillfeedManager : MonoBehaviour
{
    public static KillfeedManager instance;


    public GameObject killfeedItem;
    public float killfeedStayDuration;

    private void Awake()
    {
        instance = this;
    }

    public void AddItem(string text)
    {
        GameObject killfeedItemSpawn = Instantiate(killfeedItem, transform);
        Destroy(killfeedItemSpawn, killfeedStayDuration);
        TextMeshProUGUI killfeedText = killfeedItemSpawn.GetComponentInChildren<TextMeshProUGUI>();
        killfeedText.text = text;
    }
}
