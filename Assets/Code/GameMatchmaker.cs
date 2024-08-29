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

    Multipass mp;
    private Lobby _connectedLobby;
    public TextMeshProUGUI _debugText;
    public NetworkManager _networkManager;
    private const string JoinCodeKey = "k";
    public TMP_InputField _playerName;
    private string _playerId;
    public static GameMatchmaker instance;
    public Transport tugboat;
    private void Awake()
    {
        instance = this;
        _lobby.SetActive(true);
        _buttons.SetActive(true);

            if (Application.isBatchMode && !Application.isEditor)
            {
                serverBuild = true;
                Debug.LogWarning("APPLICATION IN SERVER MODE");
            }
            else
            {
                serverBuild = false;
            }


    }
    //public async void CreateOrJoinLobby()
    //{
    //    await Authenticate();
    //    _connectedLobby = await QuickJoinLobby() ?? await CreateLobby();
    //    if (_connectedLobby != null)
    //    {
    //        _buttons.SetActive(false);
    //        _lobby.SetActive(false);
    //    }

    //}
    private void Start()
    {
        if (serverBuild)
        {
            StartServer();
        }
    }
    public void StartServer()
    {
        tugboat.StartConnection(true);
        if(_playerName.text.Length > 0)
        {
            tugboat.StartConnection(false);
            _buttons.SetActive(false);
            _lobby.SetActive(false);
        }



        //await Authenticate();
        //_connectedLobby =  await CreateLobby();
        //Debug.LogWarning("Server CREATED!!!!");
    }

    public void Play()
    {
            tugboat.StartConnection(false);
            _buttons.SetActive(false);
            _lobby.SetActive(false);
    }
//    private async Task Authenticate()
//    {
//        _debugText.text = "Authenticating...";
//        Debug.LogWarning("SErver authenticating Unity Lobby");
//        var options = new InitializationOptions();
//#if UNITY_EDITOR
//        options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
//#endif
//        await UnityServices.InitializeAsync(options);
//        await AuthenticationService.Instance.SignInAnonymouslyAsync();
//        _debugText.text = "Done authenticating. Ready to play";
//        Debug.LogWarning("Server signed into lobby");
//        _playerId = AuthenticationService.Instance.PlayerId;
//    }
//    private async Task<Lobby> QuickJoinLobby()
//    {
//        try
//        {
//            _debugText.text = "Joining server";
//            var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();

//            var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);

//            SetTransportAsClient(a);


//            if (Transport == transportType.fishyUnity)
//            {
//                FishyUnityTransport fishyUnityTransport = mp.GetTransport<FishyUnityTransport>();

//                fishyUnityTransport.StartConnection(false);
//            }
//            else if (Transport == transportType.tugboat)
//            {
//                tugboat.StartConnection(false);
//            }
//            return lobby;
//        }
//        catch (Exception e)
//        {
//            _debugText.text = "No server avaiable to join. Error: " + e.Message;
//            return null;
//        }
//    }
//    private async Task<Lobby> CreateLobby()
//    {
//        try
//        {
//            _debugText.text = "Creating server";
//            Debug.LogWarning("Server creating");
//            const int maxPlayers = 100;

//            var a = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
//            var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

//            var options = new CreateLobbyOptions
//            {
//                Data = new Dictionary<string, DataObject> { { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
//            };
//            var lobby = await Lobbies.Instance.CreateLobbyAsync("Afterblast Lobby", maxPlayers, options);
//            Debug.LogWarning("SERVER CREATEdD \n SERVER JOIN CODE:" + joinCode);
//            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));

//            if(Transport == transportType.fishyUnity)
//            {
//                FishyUnityTransport fishyUnityTransport = mp.GetTransport<FishyUnityTransport>();
//                fishyUnityTransport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);
//                fishyUnityTransport.StartConnection(true);
//                fishyUnityTransport.StartConnection(false);

//            }
               
//            else if(Transport == transportType.tugboat)
//            {
//                tugboat.StartConnection(true);
//            }



//            //_networkManager.ServerManager.StartConnection();
//            //_networkManager.ClientManager.StartConnection();
//            return lobby;
//        }
//        catch (Exception e)
//        {
//            _debugText.text = "Error creating lobby. Error: " + e.Message;
//            Debug.LogWarning("Server error : " + e.Message);
//            return null;
//        }
//    }
//    public void SetTransportAsClient(JoinAllocation a)
//    {
//        _debugText.text = "Setting client data: " + a.ConnectionData;
//        if (Transport == transportType.fishyUnity)
//            mp.GetTransport<FishyUnityTransport>().SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
//    }
//    private static IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
//    {
//        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
//        while (true)
//        {
//            Debug.LogWarning("Server heartbeat  coroutine running");
//            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
//            yield return delay;
//        }
//    }
    //private void OnDestroy()
    //{
    //    try
    //    {
    //        StopAllCoroutines();

    //        if (_connectedLobby != null)
    //        {
    //            if (_connectedLobby.HostId == _playerId)
    //                Lobbies.Instance.DeleteLobbyAsync(_connectedLobby.Id);
    //            else
    //                Lobbies.Instance.RemovePlayerAsync(_connectedLobby.Id, _playerId);
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        print("Error shutting down lobby: " + e.Message);
    //    }
    //}
}
