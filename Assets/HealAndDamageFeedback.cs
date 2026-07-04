using System.Collections;
using UnityEngine;

public class HealAndDamageFeedback : MonoBehaviour
{
    // Usiamo un ARRAY (una lista) per contenere tutti i pezzi del corpo
    private SpriteRenderer[] spriteRenderers;
    private Color[] coloriOriginali;

    [Header("settings for hit feedback")]
    public Color damageColor = Color.red;
    public float feedbackDurationForHit = 0.1f;

    [Header("settings for heal feedback")]
    public Color healColor = Color.green;
    public float feedbackDurationForHeal = 0.2f;



    void Start()
    {
        // GetComponentsInChildren trova tutti gli SpriteRenderer nell'oggetto e nei suoi figli!
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        // Prepariamo una lista per salvare i colori originali di ogni singolo pezzo
        coloriOriginali = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            coloriOriginali[i] = spriteRenderers[i].color;
        }
    }

    public void StartHitAnimation()
    {
        StartCoroutine(FlashRoutine(damageColor, feedbackDurationForHit));
    }

    
    public void StartHealAnimation()
    {
        StartCoroutine(FlashRoutine(healColor, feedbackDurationForHeal));
    }


    private IEnumerator FlashRoutine(Color color, float duration)
    {
        SpriteRenderer[] currentSprites = GetComponentsInChildren<SpriteRenderer>();

        // PASSO 1: Colora tutti i pezzi
        foreach (SpriteRenderer sr in currentSprites)
        {
            if (sr != null)
            {
                sr.color = color;
            }
        }

        // PASSO 2: Aspetta
        yield return new WaitForSeconds(duration);

        // PASSO 3: Rimetti a ogni pezzo il suo colore originale
        foreach (SpriteRenderer sr in currentSprites)
        {
            if (sr != null)
            {
                sr.color = Color.white;
            }
        }
    }
}
