using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;


[CreateAssetMenu(fileName = "New Sound Object", menuName = "Audio/Sound", order = 1)]
public class sound : ScriptableObject
{
    [Header("Sound Properties")]
    public string soundName;
    public List<AudioClip> soundClips;
    public AudioMixerGroup group;
    [Range(0, 1)]
    public float volume, spatialBlend;
    [Range(0, 500)] public float minDistance;

    public void Play(Vector3 position, float pitch = 1, Transform parent = null, float volumeMultiplier = 1)
    {
        if (soundClips.Count > 0)
        {
            //AudioManager.instance.PlaySound(this, position);
            AudioManager.instance.PlaySoundLocal(this, position, parent, true, volumeMultiplier);
        }
    }
    public void Stop()
    {
        if (soundClips.Count > 0)
        {
            AudioManager.instance.StopSound(this);
        }
    }
}
