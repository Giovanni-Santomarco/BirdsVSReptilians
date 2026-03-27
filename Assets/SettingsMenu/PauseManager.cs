using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public AudioMixer mainMixer;
    public GameObject pauseMenuUI;
    public Slider musicSlider;
    public Slider sfxSlider;

    private bool isPaused = false;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Resumes game physics and timers
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Freezes the game
        isPaused = true;
    }

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

    public void onExitButton()
    {
        // the following two lines will allow the gamer to play again
        Time.timeScale = 1f; 
        isPaused = false;

        SceneManager.LoadScene("MainMenu");
    }
}