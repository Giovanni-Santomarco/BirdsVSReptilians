using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Impostazioni Cursore")]
    public Texture2D crosshairTexture;

    public GameObject gameOverMenuUI;
    public LevelManager levelManager;
    public Animator transitionAnimator;
    private float transitionTime = 2f;
    private int level = 1;
    private bool isAnyEnemyInCurrentLevel = true;
    private bool isPlayerDead = false;

    void Start()
    {
        SetCustomCursor();
        InitGame();
    }

    void InitGame()
    {
        levelManager.GenerateLevel(level);
    }
    IEnumerator InitGame1()
    {
        // Cover the screen
        transitionAnimator.SetTrigger("StartTransition");

        // Wait for the animation to completely finish
        // We use WaitForSecondsRealtime so it works even if you paused the game (Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(transitionTime);

        // 3. Perform the heavy lifting while the screen is covered
        levelManager.GenerateLevel(level);

        // Add a tiny extra wait time here 
        // to stay covered for a split second after generating.
        yield return new WaitForSecondsRealtime(0.2f);

        // Reveal the screen
        transitionAnimator.SetTrigger("EndTransition");
    }

    IEnumerator GameOver()
    {
        // Cover the screen
        transitionAnimator.SetTrigger("StartTransition");

        // 2. Wait for the animation to completely finish
        // We use WaitForSecondsRealtime so it works even if you paused the game (Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(transitionTime);

        // 3. Perform the heavy lifting while the screen is covered
        //SceneManager.LoadScene("MainMenu");
        gameOverMenuUI.SetActive(true);
        transitionAnimator.SetTrigger("EndTransition");
        yield return new WaitForSecondsRealtime(transitionTime/4);
        Time.timeScale = 0f;
    }
    void Update()
    {
        // level completed, animation + level switch
        if (!isAnyEnemyInCurrentLevel) 
        {
            level++;
            isAnyEnemyInCurrentLevel=true;
            StartCoroutine(InitGame1());
        }
        // game over, animation + main menu
        if (isPlayerDead) // run 
        {
            level = 1;
            isPlayerDead = false;
            StartCoroutine(GameOver());
        }
    }

    public void noEnemiesInCurrentLevel()
    {
        isAnyEnemyInCurrentLevel = false;
    }

    internal void playerIsDead()
    {
        isPlayerDead = true;
    }

    public void onExitButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public int GetCurrentLevel()
    {
        return level;
    }

    void SetCustomCursor()
    {
        if (crosshairTexture != null)
        {
            // Calcoliamo il centro dell'immagine cursore.
            Vector2 hotspot = new Vector2(crosshairTexture.width / 2f, crosshairTexture.height / 2f);

            Cursor.SetCursor(crosshairTexture, hotspot, CursorMode.Auto);
        }
    }
}