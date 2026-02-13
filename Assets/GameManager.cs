using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BoardManager boardScript;
    private int level = 1;

    void Start()
    {
        InitGame();
    }

    void InitGame()
    {
        boardScript.GenerateLevel(level);
    }

    /*
    void Update()
    {
        if (!boardScript.isEnemies())
        {
            level++;
            Invoke("InitGame", 2f); // 2 second delay for transition
        }

    }
    */
}