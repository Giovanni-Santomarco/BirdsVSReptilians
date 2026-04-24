using UnityEngine;
using UnityEngine.Audio; // Required for Audio Mixer classes

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public AudioClip menuSong;

    //public AudioMixer mainMixer;

    public AudioMixerGroup mixerGroup;

    //private readonly string volumeParameter = "MusicVol";
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // 1. Route the audio to the mixer group
        if (mixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = mixerGroup;
        }
        else
        {
            Debug.LogWarning("MenuMusicController: No Audio Mixer Group assigned!");
        }

        // 2. Setup and play the song
        if (menuSong != null)
        {
            audioSource.clip = menuSong;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("MenuMusicController: No song assigned!");
        }
    }

    /// <summary>
    /// Call this method from a UI Slider's "On Value Changed" event.
    /// The slider should be set to Min Value: 0.0001 and Max Value: 1.
    /// </summary>
    ///
    /*
    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        // Convert 0-1 slider value to -80 to 0 decibels
        mainMixer.SetFloat("MusicVol", Mathf.Log10(volume) * 20);
    }
    */
}