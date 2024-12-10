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
using FishNet.Demo.AdditiveScenes;
using FishNet.Transporting;
using KINEMATION.FPSAnimationFramework.Runtime.Camera;
using FishNet.Object.Synchronizing;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
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

    public NetworkObject player;
    public static bool operator ==(PlayerData c1, PlayerData c2)
    {
        return c1.Equals(c2);
    }

    public static bool operator !=(PlayerData c1, PlayerData c2)
    {
        return !c1.Equals(c2);
    }
    public override bool Equals(object o)
    {
        if (o == null)
            return false;
        return true;
    }

    public override int GetHashCode()
    {
        return 0;
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

    public bool hideHeadPartsLocalClient;
    public List<SkinnedMeshRenderer> headParts;

    public weaponManager weaponManager;

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
    public FPSCameraController fpsCamera;

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
        fpsCamera.enabled = false;
        gameManager = GameManager.instance;
        HideModel();
    }
    private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
    {
        if(args.ConnectionState != RemoteConnectionState.Started)
            RemovePlayer(connection.FirstObject.GetComponent<PlayerManager>().playerData, connection.FirstObject.transform);
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;

        playerData.playerClientID = Owner.ClientId;
        playerData.playerKills = 0;
        playerData.playerDeaths = 0;


        if (base.IsOwner)
        {
            playerData.player = NetworkObject;
            Vector3 spawnPosition = GameManager.instance.spawnPositions[UnityEngine.Random.Range(0, GameManager.instance.spawnPositions.Count)].position;
            Respawn(spawnPosition);

            if (gameManager.matchmaker._playerName.text == string.Empty)
                playerData.playerName = "Player " + (ServerManager.Clients.Count + 1);
            else
                playerData.playerName = gameManager.matchmaker._playerName.text;

            ServerAddPlayer(playerData);
            GetUpdatedPlayers(LocalConnection);
        }
        
    }
    [ServerRpc]
    public void GetUpdatedPlayers(NetworkConnection sendTo)
    {
        print("Server received request. Player Count: " + gameManager.players.Count);
        GetUpdated(sendTo, gameManager.players);
    }
    [TargetRpc]
    public void GetUpdated(NetworkConnection conn, List<PlayerData> updatedPlayers)
    {
        for(int i = 0; i < updatedPlayers.Count; i++)
        {
            AddPlayer(updatedPlayers[i]);
        }
    }
    public void ChangePlayerName(string playerName)
    {
        playerData.playerName = playerName;
        SetPlayerData(playerData);
    }
    public void SetPlayerData(PlayerData playerData)
    {
        ServerAddPlayer(playerData);
    }

    [ServerRpc]
    public void ServerAddPlayer(PlayerData playerDataa)
    {
        AddPlayer(playerDataa);
        ObserverAddPlayer(playerDataa);
    }
    public void AddPlayer(PlayerData newPlayer)
    {
        NetworkConnection playerConnection = null;

        if(IsServerInitialized)
            ServerManager.Clients.TryGetValue(newPlayer.playerClientID, out playerConnection);
        else
            InstanceFinder.ClientManager.Clients.TryGetValue(newPlayer.playerClientID, out playerConnection);

        if(playerConnection != null)
        {
            PlayerManager playe = playerConnection.FirstObject.GetComponent<PlayerManager>();
            playe.gameObject.name = newPlayer.playerName;
            playe.playerData = newPlayer;
            playe.nameText.text = newPlayer.playerName;

            newPlayer.player = playerConnection.FirstObject;
        }
        else
        {
            print("Couldn't find player on Client list: " + newPlayer.playerName);
        }

        bool foundPlayer = false;
        for (int i = 0; i < gameManager.players.Count; i++)
        {
            if (gameManager.players[i].playerClientID == newPlayer.playerClientID)
            {
                print("Updated player: " + newPlayer.playerName);
                gameManager.players[i] = newPlayer;
                foundPlayer = true;
                break;
            }
        }
        if(!foundPlayer)
        {
            print("Added player: " + newPlayer.playerName);
            gameManager.players.Add(newPlayer);
        }

        leaderboardManager.UpdatePlayer(newPlayer);
    }
    [ObserversRpc]
    public void ObserverAddPlayer(PlayerData player)
    {
        AddPlayer(player);
       
    }
    
    private void Start()
    {
        nameText.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(false);
        mainCam.gameObject.tag = (Owner.IsLocalClient ? "MainCamera" : "Untagged");
        listener.enabled = false;
        playerUI.SetActive(false);
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        ServerManager.Objects.OnPreDestroyClientObjects += OnPreDestroyClientObjects;

    }
    public void Die()
    {
        playerMovement.canMove = false;
        DropWeapons();
        animator._animator.SetTrigger("Death");
        death.Play(transform.position);
    }

    public void DropWeapons()
    {
        weaponManager.DropWeapon(weaponManager.weaponPickup.NetworkObject);
    }
    private void OnPreDestroyClientObjects(NetworkConnection conn)
    {
        if(conn == Owner)
        {
            RemoveOwnership();
        }
    }
    
    public void RemovePlayer(PlayerData player, Transform playerObject)
    {
        gameManager.killfeedManager.AddItem(player.playerName + " left the game!");
        gameManager.players.Remove(player);

        GameObject effect = Instantiate(gameManager.playerDespawnEffect, playerObject.transform.position, Quaternion.identity);
        Destroy(effect, 2);

        Destroy(playerObject.gameObject);
    }
    public override void OnStopServer()
    {
        ServerManager.Objects.OnPreDestroyClientObjects -= OnPreDestroyClientObjects;
        ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
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
        playerMovement.canMove = false;
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
        Respawn(GameManager.instance.spawnPositions[UnityEngine.Random.Range(0, GameManager.instance.spawnPositions.Count)].position);
    }
    [TargetRpc]
    public void Kill(NetworkConnection conn, NetworkObject whoIKilled)
    {

        PlayerManager playerWhoIKilled = whoIKilled.GetComponent<PlayerManager>();
        print("Eliminated + " + playerWhoIKilled.playerData.playerName);

        playerData.playerKills++;
        SetPlayerData(playerData);
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
        if(base.IsOwner && hideHeadPartsLocalClient)
        {
            for (int i = 0; i < headParts.Count; i++)
                headParts[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
        else
        {
            for (int i = 0; i < headParts.Count; i++)
                headParts[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
        if (!base.IsOwner)
            return;
        if (base.IsOwner && Input.GetKeyDown(KeyCode.Space))
        {

            SetPlayerData(playerData);
            GetUpdatedPlayers(LocalConnection);
        }

        if (Input.GetKeyDown(KeyCode.R))
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
        mainCam.enabled = false;
        HideModel();     
        transform.position = position + gameManager.spawnOffset;
       // UpdatePlayer(Time.time, playerData, NetworkObject);
        GameObject effect = Instantiate(spawnEffect, transform.position, Quaternion.identity);
        Destroy(effect, 3);

        yield return new WaitForSeconds(spawnInDelay);

        model.SetActive(true);


        if (base.Owner.IsLocalClient)
        {
            mainCam.enabled = true;
            fpsCamera.enabled = true;
            fpsCamera.Initialize();
            gameUI.SetActive(true);
            playerUI.SetActive(true);
            listener.enabled = true;
            respawn.Play(transform.position);

        }

        animator._animator.SetTrigger("Spawn");
        ShowModel();
        playerMovement.canMove = false;
        animator.fpsAnimator.Initialize();
        animator.fpsAnimator.RebuildPlayables();
        yield return new WaitForSeconds(2);
        playerMovement.canMove = true;
    }
    public void HideModel()
    {
        playerMovement.canMove = false;
        playerShoot.enabled = false;
        if(base.Owner.IsLocalClient)
            gameUI.SetActive(false);
        if(nameText)
            nameText.gameObject.SetActive(false);
        model.SetActive(false);
    }
    public void ShowModel()
    {
        model.SetActive(true);
        animator.fpsAnimator.Initialize();
        animator.fpsAnimator.RebuildPlayables();
 
        if (base.Owner.IsLocalClient)
            gameUI.SetActive(true);
        playerMovement.canMove = true;
        if(nameText)
            nameText.gameObject.SetActive(!Owner.IsLocalClient);
        playerShoot.enabled = true;
    }
    public void PauseGame()
    {
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void LeaveGame()
    {
        if (IsServerInitialized)
            ServerManager.StopConnection(true);
        else
            ClientManager.StopConnection();
        GameMatchmaker.instance.Lobby();
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
    [ObserversRpc(BufferLast =true)]
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