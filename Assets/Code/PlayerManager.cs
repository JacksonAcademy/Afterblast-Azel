using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine;
using FishNet.Connection;
using Unity.VisualScripting;
using FishNet.Serializing;
using DamageNumbersPro;
using static UnityEngine.Experimental.Rendering.RayTracingAccelerationStructure;
using FishNet;
public static class PlayerExtensions
{
    public static void WritePlayerData(this Writer writer, PlayerData value)
    {
        writer.WriteString(value.playerName);
        writer.WriteInt32(value.playerKills);
        writer.WriteInt32(value.playerDeaths);
        writer.WriteInt32(value.playerClientID);
    }
    public static PlayerData ReadPlayerData(this Reader reader)
    {
        PlayerData p = new PlayerData();
        p.playerName = reader.ReadString();
        p.playerKills = reader.ReadInt32();
        p.playerDeaths = reader.ReadInt32();
        p.playerClientID = reader.ReadInt32();
        return p;
    }
}
[System.Serializable]
public struct PlayerData
{
    public string playerName;
    public int playerKills;
    public int playerDeaths;
    public int playerClientID;
    public static bool operator ==(PlayerData c1, PlayerData c2)
    {
        return c1.Equals(c2);
    }

    public static bool operator !=(PlayerData c1, PlayerData c2)
    {
        return !c1.Equals(c2);
    }

}

public class PlayerManager : NetworkBehaviour
{
    private GameManager gameManager;
    public GameObject playerUI;
    public GameObject eliminatedScreen, eliminationScreen;
    public GameObject model;
    [Header("Eliminated Screen")]
    public TextMeshProUGUI secondsLeftText;
    public TextMeshProUGUI eliminatedByText;
    [Header("Elimination Screen")]
    public TextMeshProUGUI eliminationText;
    public TextMeshProUGUI serverPlayerCountText;
    public TextMeshProUGUI eliminationsText;

    public GameObject spawnEffect;
    public PlayerMovement playerMovement;
    public PlayerShoot playerShoot;
    public float respawnDelay, spawnInDelay;

    public PlayerData playerData;

    public GameObject gameUI;
    public List<Renderer> renderers;
    public Color playerColor;
    public Camera mainCam;


    public PlayerAnimator animator;
    public PlayerHealth playerHealth;
    public GunManager gunManager;
    public LeaderboardManager leaderboardManager;
    public sound respawn, death, elimination;

    public DamageNumber damagePopup;

