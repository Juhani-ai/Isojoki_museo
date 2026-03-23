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
            
            // Luodaan omat sourcet koodilla, niin ne eivät katoa
            musiikkiSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();
            
            musiikkiSource.loop = true;
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
        // Haetaan tallennettu volume
        float tallennettuVol = PlayerPrefs.GetFloat("MasterVol", 0.5f);
        PaivitaVolumet(tallennettuVol);

        // Aloitetaan musiikki vain kerran (pelin alussa)
        if (musiikkiLista.Count > 0 && !musiikkiSource.isPlaying) 
        {
            musiikkiSource.clip = musiikkiLista[0];
            musiikkiSource.Play();
        }
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
        // Varmistetaan, että SFX-source on olemassa ja siinä on ääniä
        if (sfxSource != null && klikkausAanet.Count > 0)
        {
            AudioClip klippi = klikkausAanet[Random.Range(0, klikkausAanet.Count)];
            
            // TÄMÄ ON SE KORJAUS: PlayOneShot on paras efekteille
            sfxSource.PlayOneShot(klippi);
            
            // Debug-logi (voit poistaa tämän kun toimii)
            Debug.Log("SFX Soitettu: " + klippi.name);
        }
    }
}

