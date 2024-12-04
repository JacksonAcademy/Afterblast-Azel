using Demo.Scripts.Runtime.Item;
using FishNet.Object;
using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.FPSAnimationFramework.Runtime.Playables;
using KINEMATION.KAnimationCore.Runtime.Input;
using KINEMATION.KAnimationCore.Runtime.Rig;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class weaponManager : NetworkBehaviour
{
    public Interactable weaponPickup;
    public Item weaponEquipped;

    public PlayerManager playerManager;


    public Animator animator;
    public FPSAnimatorProfile unarmed;
    public FPSAnimator fpsanimator;
    public UserInputController _userInput;
    public static weaponManager instance;
    public KRig kRig;

    private FPSAnimatorProfile weaponProfile;
    public List<GameObject> weapons;
    public List<GameObject> weaponsToSpawn;
    public Transform weaponBone;
    private void Awake()
    {
        instance = this;
    }
    public void EquipWeapon(NetworkObject weapon)
    {
        EquipWeaponClient(weapon);
        EquipWeaponServer(weapon);
    }
    [ServerRpc(RequireOwnership = false)]
    public void EquipWeaponServer(NetworkObject weapon)
    {
        if (!base.IsOwner)
            EquipWeaponClient(weapon);
        EquipWeaponObservers(weapon);
    }
    private void Start()
    {
        fpsanimator.Initialize();

        _userInput.SetValue(FPSANames.StabilizationWeight, 0f);
        _userInput.SetValue(FPSANames.PlayablesWeight, 0f);
        foreach (var prefab in weaponsToSpawn)
        {
            var weapon = Instantiate(prefab, transform.position, Quaternion.identity);

            var weaponTransform = weapon.transform;

            weaponTransform.parent = weaponBone;
            weaponTransform.localPosition = Vector3.zero;
            weaponTransform.localRotation = Quaternion.identity;

            weapons.Add(weapon);
            weapon.gameObject.SetActive(false);
        }

    }

    [ObserversRpc(ExcludeOwner = true)]
    public void EquipWeaponObservers(NetworkObject weapon)
    {
        EquipWeaponClient(weapon);
    }
    private void EquipWeaponClient(NetworkObject weapon)
    {
        for (int i= 0; i < weapons.Count; i++)
        {
            if (weapons[i].name.Contains(weapon.gameObject.name))
            {
                print("Found weapon: " + weapons[i].name);
                weaponEquipped = weapons[i].GetComponent<Item>();
                weaponEquipped.gameObject.SetActive(true);
                break;
            }
        }


        weaponPickup = weapon.GetComponent<Interactable>();
        weaponPickup.Pickup();

        _userInput.SetValue(FPSANames.StabilizationWeight, 1f);
        _userInput.SetValue(FPSANames.PlayablesWeight, 1f);
        weaponProfile = weaponEquipped.GetComponent<FPSAnimatorEntity>().animatorProfile;
        //fpsanimator.LinkAnimatorProfile(unarmed);

        weaponEquipped.fpsItem.OnEquip(animator.gameObject);
        weapon.SetParent(NetworkObject);
        if(base.IsServerInitialized)
            weapon.GiveOwnership(LocalConnection);
        weapon.gameObject.SetActive(false);
        //fpsanimator.Initialize();
    }
    public void DropWeapon(NetworkObject weapon)
    {
        DropWeaponClient(weapon);
        DropWeaponServer(weapon);
    }
    [ServerRpc(RequireOwnership = false)]
    public void DropWeaponServer(NetworkObject weaponToDrop)
    {
        if(!base.IsOwner)
            DropWeaponClient(weaponToDrop);
        DropWeaponObservers(weaponToDrop);
    }
    [ObserversRpc(ExcludeOwner = true)]
    public void DropWeaponObservers(NetworkObject weaponToDrop)
    {
        DropWeaponClient(weaponToDrop);
    }
    public void DropWeaponClient(NetworkObject weapon)
    {
        weaponPickup.Drop();

        print("Dropping wewapon: " + weapon.gameObject.name);
        print("Dropping equipped weapon: " + weaponEquipped.gameObject.name);
        _userInput.SetValue(FPSANames.StabilizationWeight, 0f);
        _userInput.SetValue(FPSANames.PlayablesWeight, 0f);

        weapon.UnsetParent();
        weaponPickup.gameObject.SetActive(true);
        weaponPickup.NetworkObject.RemoveOwnership();

        weaponEquipped.fpsItem.OnUnEquip();
        fpsanimator.LinkAnimatorProfile(unarmed);
        weaponEquipped.gameObject.SetActive(false);

        weaponPickup = null;
        weaponEquipped = null;
        fpsanimator.Initialize();
    }
    public void StartSprint()
    {
        if (weaponEquipped)
        {
            if (!weaponEquipped.gameObject.activeInHierarchy)
                return;

            _userInput.SetValue(FPSANames.StabilizationWeight, 0f);
            _userInput.SetValue(FPSANames.PlayablesWeight, 0f);
            weaponEquipped.fpsItem.OnUnEquip();
            fpsanimator.LinkAnimatorProfile(unarmed);
            weaponEquipped.gameObject.SetActive(false);
        }
    }
    public void StopSprint()
    {
        if (weaponEquipped)
        {
            if (weaponEquipped.gameObject.activeInHierarchy)
                return;

           // if (weaponProfile)
               // fpsanimator.LinkAnimatorProfile(weaponProfile);

            _userInput.SetValue(FPSANames.StabilizationWeight, 1f);
            _userInput.SetValue(FPSANames.PlayablesWeight, 1f);

            weaponEquipped.fpsItem.OnEquip(animator.gameObject);
            weaponEquipped.gameObject.SetActive(true);
        }
    }
    public void Update()
    {
        if (!playerManager.NetworkObject.IsOwner)
            return;

        if(Input.GetKeyDown(KeyCode.Q))
        {
            if (weaponPickup)
            {
                DropWeapon(weaponPickup.NetworkObject);
            }

        }
    }
}
