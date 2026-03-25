using UnityEngine;
using TMPro;

public class Pistemanageri : MonoBehaviour
{
    // Static tarkoittaa, että tämä luku on sama kaikissa skeneissä
    public static int kokonaisPisteet = 0;
    
    [Header("UI-Elementit")]
    public TMP_Text pisteNaytto;
    public string etuliite = "Pisteet: ";

    void Start()
    {
        PaivitaUI();
    }

    // Tätä metodia kutsutaan minipeleistä: Pistemanageri.LisaaPisteita(10);
    public static void LisaaPisteita(int maara)
    {
        kokonaisPisteet += maara;
        // Etsitään skenessä oleva manageri ja päivitetään sen teksti
        GameObject.FindAnyObjectByType<Pistemanageri>()?.PaivitaUI();
    }

    public void PaivitaUI()
    {
        if (pisteNaytto != null)
        {
            pisteNaytto.text = etuliite + kokonaisPisteet.ToString();
        }
    }

    // Jos haluat tallentaa pisteet selaimen muistiin (WebGL)
    public static void TallennaPisteet()
    {
        PlayerPrefs.SetInt("MuseoPisteet", kokonaisPisteet);
        PlayerPrefs.Save();
    }
}

