using FishNet.Managing;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class WeaponSpawner : NetworkBehaviour
{
    public List<NetworkObject> weaponsToSpawn = new List<NetworkObject>();

    public Transform spawnPoint;

    public override void OnStartServer()
    {
        base.OnStartServer();
            SpawnWeapon();
    }
    public void SpawnWeapon()
    {
        NetworkObject randomObject = weaponsToSpawn[Random.Range(0, weaponsToSpawn.Count)];
        NetworkObject gun = Instantiate(randomObject);
        gun.SetParent(this);
        gun.transform.localPosition = Vector3.zero;
        gun.transform.localScale = Vector3.one;
        Spawn(gun);

    }
}
