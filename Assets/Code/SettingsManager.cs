using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public CanvasGroup canvas;
    public bool esced;
    public GameObject settings;

    private void Awake()
    {
        settings.SetActive(false);
        esced = false;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            esced = !esced;

        canvas.interactable = esced;
        canvas.alpha = esced ? 1 : 0;
    }
}
