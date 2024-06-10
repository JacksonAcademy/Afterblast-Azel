using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    public TextMeshProUGUI text;
    public int index;
    [HideInInspector] public PopupManager popup;
    private Camera cam;
    private Vector3 worldPos;
    private void Awake()
    {
        StartCoroutine(DESTROY());
    }
    public void SetPos(Camera camera, Vector3 worldPosition, string a)
    {
        cam = camera;
        worldPos = worldPosition;
        text.text = a.ToString();
    }
    private void LateUpdate()
    {
        Vector3 pos = cam.WorldToScreenPoint(worldPos);
        transform.position = pos;
    }
    public IEnumerator DESTROY()
    {

        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
