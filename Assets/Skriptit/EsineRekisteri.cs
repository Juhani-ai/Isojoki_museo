using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Tarvitaan helppoon listatarkistukseen

public class EsineRekisteri : MonoBehaviour
{
    // Kaikki esineet
    public static string[] kaikkiEsineet = { 
        "Kahvihuhmar", "Kappa", "Kauha", "Koristelupuu", 
        "Leikkihevonen", "Leili", "Luotipuntari", "Pahkakuppi", 
        "Pytty", "Sahtikulho", "Survin", "Taskoin", "Voiaski" 
    };

    // Lista esineistä, jotka ovat AINA auki (viisi ensimmäistä)
    private static string[] ainaAuki = { 
        "Kahvihuhmar", "Kappa", "Kauha", "Koristelupuu", "Leikkihevonen" 
    };

    public static List<string> HaeAvatutEsineet()
    {
        List<string> avatut = new List<string>();

        foreach (string id in kaikkiEsineet)
        {
            // 1. Jos esine kuuluu "ainaAuki"-listaan, se lisätään suoraan
            if (System.Array.Exists(ainaAuki, element => element == id))
            {
                avatut.Add(id);
            }
            // 2. Muussa tapauksessa tarkistetaan onko se ostettu
            else if (PlayerPrefs.GetInt("Unlocked_" + id, 0) == 1)
            {
                avatut.Add(id);
            }
        }
        return avatut;
    }

    public static bool OnkoTarpeeksiEsineita(int tarvittavaMaara)
    {
        return HaeAvatutEsineet().Count >= tarvittavaMaara;
    }
}

