using FishNet.Component.ColliderRollback;
using FishNet.Managing.Timing;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerShoot : NetworkBehaviour
{
    public float bulletTravelTime;
    public LayerMask shootLayer, groundLayer;
    public TrailRenderer shootLine;
    public Transform shootPoint;
    public int damage;
    [Header("Aiming")]
    public Vector3 normalPosition, aimingPosition, crouchingPosition, crouchingAimingPosition, slidingPosition;
    public float normalFov, aimingFov, aimLerp, slidingFOV;

    public Camera _camera;
    public Vector3 bulletSpread;
    private float _nextFire;
    public float timeBetweenShots;
    public GameObject hitEffect, muzzleEffect, playerHitEffect;

    [Header("UI")]
    public GameObject cantShootCrosshair;
    public PlayerAnimator playerAnimator;
    public PlayerMovement playerMovement;
    public PlayerManager playerManager;

    public lookAt gunRecoil;
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

        Vector3 direction = GetDirection();



        Shoot(default, shootPoint.position, direction);
        CmdShoot(base.TimeManager.GetPreciseTick(base.TimeManager.LastPacketTick.Value()), shootPoint.position, direction);
    }
    [ServerRpc]
    public void CmdShoot(PreciseTick pt, Vector3 start, Vector3 direction)
    {
        if(!base.IsOwner)
        {
            Shoot(pt, start, direction);
        }
        ObserversFire(start, direction);

    }
    public void Shoot(PreciseTick pt, Vector3 start, Vector3 direction)
    {
        _nextFire = Time.time + timeBetweenShots;

        GameObject muzzleFlash = Instantiate(muzzleEffect, shootPoint.position, shootPoint.rotation);
        Destroy(muzzleFlash, 1f);
        playerAnimator.ResetAim();

        gunRecoil.Shoot();
        
        base.RollbackManager.Rollback(pt, RollbackPhysicsType.Physics, base.IsOwner);
        RaycastHit hit;
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        //Create a direction ray
        if(Physics.Raycast(ray, out hit))
        {
            direction = Vector3.Normalize(hit.point - start);
            Debug.DrawRay(start, direction * 1000, Color.red, 100);
        }
        //Shoot a real ray along the direction ray
        if (Physics.Raycast(start, direction, out hit, Mathf.Infinity, shootLayer))
        {
            if (base.IsClientInitialized)
            {
                TrailRenderer trail = Instantiate(shootLine, shootPoint.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, hit));

                if (hit.transform.tag == "Player")
                {
                    PlayerHitbox hitbox = hit.transform.GetComponent<PlayerHitbox>();
                    hitbox.playerHealth.animator.Hit();
                    GameObject effect = Instantiate(playerHitEffect, hit.point, Quaternion.identity);
                    Destroy(effect, 2);
                    if(base.IsOwner)
                    {
                        PopupManager.instance.SpawnDamagePopup(hitbox.transform.position + (Vector3.up * 2), damage);
                        // If the hit players health minus the dealt damage is less than zero
                        if(hitbox.GetHealth(playerManager) <= damage)
                        {
                            //Client eliminated the hit player
                            playerManager.Kill(hitbox.playerHealth.player);
                        }
                    }



                    //If the server detects the shot was a hit
                    if (base.IsServerInitialized)
                        hitbox.Damage(damage, playerManager);
                }
                else if(hit.transform.tag == "Target")
                {
                    GameObject effect = Instantiate(playerHitEffect, hit.point + (Vector3.up * 2), Quaternion.identity);
                    Destroy(effect, 2);
                    if (base.IsOwner)
                        PopupManager.instance.SpawnDamagePopup(hit.point, damage);
                }

            }
        }
        base.RollbackManager.Return();

    }
    [ObserversRpc]
    public void ObserversFire(Vector3 pos, Vector3 forward)
    {
        if (base.IsOwner || base.IsServerInitialized)
            return;

        RaycastHit hit;
        Physics.Raycast(pos, forward, out hit, Mathf.Infinity, shootLayer);

        Shoot(default, pos, forward);
    }
    public Vector3 GetDirection()
    {
        Vector3 direction = _camera.transform.forward;

        direction += new Vector3(Random.Range(-bulletSpread.x, bulletSpread.x), Random.Range(-bulletSpread.y, bulletSpread.y), Random.Range(-bulletSpread.z, bulletSpread.z));

        direction.Normalize();
        return direction;
    }
    public IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit hit)
    {
        float time = 0;
        Vector3 startPos = trail.transform.position;
        while (time < bulletTravelTime)
        {
            trail.transform.position = Vector3.Lerp(startPos, hit.point, time);
            time += Time.deltaTime / bulletTravelTime;
            yield return null;
        }
        if(hit.transform.tag != "Player")
        {
            trail.transform.position = hit.point;
            GameObject temp = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(temp, 2);
        }


        Destroy(trail.gameObject, trail.time);
    }

}
