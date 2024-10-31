using Demo.Scripts.Runtime.Item;
using FishNet.Object;
using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.FPSAnimationFramework.Runtime.Playables;
using KINEMATION.KAnimationCore.Runtime.Input;
using KINEMATION.KAnimationCore.Runtime.Rig;
using UnityEngine;

public class weaponManager : NetworkBehaviour
{
    public Pickup equippedItem;
    public PlayerManager playerManager;
    public EmptyParentBehaviour weaponBone;


    public Animator animator;
    public FPSAnimatorProfile unarmed;
    public FPSAnimator fpsanimator;
    public UserInputController _userInput;
    public static weaponManager instance;
    public KRig kRig;

    private FPSAnimatorProfile weaponProfile;
    private void Awake()
    {
        instance = this;
    }
    public void EquipWeapon(FPSItem weaponToEquip)
    {
        EquipWeaponClient(weaponToEquip.GetComponent<NetworkObject>());
        EquipWeaponServer(weaponToEquip.GetComponent<NetworkObject>());
    }
    [ServerRpc(RequireOwnership = false)]
    public void EquipWeaponServer(NetworkObject weapon)
    {
        if(!base.IsOwner)
            EquipWeaponClient(weapon);
        EquipWeaponObservers(weapon);
    }
    [ObserversRpc(BufferLast = true, ExcludeOwner = true)]
    public void EquipWeaponObservers(NetworkObject weapon)
    {
        EquipWeaponClient(weapon);
    }
    private void EquipWeaponClient(NetworkObject weaponToEquip)
    {
        print("Picked up weapon");
        FPSItem weapon = weaponToEquip.GetComponent<FPSItem>();
        Pickup pickupWeapon = weapon.GetComponent<Pickup>();

        equippedItem = pickupWeapon;
        pickupWeapon.interactable.Pickup();

        _userInput.SetValue(FPSANames.StabilizationWeight, 1f);
        _userInput.SetValue(FPSANames.PlayablesWeight, 1f);

        weaponToEquip.GetComponent<NetworkObject>().SetParent(weaponBone);
        weaponToEquip.transform.localPosition = Vector3.zero;
        weaponToEquip.transform.localRotation = Quaternion.identity;
        weaponProfile = weaponToEquip.GetComponent<FPSAnimatorEntity>().animatorProfile;

        //weapon.OnEquip(animator.gameObject);
    }
    public void DropWeapon(FPSItem weaponToDrop)
    {
        DropWeaponClient(weaponToDrop.GetComponent<NetworkObject>());
        DropWeaponServer(weaponToDrop.GetComponent<NetworkObject>());
    }
    [ServerRpc(RequireOwnership = false)]
    public void DropWeaponServer(NetworkObject weaponToDrop)
    {
        DropWeaponObservers(weaponToDrop);
    }
    [ObserversRpc(BufferLast = true, ExcludeOwner = true)]
    public void DropWeaponObservers(NetworkObject weaponToDrop)
    {
        DropWeaponClient(weaponToDrop);
    }
    public void DropWeaponClient(NetworkObject weaponToDrop)
    {
        FPSItem weapon = weaponToDrop.GetComponent<FPSItem>();

        weapon.OnUnEquip();

        equippedItem.Drop(playerManager.mainCam.transform.forward * 10);
        equippedItem = null;

        fpsanimator.LinkAnimatorProfile(unarmed);

        _userInput.SetValue(FPSANames.StabilizationWeight, 0f);
        _userInput.SetValue(FPSANames.PlayablesWeight, 0f);

        weaponProfile = null;
    }
    public void StartSprint()
    {
        if (equippedItem)
        {
            if (!equippedItem.gameObject.activeInHierarchy)
                return;
            _userInput.SetValue(FPSANames.StabilizationWeight, 0f);
            _userInput.SetValue(FPSANames.PlayablesWeight, 0f);
            fpsanimator.LinkAnimatorProfile(unarmed);
            equippedItem.fpsItem.OnUnEquip();
            equippedItem.gameObject.SetActive(false);
        }
    }
    public void StopSprint()
    {
        if (equippedItem)
        {
            if (equippedItem.gameObject.activeInHierarchy)
                return;
            if(weaponProfile)
                fpsanimator.LinkAnimatorProfile(weaponProfile);
            _userInput.SetValue(FPSANames.StabilizationWeight, 1f);
            _userInput.SetValue(FPSANames.PlayablesWeight, 1f);
            equippedItem.fpsItem.OnEquip(animator.gameObject);
            equippedItem.gameObject.SetActive(true);

        }
    }
    public void Update()
    {
        if (!playerManager.NetworkObject.IsOwner)
            return;

        if(Input.GetKeyDown(KeyCode.Q))
        {
            if (equippedItem)
            {
                DropWeapon(equippedItem.fpsItem);
            }

        }
    }
}
