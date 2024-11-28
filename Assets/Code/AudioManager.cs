using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : NetworkBehaviour
{
    public static AudioManager instance;
    public List<AudioPlayer> players;
    public Transform playerParent;
    public int playerCount;
    public AudioPlayer player;
    public AudioMixerGroup sfxGroup;

    private void Awake()
    {
        instance = this;
        if(players.Count == 0)
        {
            for (int i = 0; i < playerCount; i++)
            {
                AudioPlayer tempPlayer = Instantiate(player, playerParent);
                players.Add(tempPlayer);
                tempPlayer.source.outputAudioMixerGroup = sfxGroup;
            }
        }

    }
    public void RetrieveSound(string path)
    {

    }
    //public void PlaySound(sound s, Vector3 pos)
    //{
    //    PlaySoundLocal(s, pos);
    //    PlaySoundServer(s, pos);
    //}
    //[ServerRpc]
    //private void PlaySoundServer(sound s, Vector3 pos)
    //{
    //    PlaySoundObservers(s, pos);
    //}
    //[ObserversRpc(ExcludeOwner = true)]
    //private void PlaySoundObservers(sound s, Vector3 pos)
    //{
    //    PlaySoundLocal(s, pos);
    //}
    public void PlaySoundLocal(sound s, Vector3 pos, Transform parent = null, bool overAllClients = true, float volumeMultiplier = 1)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (!players[i].playing)
            {
                print("Playing sound:: " + s.name);
                players[i].Play(s, pos, sfxGroup, parent, volumeMultiplier);
                break;
            }
        }
    }
    public void StopSound(sound s)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playing)
            {
                if (players[i].playingSound == s)
                {
                    players[i].StopSound();
                    break;
                }
            }
        }
    }
}
