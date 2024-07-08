using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum gunType
{
    rifle,
    pistol
}

public class Gun : MonoBehaviour
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

    public float reloadTime;

    public float damage;

    public float timeBetweenShots;

    public int maxMagazineSize;

    public int currentAmmo;

    public void Reset()
    {
        currentAmmo = maxMagazineSize;
        Drop();
    }
    public void Shoot()
    {
        currentAmmo--;
    }
    public void Pickup(Transform holder)
    {
        transform.SetParent(holder, false);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one * gunHoldSize;
        transform.localRotation = Quaternion.Euler(gunHoldRotation);
        gunIdleEffect.SetActive(false);
    }
    public void Drop()
    {
        gunIdleEffect.SetActive(true);
        transform.SetParent(null);
        transform.localScale = Vector3.one * gunDropSize;
        transform.localRotation = Quaternion.Euler(gunDropRotation);
    }
}
