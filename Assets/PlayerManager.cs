using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

    public string playerName;
    public int playerKills;
    public int playerDeaths;

    public GameObject gameUI;
    public List<Renderer> renderers;
    public int playerNumber;
    public Color playerColor;

    public TextMeshProUGUI nameText;
    public override void OnStartNetwork()
    {
        Initialize();
        //Respawn();
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        Respawn(GameManager.instance.spawnPositions[Random.Range(0, GameManager.instance.spawnPositions.Count)].position);
    }
    public void Initialize()
    {
        playerUI.SetActive(Owner.IsLocalClient);
    }
    public void Eliminated(PlayerManager playerEliminated)
    {
        StartCoroutine(EliminatedCoroutine(playerEliminated));
    }
    public void Elimination(PlayerManager playerElimination)
    {
        StartCoroutine (EliminationCoroutine(playerElimination));
    }
    private IEnumerator EliminatedCoroutine(PlayerManager playerEliminated)
    {
        eliminatedScreen.SetActive(true);
        Hide();
        eliminatedByText.text = "Eliminated by " + playerEliminated.playerName;
        float seconds = 5;
        while(seconds >= 0)
        {
            seconds -= Time.deltaTime;
            secondsLeftText.text = "Respawning in " + Mathf.Round(seconds * 10.0f) * 0.1f;
            yield return null;
        }
        eliminatedScreen.SetActive(false);
        Respawn(GameManager.instance.spawnPositions[Random.Range(0, GameManager.instance.spawnPositions.Count)].position);
    }
    private IEnumerator EliminationCoroutine(PlayerManager playerElimination)
    {
        eliminationScreen.SetActive(true);
        eliminationText.text = "Eliminated" + playerElimination.playerName;
        yield return new WaitForSeconds(2);
        eliminationScreen.SetActive(false);
    }
    private void Update()
    {
        if (!base.IsOwner)
            return;

        if(Input.GetKeyDown(KeyCode.R))
        {
            Respawn(GameManager.instance.spawnPositions[Random.Range(0, GameManager.instance.spawnPositions.Count)].position);
        }
    }
    public void Respawn(Vector3 position)
    {
        StartCoroutine(RespawnCoroutine(position));
        CMDRespawn(position);
    }
    [ServerRpc]
    public void CMDRespawn(Vector3 position)
    {
        if(!base.IsOwner)
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
        if(!base.IsOwner)
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
    private IEnumerator RespawnCoroutine(Vector3 position)
    {
        print("REspawning player");
        transform.position = position;
        Hide();
        GameObject effect = Instantiate(spawnEffect, transform.position, Quaternion.identity);
        Destroy(effect, 10);
        yield return new WaitForSeconds(spawnInDelay);
        Show();
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
        gameUI.SetActive(false);
        nameText.gameObject.SetActive(false);
        model.SetActive(false);
    }
    public void ShowModel()
    {
        model.SetActive(true);
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
    private void Start()
    {
        Hide();
        GameManager.instance.playerCount++;
        playerNumber = Owner.ClientId;
        playerColor = GameManager.instance.playerColors[playerNumber];
        playerName = GameManager.instance.playerNames[playerNumber];


        nameText.gameObject.SetActive(!Owner.IsLocalClient);
        name = playerName;


    }
    private void FixedUpdate()
    {
        for (int i = 0; i < renderers.Count; i++)
        {
            renderers[i].material.color = playerColor;
        }
        nameText.text = playerName;
    }
    public override void OnStopNetwork()
    {
        GameManager.instance.playerCount--;
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