using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public AudioMixer mainMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        // Convert 0-1 slider value to -80 to 0 decibels
        mainMixer.SetFloat("MusicVol", Mathf.Log10(volume) * 20);
    }

    public void SetSFXVolume()
    {
        float volume = sfxSlider.value;
        mainMixer.SetFloat("SfxVol", Mathf.Log10(volume) * 20);
    }
}