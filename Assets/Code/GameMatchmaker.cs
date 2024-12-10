using FishNet.Managing;
using FishNet.Transporting.UTP;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;

using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using UnityEngine;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using FishNet.Transporting.Multipass;
using FishNet.Managing.Transporting;
using FishNet.Object;
using FishNet.Managing.Server;
using FishNet;
using FishNet.Connection;





#if UNITY_EDITOR
using ParrelSync;
#endif
using UnityEngine.UI;
public enum transportType
{ fishyUnity, tugboat
}

public class GameMatchmaker : MonoBehaviour
{
    public transportType Transport;
    public GameObject _buttons, _lobby;
    private bool serverBuild;

    public GameObject loadingScreen, failedToConnect;
    //public NetworkObject player;

    public Camera lobbyCam, serverSpectateCamera;
    public TextMeshProUGUI _debugText;
    public NetworkManager _networkManager;
    public TMP_InputField _playerName, clientIPAddress;
    public static GameMatchmaker instance;
    public Transport tugboat;

    public bool spawnPlayer = true;

    public string defaultIP = "18.191.185.25";
    public float loadingWaitTime = 5;
    private void Awake()
    {
        instance = this;


        Lobby();

        if (Application.isBatchMode && !Application.isEditor)
            serverBuild = true;
        else
            serverBuild = false;
    }

    public void Lobby()
    {
        _lobby.SetActive(true);
        _buttons.SetActive(true);

        lobbyCam.enabled = true;
        serverSpectateCamera.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void Start()
    {
        if (serverBuild)
        {
            StartServer();
        }
        SetIPAddress(defaultIP);
        clientIPAddress.SetTextWithoutNotify(defaultIP);
        InitializeAsync();

    }
    public async void InitializeAsync()
    {
        var options = new InitializationOptions();
#if UNITY_EDITOR 
        options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
#endif
        await UnityServices.InitializeAsync(options);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    public void SetIPAddress(string ip)
    {
        tugboat.SetClientAddress(ip);
        if(string.IsNullOrEmpty(ip))
        {
            tugboat.SetClientAddress("18.191.185.25");
        }
    }
    public void StartServer()
    {
        print("Started server. Dedicated server? " + (Application.isBatchMode && !Application.isEditor));
        spawnPlayer = false;
        tugboat.StartConnection(true);
        if (Application.isBatchMode && !Application.isEditor)
            tugboat.StartConnection(false);

        lobbyCam.enabled = false;
        serverSpectateCamera.enabled = true;

        _buttons.SetActive(false);
        _lobby.SetActive(false);
    }
    public void Play()
    {
        StartCoroutine("StartLoading");
        spawnPlayer = true;
        GameManager.instance.gameObject.SetActive(true);

        if (_playerName.text == "server")
        {
            tugboat.SetClientAddress("localhost");
            tugboat.StartConnection(true);
        }
        else if(_playerName.text == "client")
        {
            tugboat.SetClientAddress("localhost");
        }

        tugboat.StartConnection(false);
    }    
    public IEnumerator StartLoading()
    {
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(loadingWaitTime);
        loadingScreen.SetActive(false);
        failedToConnect.SetActive(true);
        _buttons.SetActive(true);
        _lobby.SetActive(true);
        lobbyCam.enabled = true;

    }
    public void HideLoadingScreen()
    {
        lobbyCam.enabled = false;
        StopCoroutine("StartLoading");
        loadingScreen.SetActive(false);
        _buttons.SetActive(false);
        _lobby.SetActive(false);
    }
}
