using System.Collections;
using UnityEngine;

public class DamageFeedback : MonoBehaviour
{
    // Usiamo un ARRAY (una lista) per contenere tutti i pezzi del corpo
    private SpriteRenderer[] spriteRenderers;
    private Color[] coloriOriginali;

    [Header("Impostazioni Lampeggio")]
    public Color coloreDanno = Color.red;
    public float durataLampeggio = 0.1f;

    void Start()
    {
        // GetComponentsInChildren trova tutti gli SpriteRenderer nell'oggetto e nei suoi figli
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
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // coloro i pezzi del corpo del colore che uso per indicare che si è subito del danno
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].color = coloreDanno;
        }

        
        yield return new WaitForSeconds(durataLampeggio);

        // rimetto il colore originale
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].color = coloriOriginali[i];
        }
    }
}