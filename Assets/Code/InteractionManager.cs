using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public InteractionItem interactionItem;
    public PlayerManager player;
    public List<InteractionItem> interactions;

    public Vector3 interactOffset;
    public new Camera camera;
    public void AddInteract(string itemName, string itemRarity, int itemAmount, Transform interactObject)
    {
        InteractionItem instantiatedItem = Instantiate(interactionItem, transform);
        instantiatedItem.SetText(itemRarity, itemName, itemAmount);
        instantiatedItem.interactObject = interactObject;
        Interactable interact = interactObject.GetComponent<Interactable>();
        interactions.Add(instantiatedItem);
    }
    public void RemoveInteract(Transform interactObject)
    {
        for(int i =0; i < interactions.Count; i++)
        {
            if (interactions[i].interactObject == interactObject)
            {
                InteractionItem interactionToRemove = interactions[i];
                interactions.Remove(interactionToRemove);
                Destroy(interactionToRemove.gameObject);
            }
        }
    }
    public void FixedUpdate()
    {
        if (!player.IsOwner)
            return;

        for(int i =0; i < interactions.Count; ++i)
        {
            interactions[i].transform.position = camera.WorldToScreenPoint(interactions[i].interactObject.transform.position) + interactOffset;
        }
    }
}
