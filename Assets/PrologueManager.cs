using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PrologueManager : MonoBehaviour
{
    [Header("Componenti UI")]
    public Image storyImage;
    public TextMeshProUGUI storyText;

    [Header("Contenuti del Prologo")]
    public Sprite[] prologueImages;
    [TextArea(3, 5)]
    public string[] prologueTexts;

    [Header("Audio")]
    public AudioSource musicSource;       // Lo speaker per la musica
    //public AudioSource voiceSource;       // Lo speaker per la voce
    public AudioClip backgroundMusic;     // La canzone di sottofondo
    //public AudioClip[] voiceClips;        // I 4 file audio della voce

    [Header("Impostazioni Tempi")]
    public float fadeDuration = 1.5f;  
    public float displayDuration = 5f;  

    [Header("Prossima Scena")]
    public string nextSceneName = "MainMenu"; 

    void Start()
    {
        //immagine e testo invisibili all'avvio
        SetAlpha(storyImage, 0f);
        SetAlphaText(storyText, 0f);

        //musica di sottofondo
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }

        //faccio partire il prologo
        StartCoroutine(PlayPrologue());
    }

    void Update()
    {
        // Se il giocatore preme Spazio, Invio o Esc, salta il prologo!
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    IEnumerator PlayPrologue()
    {
        for (int i = 0; i < prologueImages.Length; i++)
        {
            storyImage.sprite = prologueImages[i];

            if (i < prologueTexts.Length)
                storyText.text = prologueTexts[i];
            else
                storyText.text = "";

            //Dissolvenza in entrata
            yield return StartCoroutine(Fade(0f, 1f));

            //Aspettiamo che il giocatore legga
            yield return new WaitForSeconds(displayDuration);

            //Dissolvenza in uscita 
            yield return StartCoroutine(Fade(1f, 0f));

            //Pausa piccolissima tra una schermata e l'altra a schermo nero
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(2f);

        //carichiamo la scena successiva alla fine del prologo
        SceneManager.LoadScene(nextSceneName);
    }

    //gestisce la matematica della sfumatura
    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // Calcoliamo a che punto della sfumatura siamo
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);

            SetAlpha(storyImage, currentAlpha);
            SetAlphaText(storyText, currentAlpha);

            yield return null; // Aspetta il prossimo frame
        }

        // Assicuriamoci che alla fine l'alpha sia esattamente il valore bersaglio
        SetAlpha(storyImage, endAlpha);
        SetAlphaText(storyText, endAlpha);
    }

    // Funzioni di comodità per cambiare l'alpha (trasparenza) senza toccare i colori
    void SetAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    void SetAlphaText(TextMeshProUGUI txt, float alpha)
    {
        Color c = txt.color;
        c.a = alpha;
        txt.color = c;
    }
}