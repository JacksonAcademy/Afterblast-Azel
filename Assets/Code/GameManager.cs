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

public class GameManager : NetworkBehaviour
{
    public List<PlayerData> players;
    public int playerCount;
    public List<Color> playerColors;
    public List<Transform> spawnPositions;
    public GameMatchmaker matchmaker;
    public static GameManager instance;

    public GameObject disconnectedUI;
    void Awake()
    {
        disconnectedUI.SetActive(false);
        instance = this;
    }
    [ServerRpc]
    public void AddPlayer(PlayerData playerToAdd)
    {
        players.Add(playerToAdd);
        print(playerToAdd.playerName + " joined the game!");
    }
    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        disconnectedUI.SetActive(true);
    }
}
