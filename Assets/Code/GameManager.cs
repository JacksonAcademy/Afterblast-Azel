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
using UnityEngine.UIElements;
using System;
using UnityEditor.MemoryProfiler;
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

    public event Action<NetworkObject> OnSpawned;
    void Awake()
    {
        instance = this;
        disconnectedUI.SetActive(false);
        
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        IsReady = true;
    }
    private void Start()
    {
        killfeedManager = KillfeedManager.instance;

        networkManager.SceneManager.OnClientLoadedStartScenes += Play;
    }
    [ServerRpc]
    public void UpdatePlayer(float timeSent, PlayerData playerToUpdate, NetworkObject player)
    {
        UpdatePlayerLocal(timeSent, playerToUpdate, player);
        UpdateObserverPlayers(timeSent, playerToUpdate, player);
    }
    [ObserversRpc(BufferLast = true)]
    public void UpdateObserverPlayers(float timeSent, PlayerData playerData, NetworkObject player)
    {
        UpdatePlayerLocal(timeSent, playerData, player);
    }
    public Vector3 GetSpawnPoint()
    {
        return spawnPositions[UnityEngine.Random.Range(0, spawnPositions.Count)].position;
    }
    private void Play(NetworkConnection conn, bool asServer)
    {
        if (!asServer)
            return;
        GiveOwnership(conn);
        NetworkObject nob = Instantiate(playerObject);
        networkManager.ServerManager.Spawn(nob, conn);
        //nob.LocalConnection.SetFirstObject(nob);
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
                print("Player updated:" + playerData.playerName);
            }
        }
        if (!containsPlayerAlready)
        {
            //Player joined teh game
            players.Add(playerData);
            if (currentTime - killfeedDelay < timeSent)
                killfeedManager.AddItem(playerData.playerName + " joined the game!");
        }
        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        playerManager.ClientSync(playerData, playerColors[players.Count]);
    }
    [ServerRpc(RequireOwnership = false)]
    public void RemovePlayerServer(float timeSent, PlayerData playerToRemove, NetworkObject player)
    {
        print("SERVER MADE PLAYER LEAVE: " + playerToRemove.playerName);
        RemovePlayerLocal(timeSent, playerToRemove, player);
        RemovePlayerObserver(timeSent, playerToRemove, player);
    }
    public void RemovePlayerLocal(float timeSent, PlayerData playerToRemove, NetworkObject player)
    {
        //We don't want players adding things to the killfeed a certain time after the RPC has been sent, or else it'll clog up
      //  float currentTime = Time.time;
       // float killfeedDelay = 2;
        //if (currentTime - killfeedDelay < timeSent)
            killfeedManager.AddItem(playerToRemove.playerName + " left the game!");
        players.Remove(playerToRemove);
        GameObject effect = Instantiate(playerDespawnEffect, player.transform.position, Quaternion.identity);
        Destroy(effect, 2);
        Destroy(player.gameObject);

    }
    [ObserversRpc(BufferLast = true)]
    public void RemovePlayerObserver(float timeSent, PlayerData playerToRemove, NetworkObject player)
    {
        print("Player left: " + playerToRemove.playerName);
        RemovePlayerLocal(timeSent, playerToRemove, player);
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
            GameMatchmaker.instance._lobby.SetActive(true);
            GameMatchmaker.instance._buttons.SetActive(true);
        }

    }
}
