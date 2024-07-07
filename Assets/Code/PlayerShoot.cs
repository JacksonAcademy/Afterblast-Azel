using DamageNumbersPro;
using FishNet.Component.ColliderRollback;
using FishNet.Managing.Timing;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using FishNet.Connection;
using System.Security.Cryptography;

public class PlayerShoot : NetworkBehaviour
{
    public float bulletTravelTime;
    public LayerMask shootLayer, groundLayer;
    public TrailRenderer shootLine;
    public Transform shootPoint;
    [Header("Aiming")]
    public Vector3 normalPosition, aimingPosition, crouchingPosition, crouchingAimingPosition, slidingPosition;
    public float normalFov, aimingFov, aimLerp, slidingFOV;

    public Camera _camera;
    public Vector3 bulletSpread;
    private float _nextFire;
    public float timeBetweenShots;
    public GameObject groundHitEffect, muzzleEffect, shotBlank;
    public DamageNumber damagePopup;
    [Header("UI")]
    public GameObject cantShootCrosshair;
    public PlayerAnimator playerAnimator;
    public PlayerMovement playerMovement;
    public PlayerManager playerManager;


    private DamageNumber spawnedDamageNumber;

    public lookAt gunRecoil;
    public RaycastHit hitPlayer;
    public NetworkObject hitPlayerObject;
    public Vector3 clientHitPoint, shootDirection;
    [HideInInspector] public bool canShoot;
    private void Awake()
    {
        aimingFov = _camera.fieldOfView;
        normalPosition = _camera.transform.localPosition;
    }
    private void Update()
    {
        if(base.IsOwner)
        {
            CheckShot();
            float fov = playerMovement._aiming ? aimingFov : normalFov;
            Vector3 camPos = Vector3.zero;
            if (!playerMovement._crouching)
                camPos = playerMovement._aiming ? aimingPosition : normalPosition;
            else
                camPos = playerMovement._aiming ? crouchingAimingPosition : crouchingPosition;
            if (playerMovement._sliding)
            {
                camPos = slidingPosition;
                fov = slidingFOV;
            }
            //camPos = normalPosition;

            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, fov, aimLerp * Time.deltaTime);
            _camera.transform.localPosition = Vector3.Lerp(_camera.transform.localPosition, camPos, aimLerp * Time.deltaTime);
        }

    }
    private void CheckShot()
    {
        GunManager gun = playerManager.gunManager;
        RaycastHit hit;
        if (Physics.Linecast(_camera.transform.position, shootPoint.position, out hit, groundLayer)) 
        {
            cantShootCrosshair.SetActive(true);
            cantShootCrosshair.transform.position = _camera.WorldToScreenPoint(hit.point);
            return;
        }
        else
        {
            cantShootCrosshair.SetActive(false);
        }

        if (Time.time < _nextFire)
            return;
        if (!Input.GetMouseButtonDown(0))
            return;
        if (gun.equippedGun == null)
            return;
        if (!canShoot)
            return;

        Vector3 direction = GetDirection();
        hitPlayerObject = null;
        int damage = (int)gun.equippedGun.damage;
        float nextFire = Time.time + gun.equippedGun.timeBetweenShots;
        ShootClient(default, shootPoint.position, direction, damage, nextFire);
        ServerShoot(base.TimeManager.GetPreciseTick(TickType.LastPacketTick), shootPoint.position, shootPoint.eulerAngles, shootDirection, damage, nextFire, clientHitPoint, hitPlayerObject, NetworkObject);
    }
    [TargetRpc]
    public void ReconcileDamage(NetworkConnection conn)
    {
        print("Reconciling damage number");
        spawnedDamageNumber.DestroyDNP();
    }
    [ServerRpc]
    public void ServerShoot(PreciseTick pt, Vector3 shootPoint, Vector3 shootPointRotation, Vector3 shootDirection, int damage, float nextFire, Vector3 clientHitPoint, NetworkObject whoGotShot, NetworkObject whoShot)
    {
        _nextFire = nextFire;
        RaycastHit hit = new RaycastHit();
       // base.RollbackManager.Rollback(pt, RollbackPhysicsType.Physics, base.IsOwner);

        //Shoot a linecast to see if the client shoot path is valid, if the client shot a player
        if(whoGotShot != null)
        {
            if (Physics.Linecast(shootPoint, whoGotShot.transform.position, out hit, shootLayer))
            {
                if (hit.transform.tag == "Player")
                {
                    PlayerManager playerShot = whoShot.GetComponent<PlayerManager>();
                    PlayerManager playerWhoGotShot = whoGotShot.GetComponent<PlayerManager>();

                    playerWhoGotShot.DamagePlayerObservers(damage, whoGotShot); 

                    ////Damage hasn't been applied yet, so subtract the damage that will be applied from the base health
                    if(playerWhoGotShot.playerHealth.health-damage <= 0)
                    {
                        playerWhoGotShot.playerHealth.ObserversDieEffects(whoShot);
                        playerShot.Kill(whoShot.Owner, whoGotShot);
                    }

                }
                else if (hit.transform.tag == "Target")
                    DamageTarget(damage, hitPlayer);
            }
        }
        else
        {
            //If the client didn't shoot a player, but anything else, do a raycast
            if (Physics.Raycast(shootPoint, shootDirection, out hit, Mathf.Infinity, shootLayer))
            {
            }
        }
        Debug.DrawRay(shootPoint, shootDirection * 2, Color.red, 10f);
        ObserversFire(shootPoint, shootPointRotation, shootDirection, clientHitPoint, hit.normal, whoGotShot!=null, whoGotShot);
        //base.RollbackManager.Return();
    }
    public void CheckDamage(int damage, RaycastHit hit)
    {
        //Spawn blank effect, damage popup will spawn in DamagePlayer, sent by the server
        GameObject effect = Instantiate(shotBlank, hit.point, Quaternion.identity);
        Destroy(effect, 2);
        spawnedDamageNumber = damagePopup.Spawn(hit.transform.position + Vector3.up, damage, hit.transform);

        hit.transform.GetComponent<PlayerHitbox>().playerHealth.animator.Hit();
    }

    public void ShootEffect(Vector3 shootPoint, Vector3 shootPointRotation, Vector3 direction, Vector3 hitPoint, Vector3 hitNormal, bool hitPlayer)
    {
        GameObject muzzleFlash = Instantiate(muzzleEffect, shootPoint, Quaternion.LookRotation(shootPointRotation));
        Destroy(muzzleFlash, 1f);

        playerAnimator.ResetAim();
        gunRecoil.Shoot();

        TrailRenderer trail = Instantiate(shootLine, shootPoint, Quaternion.identity);

        StartCoroutine(SpawnTrail(trail, direction, hitPoint, hitNormal, hitPlayer));
    }
    public void ShootClient(PreciseTick pt, Vector3 start, Vector3 direction, int damage, float nextFire)
    {
        _nextFire = nextFire;
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        hitPlayer = default(RaycastHit);
        hitPlayerObject = null;
        //Create a direction ray
        if(Physics.Raycast(ray, out hitPlayer))
            shootDirection = Vector3.Normalize(hitPlayer.point - start);
        Debug.DrawRay(start, shootDirection * 100, Color.red, 10f);
        //Shoot a real ray along the direction ray
        if (Physics.Raycast(start, shootDirection, out hitPlayer, Mathf.Infinity, shootLayer))
        {
            clientHitPoint = hitPlayer.point;
            //If the client got hit, send Check the damage
            if (hitPlayer.transform.tag == "Player")
            {
                hitPlayerObject = hitPlayer.transform.GetComponent<PlayerHitbox>().playerHealth.player.NetworkObject;
                CheckDamage(damage, hitPlayer);
            }
            else if(hitPlayer.transform.tag == "Target")
                 DamageTarget(damage, hitPlayer);
        }
        ShootEffect(start, shootPoint.localEulerAngles, shootDirection, hitPlayer.point, hitPlayer.normal, hitPlayerObject != null);
    }
    public void DamageTarget(int damage, RaycastHit hit)
    {
        GameObject effect = Instantiate(shotBlank, hit.point, Quaternion.identity);
        Destroy(effect, 2);
        if (base.IsOwner)
            damagePopup.Spawn(hit.transform.position + Vector3.up, damage);
    }
    [ObserversRpc]
    public void ObserversFire(Vector3 shootPoint, Vector3 shootPointRotation, Vector3 direction, Vector3 hitPoint, Vector3 hitNormal, bool hitPlayer, NetworkObject whoGotDamaged)
    {
        if (base.IsOwner)
            return;
        Debug.DrawLine(shootPoint, hitPoint, Color.red, 5);
        if (whoGotDamaged != null)
        {

        }
        ShootEffect(shootPoint, shootPointRotation, direction, hitPoint, hitNormal, hitPlayer);
    }
    public Vector3 GetDirection()
    {
        Vector3 direction = _camera.transform.forward;

        direction += new Vector3(Random.Range(-bulletSpread.x, bulletSpread.x), Random.Range(-bulletSpread.y, bulletSpread.y), Random.Range(-bulletSpread.z, bulletSpread.z));

        direction.Normalize();
        return direction;
    }
    public IEnumerator SpawnTrail(TrailRenderer trail, Vector3 direction, Vector3 hitPoint, Vector3 hitNormal, bool hitPlayer)
    {
        float time = 0;
        Vector3 startPos = trail.transform.position;
        Ray ray = new Ray();
        if(!hitPlayer)
        {
            ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        }
        while (time < bulletTravelTime)
        {
            if (hitPlayer)
                trail.transform.position = Vector3.Lerp(startPos, hitPoint, time);
            else
                trail.transform.position = Vector3.Lerp(startPos, ray.GetPoint(100), time);
            time += Time.deltaTime / bulletTravelTime;
            yield return null;
        }
        if(hitPlayer)
        {
            trail.transform.position = hitPoint;
            GameObject effect = Instantiate(shotBlank, hitPoint, Quaternion.identity);
            Destroy(effect, 2);
        }
        else
        {
            GameObject temp = Instantiate(groundHitEffect, hitPoint, Quaternion.LookRotation(hitNormal));
            Destroy(temp, 2);
        }
        Destroy(trail.gameObject, trail.time);
    }

}
