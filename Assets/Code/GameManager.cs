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
using FishNet.Demo.AdditiveScenes;

public class GameManager : NetworkBehaviour
{
    public List<PlayerData> players;
    public int playerCount;
    public List<Color> playerColors;
    public List<Transform> spawnPositions;
    public GameMatchmaker matchmaker;
    public static GameManager instance;

    public GameObject disconnectedUI;
    private KillfeedManager killfeedManager;
    void Awake()
    {
        instance = this;
        disconnectedUI.SetActive(false);
    }
    private void Start()
    {
        killfeedManager = KillfeedManager.instance;
    }
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayer(float timeSent, PlayerData playerToUpdate, NetworkObject player)
    {
        UpdateObserverPlayers(timeSent, playerToUpdate, player);
    }
    [ObserversRpc(BufferLast = true)]
    public void UpdateObserverPlayers(float timeSent, PlayerData playerData, NetworkObject player)
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
        if(!containsPlayerAlready)
        {
            //Player joined teh game
            players.Add(playerData);
            if (currentTime - killfeedDelay < timeSent)
                killfeedManager.AddItem(playerData.playerName + " joined the game!");
        }
        player.GetComponent<PlayerManager>().SetPlayerName(playerData, playerColors[players.Count]);
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
    [ServerRpc(RequireOwnership = false)]
    public void RemovePlayer(float timeSent, PlayerData playerToRemove, NetworkObject player)
    {
        UpdateObserverPlayers(timeSent, playerToRemove, player);
    }
    [ObserversRpc(BufferLast = true)]
    public void RemovePlayerObserver(float timeSent, PlayerData playerToRemove, NetworkObject player)
    {
        //We don't want players adding things to the killfeed a certain time after the RPC has been sent, or else it'll clog up
        float currentTime = Time.time;
        float killfeedDelay = 2;
        if(currentTime - killfeedDelay < timeSent)
            killfeedManager.AddItem(playerToRemove.playerName + " left the game!");

        players.Remove(playerToRemove);

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
