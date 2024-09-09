using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public CanvasGroup canvas;
    private void Update()
    {
            canvas.interactable = Cursor.visible;
        canvas.alpha = Cursor.visible ? 1 : 0;

    }
}
