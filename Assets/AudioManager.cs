using UnityEngine;
using UnityEngine.Audio;


//tutti gli audio sfx saranno creati da questa classe: nel momento in cui ci serve istanziare un audio, questa classe crea
//un gameObject della durata della risorsa audio (alla fine della sua durata verrà distrutto)
//questo ci permette di non avere bug come avere nemici che nel momento in cui muoiono interrompono i loro suoni bruscamente
//idem per il player quando muore o quando cambia arma prima che una risorsa audio finisca
public class AudioManager : MonoBehaviour
{
    // Creiamo un "Singleton". Questo ci permette di chiamare l'AudioManager 
    // da QUALSIASI script del gioco semplicemente scrivendo AudioManager.instance
    public static AudioManager instance;
    public AudioMixer mainMixer;

    void Awake()
    {
        // Ci assicuriamo che esista un solo AudioManager in tutta la scena
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Questa è la funzione magica che chiameremo dalle nostre armi
    public void PlaySFX(AudioClip clip, Vector3 position, bool useRandomPitch = false, float volume = 1f)
    {
        if (clip == null) return;

        // Creiamo un GameObject vuoto invisibile dedicato solo a questo suono
        GameObject sfxObject = new GameObject("SFX_" + clip.name);

        sfxObject.transform.position = position;

        // Gli attacchiamo un altoparlante (AudioSource)
        AudioSource audioSource = sfxObject.AddComponent<AudioSource>();

        // Lo configuriamo
        audioSource.clip = clip;

        float currentSfxAudioVolumeDbNotation;
        mainMixer.GetFloat("SfxVol", out currentSfxAudioVolumeDbNotation);
        // currentSfxAudioVolume stores a value in DB notation (-80 - 0) but audioSource.play()
        // expects a 0-1 notation
        float currentSfxAudioVolume0to1Notation = Mathf.Pow(10f, currentSfxAudioVolumeDbNotation / 20f);
        float currentSfxAudioVolumeNormalized = currentSfxAudioVolume0to1Notation * volume;
        audioSource.volume = currentSfxAudioVolumeNormalized;

        //Trasforma il suono da "piatto" a "direzionale"
        audioSource.spatialBlend = 1f; // 0 significa 2D, 1 significa 100% 3D!

        //Regola come il suono sfuma con la distanza
        audioSource.rolloffMode = AudioRolloffMode.Linear; // Sfuma gradualmente
        audioSource.minDistance = 2f;  // Fino a 2 metri lo senti al massimo
        audioSource.maxDistance = 100f; // A 100 metri non lo senti più

        if (useRandomPitch)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
        }

        // Facciamo partire il suono!
        audioSource.Play();

        // Diciamo a Unity di distruggere questo oggetto
        // ESATTAMENTE quando la durata della clip audio è terminata!
        Destroy(sfxObject, clip.length);
    }
}