    public TextMeshProUGUI nameText;
    public NetworkObject playerObject;
    private bool isPaused = false;
    private void Awake()
    {
        gameManager = GameManager.instance;
        Hide(false);
    }
    private void Start()
    {
        nameText.gameObject.SetActive(!Owner.IsLocalClient);
        gameUI.gameObject.SetActive(Owner.IsLocalClient);
        mainCam.gameObject.tag = (Owner.IsLocalClient ? "MainCamera" : "Untagged");
    }
    public void SetPlayerName(PlayerData player, Color playerColore)
    {
        //SEt the player data for other clients
        playerData = player;
        nameText.text = player.playerName.ToString();
        gameObject.name = player.playerName.ToString();


        playerColor = playerColore;
        for (int i = 0; i < renderers.Count; i++)
        {
            renderers[i].material.color = playerColor;
        }
    }
    public override void OnStopClient()
    {
        //When player leaves game, unintentionally or intentioanlly
        gameManager.RemovePlayer(Time.time, playerData, NetworkObject);
        DropWeapons();
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        Initialize();
    }
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        //Initialize();
    }
    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
    }
    public void Initialize()
    {
        if (base.IsOwner)
        {
            //Set Player Data
            playerData.playerClientID = Owner.ClientId;
            playerData.playerKills = 0;
            playerData.playerDeaths = 0;
            //If the player didn't input a name, choose a default name "player 1"
            if (gameManager.matchmaker._playerName.text == string.Empty)
                playerData.playerName = "Player " + (gameManager.playerCount+1);
            else
                playerData.playerName = gameManager.matchmaker._playerName.text;

            //SEnd Player Data to SErver
            gameManager.UpdatePlayer(Time.time, playerData, NetworkObject);
        }
        playerUI.SetActive(Owner.IsLocalClient);
        Respawn(GameManager.instance.spawnPositions[UnityEngine.Random.Range(0, GameManager.instance.spawnPositions.Count)].position);
        respawn.Play(transform.position);
    }
    public void Die()
    {
        DropWeapons();
        death.Play(transform.position);
    }
    public void DropWeapons()
    {
        print("DRopped Weapons for player: " + playerData.playerName);
        if(gunManager.equippedGun)
            gunManager.Drop(gunManager.equippedGun, mainCam.transform.forward * 10);
    }

    //This function runs on every client
    [ObserversRpc]
    public void DamagePlayerObservers(int damage, NetworkObject whoGotShot)
    {
        //Get Player Hitbox of player who got hit
        PlayerManager playerHit = whoGotShot.GetComponent<PlayerManager>();
        //Spawn effects
        if (base.IsOwner)
            playerHit.animator.Hit();

        //Damage player who got shot
        playerHit.playerHealth.TakeDamage(damage);
    }
    public void DieScreen(PlayerManager whoKilledMe)
    {
        StartCoroutine(DieScreenCoroutine(whoKilledMe));
    }
    private IEnumerator DieScreenCoroutine(PlayerManager whoKilledMe)
    {
        eliminatedScreen.SetActive(true);
        eliminatedByText.text = "Eliminated by <size=150%><color=red> " + whoKilledMe.playerData.playerName;
        float seconds = 5;
        while (seconds >= 0)
        {
            seconds -= Time.deltaTime;
            secondsLeftText.text = "Respawning in " + Mathf.Round(seconds * 10.0f) * 0.1f;
            yield return null;
        }
        eliminatedScreen.SetActive(false);
        Show(true);
        Respawn(GameManager.instance.spawnPositions[UnityEngine.Random.Range(0, GameManager.instance.spawnPositions.Count)].position);
    }
    [TargetRpc]
    public void Kill(NetworkConnection conn, NetworkObject whoIKilled)
    {

        PlayerManager playerWhoIKilled = whoIKilled.GetComponent<PlayerManager>();
        print("Eliminated + " + playerWhoIKilled.playerData.playerName);

playerData.playerKills++;
        gameManager.UpdatePlayer(Time.time, playerData, NetworkObject);
        gameManager.Elimination(playerData.playerName + " eliminated " + playerWhoIKilled.playerData.playerName);
        StartCoroutine (KillCoroutine(playerWhoIKilled));
    }

    private IEnumerator KillCoroutine(PlayerManager whoIKilled)
    {
        eliminationScreen.SetActive(true);
        elimination.Play(transform.position);
        eliminationText.text = "Eliminated <size=150%><color=red>" + whoIKilled.playerData.playerName;
        yield return new WaitForSeconds(2);
        eliminationScreen.SetActive(false);
    }
    private void Update()
    {
        if (!base.IsOwner)
            return;

        if(Input.GetKeyDown(KeyCode.R))
        {
            Respawn(GameManager.instance.spawnPositions[UnityEngine.Random.Range(0, GameManager.instance.spawnPositions.Count)].position);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
        eliminationsText.text = playerData.playerKills.ToString();
        serverPlayerCountText.text = gameManager.playerCount.ToString();
    }
    private IEnumerator RespawnCoroutine(Vector3 position)
    {
        HideModel();
        transform.position = position;
        GameObject effect = Instantiate(spawnEffect, transform.position, Quaternion.identity);
        Destroy(effect, 10);
        yield return new WaitForSeconds(spawnInDelay);
        ShowModel();
    }
    public void Hide(bool serverHide)
    {
        HideModel();
        if(serverHide)
            CMDHideModel();
    }
    public void HideModel()
    {
        playerMovement.canMove = false;
        playerShoot.enabled = false;
        if(base.IsOwner)
            gameUI.SetActive(false);
        nameText.gameObject.SetActive(false);
        model.SetActive(false);
    }
    public void ShowModel()
    {
        model.SetActive(true);
        if (base.IsOwner)
            gameUI.SetActive(true);
        playerMovement.canMove = true;
        nameText.gameObject.SetActive(!Owner.IsLocalClient);
        playerShoot.enabled = true;
    }
    public void Show(bool serverShow)
    {
        ShowModel();
        if(serverShow)
            CMDShowModel();
    }
    public void PauseGame()
    {
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void QuitGame()
    {

        Application.Quit();

    }
    public void Respawn(Vector3 position)
    {
        StartCoroutine(RespawnCoroutine(position));
        CMDRespawn(position);
    }
    [ServerRpc(RequireOwnership = false)]
    public void CMDRespawn(Vector3 position)
    {
        if (!base.IsOwner)
            StartCoroutine(RespawnCoroutine(position));
        ObserversRespawn(position);
    }
    [ObserversRpc]
    public void ObserversRespawn(Vector3 position)
    {
        if (base.IsOwner || base.IsServerInitialized)
            return;
        StartCoroutine(RespawnCoroutine(position));
    }
    [ObserversRpc]
    public void ObserversHideModel()
    {
        if (base.IsOwner || base.IsServerInitialized)
            return;
        HideModel();
    }
    [ObserversRpc]
    public void ObserversShowModel()
    {
        if (base.IsOwner || base.IsServerInitialized)
            return;
        ShowModel();
    }
    [ServerRpc]
    public void CMDHideModel()
    {
        if (!base.IsOwner)
            HideModel();
        ObserversHideModel();
    }
    [ServerRpc]
    public void CMDShowModel()
    {
        if (!base.IsOwner)
            ShowModel();
        ObserversShowModel();
    }
    public float GetHealth(PlayerManager whoRequested)
    {
        return playerHealth.health;
    }
}
public static class IntBitExtentions
{
    public static MoveState RemoveFlag(this MoveState whole, MoveState part)
    {
        whole &= ~part;
        return whole;
    }
    public static MoveState AddFlag(this MoveState whole, MoveState part)
    {
        whole |= part;
        return whole;
    }
    public static bool Contains(this MoveState whole, MoveState part)
    {
        return (whole & part) == part;
    }
}