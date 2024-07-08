using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteract : MonoBehaviour
{
    public Transform camera;
    public float interactLength;
    public LayerMask interactLayer;

    public RaycastHit previousInteracted;
    public GunManager gunManager;
    public Gun hoveredGun;
    public Interactable hoveredInteractable;
    public void Update()
    {
        RaycastHit hit;
        Debug.DrawRay(camera.position, camera.forward * interactLength, Color.yellow);

        if (Physics.Raycast(camera.position, camera.forward, out hit, interactLength, interactLayer))
        {
            if (previousInteracted.transform == null)
            {

                previousInteracted = hit;
                hoveredInteractable = hit.transform.GetComponent<Interactable>();
                InteractionManager.instance.AddInteract(hoveredInteractable.itemName, hoveredInteractable.rarity, hoveredInteractable.amount, hit.transform, hoveredInteractable.trueCenter);
                hoveredGun = hoveredInteractable.GetComponent<Gun>();
            }

        }
        else
        {
            if (previousInteracted.transform != null)
            {
                InteractionManager.instance.RemoveInteract(hoveredInteractable.transform);
                hoveredInteractable = null;
                hoveredGun = null;
                previousInteracted = default;
            }

        }

        if (hoveredGun)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                gunManager.Pickup(hoveredGun);
                hoveredInteractable.Pickup();
                InteractionManager.instance.RemoveInteract(hoveredInteractable.transform);
                hoveredInteractable = null;
                hoveredGun = null;
                previousInteracted = default;

            }
        }

    }
}
