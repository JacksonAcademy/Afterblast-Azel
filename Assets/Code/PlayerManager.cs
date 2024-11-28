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
using FishNet;
using Unity.Services.Lobbies.Models;
using UnityEngine.EventSystems;
using Demo.Scripts.Runtime.Item;
using FishNet.Transporting.Tugboat;
using FishNet.Managing.Server;
using FishNet.Managing.Client;
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

    public weaponManager weaponManager;

    public EmptyParentBehaviour weaponBone;

    public List<FPSItem> weapons = new List<FPSItem>();

    public GameObject spawnEffect;
    public PlayerMovement playerMovement;
    public PlayerShoot playerShoot;
    public float respawnDelay, spawnInDelay;

    public PlayerData playerData;

    public AudioListener listener;

    public GameObject gameUI;
    public List<Renderer> renderers;
    public Color playerColor;
    public Camera mainCam;


    public PlayerAnimator animator;
    public PlayerHealth playerHealth;
    public LeaderboardManager leaderboardManager;
    public sound respawn, death, elimination;

    public DamageNumber damagePopup;

    public TextMeshProUGUI nameText;
    public NetworkObject playerObject;
    private bool isPaused = false;

    private void Awake()
    {
        mainCam.enabled = false;
        gameManager = GameManager.instance;
        Hide(false);
        //Initialize();
        //print("Player joined: " + playerData.playerName);

    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkManager.SceneManager.AddOwnerToDefaultScene(NetworkObject);
    }
    private void Start()
    {

        nameText.gameObject.SetActive(!Owner.IsLocalClient);
        gameUI.gameObject.SetActive(Owner.IsLocalClient);
        mainCam.gameObject.tag = (Owner.IsLocalClient ? "MainCamera" : "Untagged");
        Initialize();
    }
    public override void OnStartServer()
    {
        base.OnStartServer(); 
        ServerManager.Objects.OnPreDestroyClientObjects += OnPreDestroyClientObjects;
    }
    public void ClientSync(PlayerData player, Color playerColore)
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
    public void Initialize()
    {

        //Set Player Data
        playerData.playerClientID = Owner.ClientId;
        playerData.playerKills = 0;
        playerData.playerDeaths = 0;
        //If the player didn't input a name, choose a default name "player 1"
        if (gameManager.matchmaker._playerName.text == string.Empty)
            playerData.playerName = "Player " + (gameManager.players.Count + 1);
        else
            playerData.playerName = gameManager.matchmaker._playerName.text;

        //SEnd Player Data to SErver
        if(Owner.IsLocalClient)
        {
            gameManager.UpdatePlayer(Time.time, playerData, NetworkObject);
            Respawn(GameManager.instance.spawnPositions[UnityEngine.Random.Range(0, GameManager.instance.spawnPositions.Count)].position);
           // print("Sending player data to server");
        }

        Show(false);

        mainCam.enabled = Owner.IsLocalClient;
        listener.enabled = Owner.IsLocalClient;
        name = playerData.playerName;
        playerUI.SetActive(Owner.IsLocalClient);
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
    }
    private void OnPreDestroyClientObjects(NetworkConnection conn)
    {
        if(conn == Owner)
        {
            DropWeapons();
            RemoveOwnership();
            gameManager.RemovePlayerServer(Time.time, playerData, NetworkObject, transform.position);
        }
    }
    public void LeaveGame()
    {
        if (IsServerInitialized)
            ServerManager.StopConnection(true);
        else
            ClientManager.StopConnection();
        GameMatchmaker.instance.Lobby();

    }
    public override void OnStopServer()
    {
        if (ServerManager != null)
            ServerManager.Objects.OnPreDestroyClientObjects -= OnPreDestroyClientObjects;
    }
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
        serverPlayerCountText.text = gameManager.players.Count.ToString();
    }
    public IEnumerator RespawnCoroutine(Vector3 position)
    {
        HideModel();
        transform.position = position + gameManager.spawnOffset;
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
        if(base.Owner.IsLocalClient)
            gameUI.SetActive(false);
        nameText.gameObject.SetActive(false);
        model.SetActive(false);
    }
    public void ShowModel()
    {
        model.SetActive(true);
        if (base.Owner.IsLocalClient)
            gameUI.SetActive(true);
        playerMovement.canMove = true;
        nameText.gameObject.SetActive(!Owner.IsLocalClient);
        playerShoot.enabled = true;
    }
    public void DropItem()
    {

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
    [ServerRpc(RequireOwnership = false)]
    public void Respawn(Vector3 position)
    {
        StartCoroutine(RespawnCoroutine(position));
        ObserversRespawn(position);
    }
    [ObserversRpc]
    public void ObserversRespawn(Vector3 position)
    {
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