using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Vivox;
using Unity.Services.Core;
using Unity.Services.Authentication;
using FishNet.Object;
public class VoiceChatManager : NetworkBehaviour
{
    Channel3DProperties channel3DProperties;
    private float _nextPosUpdate;
    public AudioListener listener;
    private bool joined;
    public float audibleDistance = 32;
    public float conversationalDistance = 1;
    public float audioFadeIntensityByDistanceAudio = 1;

    public Vector3 lerpedPos;
    private Vector3 listenerPos;
    public override void OnStartClient()
    {
        LoginToVivoxAsync();
    }
    public async void LoginToVivoxAsync()
    {
        joined = false;
        print("Trying to log into vivox");
        LoginOptions options = new LoginOptions();
        options.DisplayName = GameMatchmaker.instance._playerName.text;
        options.EnableTTS = true;
        await VivoxService.Instance.LoginAsync(options);
        print("Logged into Vivox");
        string channelToJoin = "Game";

        Channel3DProperties coptions = new Channel3DProperties((int)audibleDistance, (int)conversationalDistance, audioFadeIntensityByDistanceAudio, AudioFadeModel.ExponentialByDistance);
        await VivoxService.Instance.JoinPositionalChannelAsync(channelToJoin, ChatCapability.TextAndAudio, channel3DProperties);
        print("Joined voice chat");
        joined = true;
    }
    public override void OnStopClient()
    {
        print("Logging out of Vivox");
        base.OnStopClient();
        //LeaveVoiceChat();
        //LogoutOfVivoxAsync();
    }
    public async void LeaveVoiceChat()
    {
        await VivoxService.Instance.LeaveAllChannelsAsync();
    }
    public async void LogoutOfVivoxAsync()
    {
        await VivoxService.Instance.LogoutAsync();
    }
    void Update()
    {
        if (!joined)
            return;

        VivoxService.Instance.Set3DPosition(transform.position, listener.transform.position, listener.transform.eulerAngles, listener.transform.up, "Game", true);
    }
}
