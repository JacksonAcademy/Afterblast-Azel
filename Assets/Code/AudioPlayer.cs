using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
public class AudioPlayer : MonoBehaviour
{
    public bool playing;
    public AudioSource source;
    public sound playingSound;
    private Transform parent;

    void Awake()
    {
        parent = transform.parent;
        playing = false;
    }
    public void StopSound()
    {
        playing = false;
        transform.SetParent(parent, false);
        source.Stop();
    }
    public void Play(sound s, Vector3 position, AudioMixerGroup group, Transform parent = null, float volumeMultiplier = 1)
    {
        playingSound = s;
        AudioClip clip = s.soundClips[Random.Range(0, s.soundClips.Count)];
        source.volume = s.volume * volumeMultiplier;
        source.spatialBlend = s.spatialBlend;
        source.minDistance = s.minDistance;
        transform.position = position;
        source.clip = clip;
        source.outputAudioMixerGroup = group;
        source.Play();
        if(parent)
            transform.SetParent(parent, true);
        StartCoroutine(WaitIE(clip.length));
    }
    public IEnumerator WaitIE(float seconds)
    {
        playing = true;
        yield return new WaitForSeconds(seconds);
        playing = false;
        transform.SetParent(parent, false);
    }
}