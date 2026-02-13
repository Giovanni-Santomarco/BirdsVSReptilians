using UnityEngine;
using UnityEngine.InputSystem.Android;

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
        counter = 1000;
        boardScript.GenerateLevel(level); //Generate level includes cleaning of prev level
    }
    private int counter = 1000;
    void Update()
    {
        if (counter<=0)
        {
            level++;
            Invoke("InitGame", 0f); // 2 second delay for transition
        }
        else
        {
            counter--;
        }
    }
}