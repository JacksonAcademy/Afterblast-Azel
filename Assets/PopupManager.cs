using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public static PopupManager instance;
    public DamagePopup damagePopup;

    [HideInInspector] public Camera _camera;
    public Transform uiCanvas;
    private void Awake()
    {
        _camera = Camera.main;
        instance = this;
    }
    public void SpawnDamagePopup(Vector3 position, int damage)
    {
        DamagePopup popup = Instantiate(damagePopup, uiCanvas);
        popup.SetPos(_camera, position, damage.ToString());

    }
}
