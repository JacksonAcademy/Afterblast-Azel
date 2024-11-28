using Kamgam.UGUIComponentsForSettings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public SliderUGUI masterVolume, effectVolume, voiceChatVolume;

    public AudioMixer group;
    private void Start()
    {
        SFXVolume(PlayerPrefs.GetFloat("SfxVolume"));
        effectVolume.Value = PlayerPrefs.GetFloat("SfxVolume");

        MainVolume(PlayerPrefs.GetFloat("MasterVolume"));
        masterVolume.Value = PlayerPrefs.GetFloat("MasterVolume");

        VoiceChatVolume(PlayerPrefs.GetFloat("VoiceChatVolume"));
        voiceChatVolume.Value = PlayerPrefs.GetFloat("VoiceChatVolume");
    }
    public void SFXVolume(float value)
    {
        float correctVolume = Mathf.Log10(value) * 20;
        group.SetFloat("SfxVolume", correctVolume);
        PlayerPrefs.SetFloat("SfxVolume", correctVolume);
        PlayerPrefs.Save();
    }
    public void MainVolume(float value)
    {
        float correctVolume = Mathf.Log10(value) * 20;
        group.SetFloat("MasterVolume", correctVolume);
        PlayerPrefs.SetFloat("MasterVolume", correctVolume);
        PlayerPrefs.Save();
    }
    public void VoiceChatVolume(float value)
    {
        float correctVolume = Mathf.Log10(value) * 20;
        group.SetFloat("VoiceChatVolume", correctVolume);
        PlayerPrefs.SetFloat("VoiceChatVolume", correctVolume);
        PlayerPrefs.Save();
    }

}
