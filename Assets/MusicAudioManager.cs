using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class MusicAudioManager : MonoBehaviour
{
    public AudioClip[] playlist;

    private AudioSource audioSource;

    //MUSIC GROUP OF THE MAIN MIXER
    public AudioMixerGroup mixerGroup;

    private int currentTrackIndex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
            Debug.LogWarning("GameMusicPlaylist: No Audio Mixer Group assigned!");
        }

        // 2. Make sure we don't have the single-loop setting enabled
        audioSource.loop = false;

        // 3. Start the playlist coroutine if we have songs
        if (playlist != null && playlist.Length > 0)
        {
            StartCoroutine(PlayThroughPlaylist());
        }
        else
        {
            Debug.LogWarning("GameMusicPlaylist: Playlist is empty!");
        }
    }

    private IEnumerator PlayThroughPlaylist()
    {
        while (true) // Keep the playlist running infinitely
        {
            // Set the current track and play it
            audioSource.clip = playlist[currentTrackIndex];
            audioSource.Play();

            // Wait one frame to ensure audioSource.isPlaying registers as true
            yield return null;

            // Pause the coroutine until the current song finishes playing
            yield return new WaitWhile(() => audioSource.isPlaying);

            // Move to the next track. 
            // The modulo operator (%) loops the index back to 0 when it reaches the end of the array.
            currentTrackIndex = (currentTrackIndex + 1) % playlist.Length;
        }
    }

    public void SkipTrack()
    {
        //since we have PlayThroughPlaylist() that makes the next song start as soon as the last ends
        //we can force stop a song to basically skip it and go to the next
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}
