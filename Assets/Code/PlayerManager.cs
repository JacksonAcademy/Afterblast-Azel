using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine;
using FishNet.Connection;
using Unity.VisualScripting;
using FishNet.Serializing;
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
}

public class PlayerManager : NetworkBehaviour
{
    public GameObject playerUI;
    public GameObject eliminatedScreen, eliminationScreen;
    public GameObject model;
    [Header("Eliminated Screen")]
    public TextMeshProUGUI secondsLeftText;
    public TextMeshProUGUI eliminatedByText;
    [Header("Elimination Screen")]
    public TextMeshProUGUI eliminationText;

    public GameObject spawnEffect;
    public PlayerMovement playerMovement;
    public PlayerShoot playerShoot;
    public float respawnDelay, spawnInDelay;

    public PlayerData playerData;

    public GameObject gameUI;
    public List<Renderer> renderers;
    public Color playerColor;
    public Camera mainCam;

    public TextMeshProUGUI nameText;
    private bool isPaused = false;
    private void Awake()
    {
        Hide();
    }
    private void Start()
    {
        GameManager manager = GameManager.instance;
        playerColor = manager.playerColors[manager.playerCount];

        nameText.gameObject.SetActive(!Owner.IsLocalClient);
        mainCam.gameObject.tag = (Owner.IsLocalClient ? "MainCamera" : "Untagged");
    }
    private void FixedUpdate()
    {
        if(playerData.playerName!= string.Empty)
        {
            nameText.text = playerData.playerName.ToString();
            gameObject.name = playerData.playerName.ToString();
        }
    }
    public override void OnStartNetwork()
    {
        Initialize();
        //Respawn();
    }
    public override void OnStopNetwork()
    {
        //When player leaves game, unintentionally or intentioanlly
        GameManager.instance.playerCount--;
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

    }
    public void Initialize()
    {
        GameManager manager = GameManager.instance;
        if (Owner.IsLocalClient)
        {
            manager.playerCount++;

            //Set Player Data
            playerData.playerClientID = Owner.ClientId;
            playerData.playerKills = 0;
            playerData.playerDeaths = 0;
            //If the player didn't input a name, choose a default name "player 1"
            if (manager.matchmaker._playerName.text == string.Empty)
                playerData.playerName = "Player " + manager.playerCount;
            else
                playerData.playerName = manager.matchmaker._playerName.text;

            //SEnd Player Data to SErver
            manager.AddPlayer(playerData);
        }
        playerColor = manager.playerColors[manager.playerCount];
        for (int i = 0; i < renderers.Count; i++)
        {
            renderers[i].material.color = playerColor;
        }

        playerUI.SetActive(Owner.IsLocalClient);
        Respawn(GameManager.instance.spawnPositions[UnityEngine.Random.Range(0, GameManager.instance.spawnPositions.Count)].position);
    }
    public void Die(PlayerManager whoKilledMe)
    {
        StartCoroutine(DieCoroutine(whoKilledMe));
    }
    private IEnumerator DieCoroutine(PlayerManager whoKilledMe)
    {
        eliminatedScreen.SetActive(true);
        Hide();
        print("You died to: " + whoKilledMe.playerData.playerName);
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
    public void Kill(NetworkConnection conn, PlayerManager whoIKilled)
    {
        StartCoroutine (KillCoroutine(whoIKilled));
        print("Killed player!");
    }

    private IEnumerator KillCoroutine(PlayerManager whoIKilled)
    {
        print("You killed:" + whoIKilled.playerData.playerName);
        eliminationScreen.SetActive(true);
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
    }
    private IEnumerator RespawnCoroutine(Vector3 position)
    {
        HideModel();
        print("REspawning player");
        transform.position = position;
        GameObject effect = Instantiate(spawnEffect, transform.position, Quaternion.identity);
        Destroy(effect, 10);
        yield return new WaitForSeconds(spawnInDelay);
        ShowModel();
    }
    public void Hide()
    {
        HideModel();
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
    public void Show()
    {
        ShowModel();
        CMDShowModel();
    }
    public override void OnStopClient()
    {
        base.OnStopClient();  
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
    [ServerRpc]
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