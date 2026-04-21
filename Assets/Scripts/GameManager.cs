using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverMenuUI;
    public LevelManager levelManager;
    public Animator transitionAnimator;
    private float transitionTime = 2f;
    private int level = 1;
    private bool isAnyEnemyInCurrentLevel = true;
    private bool isPlayerDead = false;

    void Start()
    {
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
        //transitionAnimator.SetTrigger("StartTransition");

        // 2. Wait for the animation to completely finish
        // We use WaitForSecondsRealtime so it works even if you paused the game (Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(transitionTime);

        // 3. Perform the heavy lifting while the screen is covered
        //SceneManager.LoadScene("MainMenu");
        gameOverMenuUI.SetActive(true);
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
        SceneManager.LoadScene("MainMenu");
    }
}