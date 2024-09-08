using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum gunType
{
    rifle,
    pistol
}
public class Gun : NetworkBehaviour
{
    public string gunName;
    public GameObject model;

    public Transform rightHandPoint, leftHandPoint, shootPoint;

    [Header("Gun Properties")]
    public GameObject muzzleFlash;
    public gunType holdType;
    public float gunHoldSize, gunDropSize;
    public Vector3 gunHoldRotation, gunDropRotation;
    public GameObject gunIdleEffect;

    public GameObject graphicalObject;

    public float reloadTime;

    public float damage;

    public float timeBetweenShots;

    public int maxMagazineSize;

    public int currentAmmo;

    private Rigidbody rb;

    public sound shootSound;
    public sound bulletWhiz;
    private void Awake()
    {
        graphicalObject = NetworkObject.GetGraphicalObject().gameObject;
        rb = GetComponent<Rigidbody>();
    }
    public new void Reset()
    {
        currentAmmo = maxMagazineSize;
        Drop(Vector3.zero, transform.position);
    }
    public void Shoot(Vector3 position)
    {
        shootSound.Play(position);
        currentAmmo--;
    }
    public void Pickup(NetworkObject player)
    {
        PickupA(player);
        ServerPickup(player);
    }
    [ServerRpc(RequireOwnership =false)]
    public void ServerPickup(NetworkObject player)
    {
        print("Server pickup");
        base.GiveOwnership(player.Owner);
        PickupA(player);
        ObserverPickup(player);
    }
    [ObserversRpc(BufferLast = true)]
    public void ObserverPickup(NetworkObject player)
    {
        PickupA(player);
    }
    public void PickupA(NetworkObject player)
    {
        PlayerManager whoPickedUp = player.GetComponent<PlayerManager>();
        whoPickedUp.gunManager.equippedGun = this;
        LocalPickup(whoPickedUp.gunManager.gunHolder.transform);
        whoPickedUp.gunManager.SetHandIK(this);
    }
    public void LocalPickup(Transform gunHolder)
    {
        NetworkObject.SetParent(gunHolder.GetComponent<EmptyParentBehaviour>());

        NetworkObject.SetGraphicalObject(null);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one * gunHoldSize;
        transform.localRotation = Quaternion.Euler(gunHoldRotation);
        gunIdleEffect.SetActive(false);
        rb.isKinematic = true;
    }
    public void Drop(Vector3 force, Vector3 dropPos)
    {
        LocalDrop(force, dropPos);
    }
    public void LocalDrop(Vector3 force, Vector3 dropPos)
    {
        if(IsServerInitialized)
            NetworkObject.RemoveOwnership();

        print("Drop gun: " + gameObject.name);
        NetworkObject.UnsetParent();
        NetworkObject.SetGraphicalObject(graphicalObject.transform);
        transform.position = dropPos;
        rb.isKinematic = false;
        rb.AddForce(force, ForceMode.Impulse);
        gunIdleEffect.SetActive(true);

        transform.localScale = Vector3.one * gunDropSize;
        transform.localRotation = Quaternion.Euler(gunDropRotation);
    }

}
