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
using Andtech.ProTracer;

public class PlayerShoot : NetworkBehaviour
{
    public float bulletTravelTime;
    public LayerMask shootLayer, groundLayer;
    [Header("Aiming")]
    public Vector3 normalPosition, aimingPosition, crouchingPosition, crouchingAimingPosition, slidingPosition;
    public float normalFov, aimingFov, aimLerp, slidingFOV;

    public Camera _camera;
    public Vector3 bulletSpread;
    private float _nextFire;
    public GameObject groundHitEffect, muzzleEffect, shotBlank;
    public DamageNumber damagePopup;
    [Header("UI")]
    public GameObject cantShootCrosshair;
    public PlayerAnimator playerAnimator;
    public PlayerMovement playerMovement;
    public PlayerManager playerManager;

    public Bullet bulletPrefab = default;
    public SmokeTrail smokeTrailPrefab = default;

    private DamageNumber spawnedDamageNumber;

    public lookAt gunRecoil;
    [HideInInspector] public RaycastHit hitPlayer;
    [HideInInspector] public NetworkObject hitPlayerObject;
    [HideInInspector] public Vector3 clientHitPoint, shootDirection;
    [HideInInspector] public bool canShoot;
    public sound damageSound;
    private void Awake()
    {
        aimingFov = _camera.fieldOfView;
        normalPosition = _camera.transform.localPosition;
    }
    private void Update()
    {
        if (!base.IsOwner)
            return;

        CamFOV();

        if (!playerManager.weaponManager.weaponEquipped)
            return;

        CheckShot();
    }
    private void CamFOV()
    {
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

        //_camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, fov, aimLerp * Time.deltaTime);
        // _camera.transform.localPosition = Vector3.Lerp(_camera.transform.localPosition, camPos, aimLerp * Time.deltaTime);
    }
    private void CheckShot()
    {
        RaycastHit hit;
        Vector3 shootPoint = playerManager.weaponManager.weaponEquipped.shootPoint.position;
        Quaternion shootRot = playerManager.weaponManager.weaponEquipped.shootPoint.rotation;
        if (Physics.Linecast(_camera.transform.position, shootPoint, out hit, groundLayer))
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
        if (!Input.GetMouseButton(0))
            return;
        if (playerManager.weaponManager.weaponEquipped == null)
            return;
        if (!canShoot)
            return;

        Vector3 direction = GetDirection();
        hitPlayerObject = null;
        int damage = (int)playerManager.weaponManager.weaponEquipped.damage;
        float nextFire = Time.time + playerManager.weaponManager.weaponEquipped.timeBetweenShots;

        ShootClient(shootPoint, direction, damage, nextFire);

    }
    public void ShootClient(Vector3 start, Vector3 direction, int damage, float nextFire)
    {

        _nextFire = nextFire;
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        hitPlayer = default(RaycastHit);
        hitPlayerObject = null;

        if (Physics.Raycast(ray, out hitPlayer, Mathf.Infinity))
        {
            shootDirection = Vector3.Normalize(hitPlayer.point - start);
            print("hit: " + hitPlayer.transform.name);
            if (Physics.Raycast(start, shootDirection, out hitPlayer, Mathf.Infinity, shootLayer))
            {
                hitPlayerObject = hitPlayer.transform.GetComponent<PlayerHitbox>().playerHealth.player.NetworkObject;
                CheckDamage(damage, hitPlayer);
            }

        }

        ShootEffect(start, shootDirection, NetworkObject);

        PreciseTick tick = TimeManager.GetPreciseTick(TickType.LastPacketTick);
        ServerShoot(tick, start, shootDirection, nextFire, NetworkObject);
    }
    [TargetRpc]
    public void ReconcileDamage(NetworkConnection conn)
    {
        print("Reconciling damage number");
        spawnedDamageNumber.DestroyDNP();
    }
    [ServerRpc]
    public void ServerShoot(PreciseTick pt, Vector3 shootPoint, Vector3 shootDirection, float nextFire, NetworkObject whoShot)
    {
        PlayerManager playerShot = whoShot.GetComponent<PlayerManager>();
        _nextFire = nextFire;
        RaycastHit hit = new RaycastHit();
        if (RollbackManager)
            RollbackManager.Rollback(pt, RollbackPhysicsType.Physics);
        bool reconcilePlayerDamage = true;
        //Shoot a ray to see if the client shoot path is valid, if the client shot a player

        if (Physics.Raycast(shootPoint, shootDirection, out hit, Mathf.Infinity, shootLayer))
        {
            reconcilePlayerDamage = false;
            print("Player hit: " + hit.transform.name);

            Debug.DrawRay(shootPoint, shootDirection * 100, Color.green, 10f);

            PlayerManager playerWhoGotShot = hit.transform.GetComponent<PlayerHitbox>().playerHealth.player;

            print(playerWhoGotShot.playerData.playerName + " got shot by " + playerShot.playerData.playerName);

            playerWhoGotShot.DamagePlayerObservers((int)playerShot.weaponManager.weaponEquipped.damage, playerWhoGotShot.NetworkObject);

            if (playerWhoGotShot.playerHealth.health - (int)playerShot.weaponManager.weaponEquipped.damage <= 0)
            {
                playerWhoGotShot.playerHealth.ObserversDieEffects(whoShot);
                playerShot.Kill(whoShot.Owner, playerWhoGotShot.NetworkObject);
            }

        }
        else
        {
            Debug.DrawRay(shootPoint, shootDirection * 100, Color.red, 10f);
        }
        if (reconcilePlayerDamage)
        {
            //playerShot.playerShoot.ReconcileDamage(playerShot.LocalConnection);
        }


        ObserversFire(shootPoint, shootDirection, whoShot);
        base.RollbackManager.Return();
    }
    public void CheckDamage(int damage, RaycastHit hit)
    {
        //Spawn blank effect, damage popup will spawn in DamagePlayer, sent by the server
        GameObject effect = Instantiate(shotBlank, hit.point, Quaternion.identity);
        Destroy(effect, 2);
        spawnedDamageNumber = damagePopup.Spawn(hit.transform.position + Vector3.up, damage, hit.transform);

        hit.transform.GetComponent<PlayerHitbox>().playerHealth.animator.Hit();
    }
    private void OnCompleted(object sender, System.EventArgs e)
    {
        if (sender is TracerObject tracerObject) Destroy(tracerObject.gameObject);
    }
    public void ShootEffect(Vector3 shootPoint, Vector3 direction, NetworkObject whoShot)
    {
        bool hitPlayer = false;
        PlayerManager playerWhoShot = whoShot.GetComponent<PlayerManager>();
        GameObject muzzleFlash = Instantiate(muzzleEffect, shootPoint, Quaternion.LookRotation(playerWhoShot.weaponManager.weaponEquipped.transform.position));
        Destroy(muzzleFlash, 1f);
        Item weapon = weaponManager.instance.weaponEquipped;
        weapon.sounds.useSound.Play(shootPoint);
        if (weapon.weapon)
            weapon.weapon.OnFire();

        Bullet bullet = Instantiate(bulletPrefab);
        SmokeTrail smokeTrail = Instantiate(smokeTrailPrefab);

        RaycastHit hit;

        if(Physics.Raycast(shootPoint, direction, out hit, Mathf.Infinity))
        {
            if(hit.transform.gameObject.tag == "PlayerHitbox")
                hitPlayer = true;
        }
        bullet.Arrived += OnImpact;

        void OnImpact(object sender, System.EventArgs e)
        {
            if (hitPlayer)
            {
                GameObject effect = Instantiate(shotBlank, hit.point, Quaternion.identity);
                Destroy(effect, 2);
                damageSound.Play(hit.point);
            }
            else
            {
                GameObject temp = Instantiate(groundHitEffect, hit.point, hit.normal == Vector3.zero ? Quaternion.LookRotation(hit.normal) : Quaternion.identity);
                Destroy(temp, 2);
            }
        }
        weapon.sounds.impactSound.Play(hit.point);
        bullet.Completed += OnCompleted;
        smokeTrail.Completed += OnCompleted;
        bullet.DrawLine(shootPoint, hit.point, bulletTravelTime, 0);
        smokeTrail.DrawLine(shootPoint, hit.point, bulletTravelTime, 0);
    }

    [ObserversRpc]
    public void ObserversFire(Vector3 shootPoint, Vector3 direction, NetworkObject whoShot)
    {
        if (base.IsOwner)
            return;
        RaycastHit aer;

        if(Physics.Raycast(shootPoint, direction, out aer, Mathf.Infinity, shootLayer))
        {
            print("Player hit: " + aer.transform.name);
            Debug.DrawRay(shootPoint, direction * 100, Color.green, 5);
        }
        else
        {
            Debug.DrawRay(shootPoint, direction * 100, Color.red, 5);
        }
        ShootEffect(shootPoint, direction, whoShot);
    }
    public Vector3 GetDirection()
    {
        Vector3 direction = _camera.transform.forward;

        direction += new Vector3(Random.Range(-bulletSpread.x, bulletSpread.x), Random.Range(-bulletSpread.y, bulletSpread.y), Random.Range(-bulletSpread.z, bulletSpread.z));

        direction.Normalize();
        return direction;
    }
}
