using FishNet.Managing;
using FishNet.Transporting.UTP;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using UnityEngine;
using FishNet.Transporting;
#if UNITY_EDITOR
using ParrelSync;
#endif

public class RelayMatchmaker : MonoBehaviour
{

    [SerializeField] private GameObject _buttons;

    private Lobby _connectedLobby;
    public TextMeshProUGUI _debugText;
    private QueryResponse _lobbies;
    public NetworkManager _networkManager;
    private FishyUnityTransport _transport;
    private const string JoinCodeKey = "k";
    private string _playerId;
    private void Awake()
    {

        _transport = _networkManager.TransportManager.GetTransport<FishyUnityTransport>();
    }
    public async void CreateOrJoinLobby()
    {
        await Authenticate();
        _connectedLobby = await QuickJoinLobby() ?? await CreateLobby();
        if (_connectedLobby != null)
            _buttons.SetActive(false);
    }
    private async Task Authenticate()
    {
        _debugText.text = "Authenticating...";
        var options = new InitializationOptions();
        #if UNITY_EDITOR
             options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
        #endif

        await UnityServices.InitializeAsync(options);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        _debugText.text = "Done authenticating. Ready to play";
        _playerId = AuthenticationService.Instance.PlayerId;
    }
    private async Task<Lobby> QuickJoinLobby()
    {
        try
        {
            _debugText.text = "Joining server";
            var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();

            var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);

            SetTransportAsClient(a);

            _networkManager.ClientManager.StartConnection();
            return lobby;
        }
        catch(Exception e)
        {
            _debugText.text = "No server avaiable to join. Error: " + e.Message;
            return null;
        }
    }
    private async Task<Lobby> CreateLobby()
    {
        try
        {
            _debugText.text = "Creating server";
            const int maxPlayers = 100;

            var a = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> { { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
            };
            var lobby = await Lobbies.Instance.CreateLobbyAsync("Afterblast Lobby", maxPlayers, options);

            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));

            _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

            _networkManager.ServerManager.StartConnection();
            _networkManager.ClientManager.StartConnection();
            return lobby;
        }
        catch (Exception e)
        {
            _debugText.text = "Error creating lobby. Error: " + e.Message;
            return null;
        }
    }
    public void SetTransportAsClient(JoinAllocation a)
    {
        _debugText.text = "Setting client data: " + a.ConnectionData;
        _transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
    }
    private static IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
    private void OnDestroy()
    {
        try
        {
            StopAllCoroutines();

            if(_connectedLobby != null)
            {
                if (_connectedLobby.HostId == _playerId)
                    Lobbies.Instance.DeleteLobbyAsync(_connectedLobby.Id);
                else
                    Lobbies.Instance.RemovePlayerAsync(_connectedLobby.Id, _playerId);
            }
        }
        catch(Exception e)
        {
            print("Error shutting down lobby: " + e.Message);
        }
    }
}
