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

    [HideInInspector] public RaycastHit previousInteracted;

    [HideInInspector] public Pickup hoveredPickup;
    public PlayerManager playerManager;
    [HideInInspector] public Interactable hoveredInteractable;
    public InteractionManager interactionManager;
    public void FixedUpdate()
    {
        if (!IsOwner)
            return;

        RaycastHit hit;

        if (Physics.Raycast(camera.position, camera.forward, out hit, interactLength, interactLayer))
        {
            if (previousInteracted.transform == null) 
            {
                previousInteracted = hit;
                hoveredInteractable = hit.transform.GetComponent<Interactable>();
                interactionManager.AddInteract(hoveredInteractable.itemName, hoveredInteractable.rarity, hoveredInteractable.amount, hit.transform);
                hoveredPickup = hoveredInteractable.GetComponent<Pickup>();
            }

        }
        else
        {
            if (previousInteracted.transform != null)
            {
                interactionManager.RemoveInteract(hoveredInteractable.transform);
                hoveredInteractable = null;
                hoveredPickup = null;
                previousInteracted = default;
            }

        }
    }
    private void Update()
    {
        if (hoveredPickup)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                hoveredPickup.PickupItem(playerManager.NetworkObject);

                interactionManager.RemoveInteract(hoveredInteractable.transform);
                hoveredInteractable = null;
                hoveredPickup = null;
                previousInteracted = default;

            }
        }
    }
}
