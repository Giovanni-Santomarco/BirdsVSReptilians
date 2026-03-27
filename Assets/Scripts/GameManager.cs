using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public BoardManager boardScript;
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
        boardScript.GenerateLevel(level);
    }
    IEnumerator InitGame1()
    {
        // 1. Play the first animation (Cover the screen)
        transitionAnimator.SetTrigger("StartTransition");

        // 2. Wait for the animation to completely finish
        // We use WaitForSecondsRealtime so it works even if you paused the game (Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(transitionTime);

        // 3. Perform the heavy lifting while the screen is covered
        boardScript.GenerateLevel(level);

        // Optional: Add a tiny extra wait time here if you want the screen 
        // to stay covered for a split second after generating, just for pacing.
        yield return new WaitForSecondsRealtime(0.2f);

        // 4. Play the second animation (Reveal the screen)
        transitionAnimator.SetTrigger("EndTransition");
    }

    IEnumerator GameOver()
    {
        // 1. Play the first animation (Cover the screen)
        transitionAnimator.SetTrigger("StartTransition");

        // 2. Wait for the animation to completely finish
        // We use WaitForSecondsRealtime so it works even if you paused the game (Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(transitionTime);

        // 3. Perform the heavy lifting while the screen is covered
        SceneManager.LoadScene("MainMenu");
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
}