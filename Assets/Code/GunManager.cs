using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class GunManager : MonoBehaviour
{
    public List<Rig> assaultRifleRigs;
    public List<Rig> pistolRigs;

    public Transform gunHolder;
    public static GunManager instance;

    public Gun equippedGun;
    private Interactable equippedGunInteract;

    public float aimWeight;
    public bool sprinting;
    private void Awake()
    {
        instance = this;
        equippedGun = null;
        NoGun();
    }
    public void NoGun()
    {
        for (int i = 0; i < assaultRifleRigs.Count; i++)
            assaultRifleRigs[i].weight = 0;
        for (int i = 0; i < pistolRigs.Count; i++)
            pistolRigs[i].weight = 0;
    }
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if(equippedGun != null)
            {
                Drop(equippedGun);
            }
        }
        if(!equippedGun)
        {
            for (int i = 0; i < assaultRifleRigs.Count; i++)
                assaultRifleRigs[i].weight = 0;
            for (int i = 0; i < pistolRigs.Count; i++)
                pistolRigs[i].weight = 0;
        }
        else if(!sprinting)
        {

            if (equippedGun.holdType == gunType.rifle)
            {
                assaultRifleRigs[1].weight = 1;
                assaultRifleRigs[0].weight = aimWeight;
            }


            if (equippedGun.holdType == gunType.pistol)
            {
                pistolRigs[0].weight = aimWeight;
                assaultRifleRigs[1].weight = 1;
            }
        }
    }
    public void Drop(Gun gun)
    {
        for (int i = 0; i < assaultRifleRigs.Count; i++)
            assaultRifleRigs[i].weight = 0;
        for (int i = 0; i < pistolRigs.Count; i++)
            pistolRigs[i].weight = 0;
        gun.Drop();
        equippedGun.Drop();
        equippedGunInteract.Drop();
        equippedGunInteract = null;
        equippedGun = null;
    }
    public void Pickup(Gun gun)
    {
        for (int i = 0; i < assaultRifleRigs.Count; i++)
            assaultRifleRigs[i].weight = 0;
        for (int i = 0; i < pistolRigs.Count; i++)
            pistolRigs[i].weight = 0;
        gun.Pickup(gunHolder);
        equippedGun = gun;
        equippedGunInteract = gun.GetComponent<Interactable>();
        if (gun.holdType == gunType.pistol)
        {
                pistolRigs[1].weight = 1;
        }
        else if(gun.holdType == gunType.rifle)
        {
                assaultRifleRigs[1].weight = 1;
        }

    }
}
