using UnityEngine;
using System.Collections.Generic;

public class KaikkiYhdessaMaster : MonoBehaviour
{
    public static KaikkiYhdessaMaster Instance;

    private AudioSource musiikkiSource;
    private AudioSource sfxSource;

    [Header("--- KIRJASTOT ---")]
    public List<AudioClip> musiikkiLista; 
    public List<AudioClip> klikkausAanet; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            musiikkiSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();
            
            musiikkiSource.loop = false; // MUUTOS: loop false, jotta voimme vaihtaa biisiä
            musiikkiSource.playOnAwake = false;
            sfxSource.playOnAwake = false;
        }
        else 
        { 
            Destroy(gameObject); 
        }
    }

    void Start()
    {
        float tallennettuVol = PlayerPrefs.GetFloat("MasterVol", 0.5f);
        PaivitaVolumet(tallennettuVol);

        // Aloitetaan musiikki shuffle-logiikalla
        if (musiikkiLista.Count > 0 && !musiikkiSource.isPlaying) 
        {
            SoitaSeuraavaMusiikki();
        }
    }

    void Update()
    {
        // Tarkistetaan onko biisi loppunut, jos on, soitetaan seuraava (Shuffle)
        if (!musiikkiSource.isPlaying && musiikkiLista.Count > 0)
        {
            SoitaSeuraavaMusiikki();
        }
    }

    // --- TÄMÄ ON SE SHUFFLE-LISÄYS ---
    public void SoitaSeuraavaMusiikki()
    {
        if (musiikkiLista.Count == 0) return;

        // Valitaan satunnainen biisi listasta
        AudioClip uusiBiisi = musiikkiLista[Random.Range(0, musiikkiLista.Count)];
        
        musiikkiSource.clip = uusiBiisi;
        musiikkiSource.Play();
        
        Debug.Log("Shuffle: Nyt soittaa: " + uusiBiisi.name);
    }

    public void MuutaMasteria(float arvo) 
    { 
        PaivitaVolumet(arvo);
        PlayerPrefs.SetFloat("MasterVol", arvo); 
    }

    void PaivitaVolumet(float arvo)
    {
        if (musiikkiSource) musiikkiSource.volume = arvo;
        if (sfxSource) sfxSource.volume = arvo;
    }

    public void SoitaKlikkaus() 
    {
        if (sfxSource != null && klikkausAanet.Count > 0)
        {
            AudioClip klippi = klikkausAanet[Random.Range(0, klikkausAanet.Count)];
            sfxSource.PlayOneShot(klippi);
            Debug.Log("SFX Soitettu: " + klippi.name);
        }
    }
}

