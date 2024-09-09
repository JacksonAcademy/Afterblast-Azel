using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using FishNet.Connection;
using Unity.VisualScripting;
using FishNet.Serializing;
using FishNet.CodeGenerating;
using FishNet.Managing.Object;
using FishNet.Managing.Server;
using Unity.Services.Lobbies.Models;
using FishNet;
using FishNet.Managing;
using System;
public class GameManager : NetworkBehaviour
{
    public Vector3 spawnOffset;
    public List<PlayerData> players;
    public List<Color> playerColors;
    public List<Transform> spawnPositions;
    public GameMatchmaker matchmaker;
    public static GameManager instance;
    public GameObject playerToSpawn;
    public GameObject disconnectedUI;
    private KillfeedManager killfeedManager;
    public NetworkManager networkManager;
    public NetworkObject playerObject;
    [HideInInspector] public bool IsReady = false;
    public GameObject playerDespawnEffect;
    void Awake()
    {
        instance = this;
        disconnectedUI.SetActive(false);

        //networkManager.SceneManager.OnClientLoadedStartScenes += Play;
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        IsReady = true;
        SpawnPlayer(LocalConnection);
    }
    ///[ServerRpc(RequireOwnership =false)]
    public void SpawnPlayer(NetworkConnection local)
    {
        if (!GameMatchmaker.instance.spawnPlayer)
            return;

        NetworkObject nob = Instantiate(playerObject);
        Spawn(nob, LocalConnection);
    }

    private void Start()
    {
        killfeedManager = KillfeedManager.instance;

    }
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayer(float timeSent, PlayerData playerToUpdate, NetworkObject player)
    {
        UpdatePlayerLocal(timeSent, playerToUpdate, player);
        UpdateObserverPlayers(timeSent, playerToUpdate, player);
    }
    [ObserversRpc(BufferLast = true, ExcludeServer = true)]
    public void UpdateObserverPlayers(float timeSent, PlayerData playerData, NetworkObject player)
    {
        UpdatePlayerLocal(timeSent, playerData, player);
    }
    public Vector3 GetSpawnPoint()
    {
        return spawnPositions[UnityEngine.Random.Range(0, spawnPositions.Count)].position;
    }
    public void UpdatePlayerLocal(float timeSent, PlayerData playerData, NetworkObject player)
    {
        float currentTime = Time.time;
        float killfeedDelay = 2;

        bool containsPlayerAlready = false;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerClientID == playerData.playerClientID)
            {
                containsPlayerAlready = true;
                players[i] = playerData;
            }
        }
        if (!containsPlayerAlready)
        {
            print("Player joined: " + playerData.playerName);
            //Player joined teh game
            players.Add(playerData);
            if (currentTime - killfeedDelay < timeSent)
                killfeedManager.AddItem(playerData.playerName + " joined the game!");
        }
        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        playerManager.ClientSync(playerData, playerColors[players.Count]);
    }
    [ServerRpc(RequireOwnership = false)]
    public void RemovePlayerServer(float timeSent, PlayerData playerToRemove, NetworkObject player, Vector3 despawnPos)
    {
        if (player == null)
            return;

        RemovePlayerLocal(timeSent, playerToRemove, player, despawnPos);
        RemovePlayerObserver(timeSent, playerToRemove, player, despawnPos);
        print("Player left: " + playerToRemove.playerName);
        //ServerManager.Kick;
        //player.Despawn();
    }
    public void RemovePlayerLocal(float timeSent, PlayerData playerToRemove, NetworkObject player, Vector3 despawnPos)
    {
        //We don't want players adding things to the killfeed a certain time after the RPC has been sent, or else it'll clog up
        //  float currentTime = Time.time;
        // float killfeedDelay = 2;
        //if (currentTime - killfeedDelay < timeSent)
        killfeedManager.AddItem(playerToRemove.playerName + " left the game!");
        players.Remove(playerToRemove);

        GameObject effect = Instantiate(playerDespawnEffect, despawnPos, Quaternion.identity);
        Destroy(effect, 2);

        Destroy(player.gameObject);
    }
    [ObserversRpc(BufferLast = true)]
    public void RemovePlayerObserver(float timeSent, PlayerData playerToRemove, NetworkObject player, Vector3 despawnPos)
    {
        //Server already removed player
        if (IsServerInitialized)
            return;

        RemovePlayerLocal(timeSent, playerToRemove, player, despawnPos);
    }
    [ServerRpc(RequireOwnership=false)]
    public void Elimination(string elimination)
    {
        ObserversElimination(elimination);
    }
    [ObserversRpc]
    public void ObserversElimination(string elimination)
    {
        killfeedManager.AddItem(elimination);
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        if(!IsServerInitialized)
        {
            disconnectedUI.SetActive(true);
            if(GameMatchmaker.instance)
            {
                if (!GameMatchmaker.instance._lobby)
                    return;

                GameMatchmaker.instance._lobby.SetActive(true);
                GameMatchmaker.instance._buttons.SetActive(true);
                GameMatchmaker.instance.lobbyCam.enabled = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

            }
        }

    }
}
