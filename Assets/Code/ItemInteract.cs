using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
public class ItemInteract : NetworkBehaviour
{
    public new Transform camera;
    public float interactLength;
    public LayerMask interactLayer;

    public RaycastHit previousInteracted;
    public GunManager gunManager;
    public Gun hoveredGun;
    public PlayerManager playerManager;
    public Interactable hoveredInteractable;
    public InteractionManager interactionManager;
    public void Update()
    {
        if (!IsOwner)
            return;

        RaycastHit hit;
        Debug.DrawRay(camera.position, camera.forward * interactLength, Color.yellow);

        if (Physics.Raycast(camera.position, camera.forward, out hit, interactLength, interactLayer))
        {
            if (previousInteracted.transform == null && gunManager.equippedGun == null) 
            {

                previousInteracted = hit;
                hoveredInteractable = hit.transform.GetComponent<Interactable>();
                interactionManager.AddInteract(hoveredInteractable.itemName, hoveredInteractable.rarity, hoveredInteractable.amount, hit.transform, hoveredInteractable.trueCenter);
                hoveredGun = hoveredInteractable.GetComponent<Gun>();
            }

        }
        else
        {
            if (previousInteracted.transform != null)
            {
                interactionManager.RemoveInteract(hoveredInteractable.transform);
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
                interactionManager.RemoveInteract(hoveredInteractable.transform);
                hoveredInteractable = null;
                hoveredGun = null;
                previousInteracted = default;

            }
        }

    }
}
