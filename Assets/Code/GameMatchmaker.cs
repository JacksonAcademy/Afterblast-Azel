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

    public NetworkObject player;

    public Camera lobbyCam, serverSpectateCamera;
    public TextMeshProUGUI _debugText;
    public NetworkManager _networkManager;
    public TMP_InputField _playerName, clientIPAddress;
    public static GameMatchmaker instance;
    public Transport tugboat;

    public bool spawnPlayer = true;
    private void Awake()
    {
        instance = this;

        _lobby.SetActive(true);

        lobbyCam.enabled = true;
        serverSpectateCamera.enabled = false;

        _buttons.SetActive(true);

        if (Application.isBatchMode && !Application.isEditor)
            serverBuild = true;
        else
            serverBuild = false;
    }
    private void Start()
    {
        if (serverBuild)
        {
            StartServer();
        }
        SetIPAddress("18.191.185.25");
        clientIPAddress.SetTextWithoutNotify("18.191.185.25");
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

       // GameManager.instance.StartServer();

    }

    public void Play()
    {
        spawnPlayer = true;
        GameManager.instance.gameObject.SetActive(true);
        tugboat.StartConnection(false);
        
        _buttons.SetActive(false);
        _lobby.SetActive(false);

        lobbyCam.enabled = false;
        serverSpectateCamera.enabled = false;
    }
}
