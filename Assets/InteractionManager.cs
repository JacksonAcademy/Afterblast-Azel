using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager instance;
    public InteractionItem interactionItem;

    public List<InteractionItem> interactions;

    public Vector3 interactOffset;
    public Camera camera;
    private void Awake()
    {
        instance = this;
    }

    public void AddInteract(string itemName, string itemRarity, int itemAmount, Transform interactObject)
    {
        InteractionItem instantiatedItem = Instantiate(interactionItem, transform);
        instantiatedItem.SetText(itemRarity, itemName, itemAmount);
        instantiatedItem.interactObject = interactObject;
        instantiatedItem.SetCam(camera);
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
    public void LateUpdate()
    {
        for(int i =0; i < interactions.Count; ++i)
        {
            interactions[i].SetPosition(camera.WorldToScreenPoint(interactions[i].interactObject.position) + interactOffset);
        }
    }
}
