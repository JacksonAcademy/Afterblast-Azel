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
using FishNet.Object.Synchronizing;
public class GameManager : NetworkBehaviour
{
    public Vector3 spawnOffset;




    public List<Color> playerColors;
    public List<Transform> spawnPositions;
    public GameMatchmaker matchmaker;
    public static GameManager instance;

    public GameObject disconnectedUI;
    [HideInInspector] public KillfeedManager killfeedManager;
    public NetworkManager networkManager;
    public NetworkObject playerObject;
    [HideInInspector] public bool IsReady = false;
    public GameObject playerDespawnEffect;
    public List<PlayerData> players;
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
        ServerSpawnPlayer(LocalConnection);
    }
    [ServerRpc(RequireOwnership = false)]
    public void ServerSpawnPlayer(NetworkConnection ahkljalkrmhh)
    {
        print("Server spawn player");
        GameObject nob = Instantiate(playerObject.gameObject);
        InstanceFinder.ServerManager.Spawn(nob, ahkljalkrmhh);
        ahkljalkrmhh.SetFirstObject(nob.GetComponent<NetworkObject>());
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        GameMatchmaker.instance.HideLoadingScreen();
    }


    private void Start()
    {
        killfeedManager = KillfeedManager.instance;

    }
    public Vector3 GetSpawnPoint()
    {
        return spawnPositions[UnityEngine.Random.Range(0, spawnPositions.Count)].position;
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
                GameMatchmaker.instance.Lobby();

            }
        }
    }
}
