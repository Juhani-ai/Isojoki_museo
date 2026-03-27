using UnityEngine;
using TMPro;

public class Pistemanageri : MonoBehaviour
{
    public static int kokonaisPisteet = 0;
    
    [Header("UI-Elementit")]
    public TMP_Text pisteNaytto;
    public string etuliite = "Pisteet: ";

    [Header("Ääniasetukset")]
    public AudioSource audioSource;
    public AudioClip kolikkoAani;

    void Start()
    {
        // Jos unohdit vetää AudioSourcen, yritetään etsiä se samasta objektista
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        PaivitaUI();
    }

    public static void LisaaPisteita(int maara)
    {
        kokonaisPisteet += maara;
        
        // Etsitään skenessä oleva manageri
        Pistemanageri manageri = Object.FindFirstObjectByType<Pistemanageri>();
        if (manageri != null)
        {
            manageri.PaivitaUI();
            manageri.SoitaPisteAani();
        }
    }

    public void PaivitaUI()
    {
        if (pisteNaytto != null)
        {
            pisteNaytto.text = etuliite + kokonaisPisteet.ToString();
        }
    }

    public void SoitaPisteAani()
    {
        if (audioSource != null && kolikkoAani != null)
        {
            // PlayOneShot on paras pisteäänille, ne voivat soida päällekkäin
            audioSource.PlayOneShot(kolikkoAani);
        }
    }

    public static void TallennaPisteet()
    {
        PlayerPrefs.SetInt("MuseoPisteet", kokonaisPisteet);
        PlayerPrefs.Save();
    }
}

