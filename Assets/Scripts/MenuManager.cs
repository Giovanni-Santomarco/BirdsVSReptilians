using UnityEngine;
using UnityEngine.SceneManagement; 

public class MenuManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void UscireDalGioco()
    {
        Application.Quit();
        Debug.Log("Il giocatore è uscito dal gioco!");
    }
}