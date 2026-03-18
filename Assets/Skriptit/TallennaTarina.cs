using System.IO;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TarinaManager : MonoBehaviour
{
    [Header("UI Paneelit")]
    public GameObject syoteKentta;      // Hierarchy: ViestiKenttä
    public GameObject kiitosViesti;     // Hierarchy: Kiitosviesti
    public GameObject scrollPaneeli;    // Hierarchy: Scroll View
    
    [Header("Napit")]
    public GameObject tallennaNappi;    // Se alkuperäinen Tallenna-nappi
    public GameObject kirjoitaNappi;     // Uusi "Kirjoita uusi" -nappi

    [Header("Tekstikomponentit")]
    public TMP_InputField syoteInputField;
    public TMP_Text scrollTeksti;

    private string jsonPolku;
    private List<string> tarinat = new List<string>();

    void Start()
    {
        jsonPolku = Path.Combine(Application.streamingAssetsPath, "tarinat.json");
        LataaTarinatMuistiin();

        // Alkutila: Syötekenttä ja Tallenna-nappi näkyvissä
        AktivoiKirjoitustila();
    }

    // Metodi "Kirjoita"-napille
    public void AktivoiKirjoitustila()
    {
        NaytaPaneeli(syoteKentta);
        
        // Vaihdetaan napit
        tallennaNappi.SetActive(true);
        kirjoitaNappi.SetActive(false);
        
        // Tyhjennetään kenttä valmiiksi uutta tarinaa varten
        syoteInputField.text = "";
    }

    public void TallennaTarina()
    {
        if (string.IsNullOrWhiteSpace(syoteInputField.text)) return;

        tarinat.Add(syoteInputField.text);
        TallennaTarinatJSON();

        // Näytetään kiitosviesti
        NaytaPaneeli(kiitosViesti);

        // Vaihdetaan Tallenna -> Kirjoita
        tallennaNappi.SetActive(false);
        kirjoitaNappi.SetActive(true);
    }

    public void AvaaSelaa()
    {
        NaytaPaneeli(scrollPaneeli);
        PaivitaScrollTeksti();

        // Varmistetaan, että selaustilassa näkyy "Kirjoita"-nappi, jotta pääsee takaisin
        tallennaNappi.SetActive(false);
        kirjoitaNappi.SetActive(true);
    }

    // --- APUMETODIT ---

    private void NaytaPaneeli(GameObject aktiivinenPaneeli)
    {
        syoteKentta.SetActive(syoteKentta == aktiivinenPaneeli);
        kiitosViesti.SetActive(kiitosViesti == aktiivinenPaneeli);
        scrollPaneeli.SetActive(scrollPaneeli == aktiivinenPaneeli);
    }

    private void LataaTarinatMuistiin()
    {
        if (File.Exists(jsonPolku))
        {
            string json = File.ReadAllText(jsonPolku);
            ListWrapper wrapper = JsonUtility.FromJson<ListWrapper>(json);
            if (wrapper != null && wrapper.tarinat != null) tarinat = wrapper.tarinat;
        }
    }

    private void TallennaTarinatJSON()
    {
        string json = JsonUtility.ToJson(new ListWrapper { tarinat = tarinat }, true);
        File.WriteAllText(jsonPolku, json);
    }

    private void PaivitaScrollTeksti()
    {
        if (scrollTeksti == null) return;
        string teksti = "";
        for (int i = 0; i < tarinat.Count; i++)
        {
            teksti += "--- Tarina " + (i + 1) + " ---\n" + tarinat[i] + "\n\n";
        }
        scrollTeksti.text = teksti;
    }

    [System.Serializable]
    public class ListWrapper { public List<string> tarinat; }
}

