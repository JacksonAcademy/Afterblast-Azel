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
    public void UpdatePlayer(PlayerData playerToUpdate)
    {
        UpdateObserverPlayers(playerToUpdate);
    }
    [ObserversRpc]
    public void UpdateObserverPlayers(PlayerData player)
    {
        if (players.Contains(player))
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].playerClientID == player.playerClientID)
                {
                    //Updated Player stats
                    players[i] = player;

                }
            }
        }
        else
        {
            //Player joined teh game
            print("Player joined teh game!");
            players.Add(player);
            killfeedManager.AddItem(player.playerName + " joined the game!");
        }
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
    public void RemovePlayer(PlayerData playerToRemove)
    {
        UpdateObserverPlayers(playerToRemove);
    }
    [ObserversRpc]
    public void RemovePlayerObserver(PlayerData playerToRemove)
    {
        players.Remove(playerToRemove);
        killfeedManager.AddItem(playerToRemove.playerName + " left the game!");
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
