using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class GunManager : NetworkBehaviour
{
    public List<Rig> assaultRifleRigs;
    public List<Rig> pistolRigs;

    public List<HandIKSetter> handIK;
    public Transform gunHolder;
    public static GunManager instance;

    public Gun equippedGun;
    private Interactable equippedGunInteract;

    public float aimWeight;
    public bool sprinting;
    public PlayerManager playerManager;

    public int equippedGunType;
    public float dropForce;
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

        for (int i = 0; i < handIK.Count; i++)
            handIK[i].SetIK(null);
    }
    public void Update()
    {
        if (equippedGunType == 0)
        {
            for (int i = 0; i < assaultRifleRigs.Count; i++)
                assaultRifleRigs[i].weight = 0;
            for (int i = 0; i < pistolRigs.Count; i++)
                pistolRigs[i].weight = 0;
        }
        else if (!sprinting)
        {

            if (equippedGunType == 1)
            {
                assaultRifleRigs[1].weight = 1;
                assaultRifleRigs[0].weight = aimWeight;

                for (int i = 0; i < pistolRigs.Count; i++)
                    pistolRigs[i].weight = 0;
            }
            if (equippedGunType == 2)
            {
                pistolRigs[0].weight = aimWeight;
                pistolRigs[1].weight = 1;

                for (int i = 0; i < assaultRifleRigs.Count; i++)
                    assaultRifleRigs[i].weight = 0;
            }
        }
        gunHolder.gameObject.SetActive(!sprinting);

        if (!base.IsOwner)
            return;

        if(Input.GetKeyDown(KeyCode.Q))
        {
            if(equippedGun != null)
            {
                Drop(equippedGun, playerManager.mainCam.transform.forward * dropForce);
            }
        }
    }
    public void Drop(Gun gun, Vector3 force)
    {
        NoGun();
        gun.Drop(force, transform.position);
        equippedGunInteract.Drop();
        equippedGunInteract = null;
        equippedGun = null;
    }
    public void SetHandIK(Gun gun)
    {
        for (int i = 0; i < handIK.Count; i++)
            handIK[i].SetIK(gun);
    }
    public void Pickup(Gun gun)
    {
        gun.Pickup(playerManager.playerObject);
        equippedGun = gun;
        equippedGunInteract = gun.GetComponent<Interactable>();
    }
}
