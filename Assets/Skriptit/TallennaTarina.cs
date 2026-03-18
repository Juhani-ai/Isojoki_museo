using System.IO;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System; // Tarvitaan DateTimea varten

public class TarinaManager : MonoBehaviour
{
    [Header("UI Paneelit")]
    public GameObject syoteKentta;
    public GameObject kiitosViesti;
    public GameObject scrollPaneeli;
    
    [Header("Napit")]
    public GameObject tallennaNappi;
    public GameObject kirjoitaNappi;

    [Header("Tekstikomponentit")]
    public TMP_InputField syoteInputField;
    public TMP_Text scrollTeksti;

    private string jsonPolku;
    
    // Muutetaan lista käyttämään TarinaData-luokkaa pelkän stringin sijaan
    private List<TarinaData> tarinat = new List<TarinaData>();

    void Start()
    {
        jsonPolku = Path.Combine(Application.streamingAssetsPath, "tarinat.json");
        LataaTarinatMuistiin();
        AktivoiKirjoitustila();
    }

    public void AktivoiKirjoitustila()
    {
        NaytaPaneeli(syoteKentta);
        tallennaNappi.SetActive(true);
        kirjoitaNappi.SetActive(false);
        syoteInputField.text = "";
    }

    public void TallennaTarina()
    {
        if (string.IsNullOrWhiteSpace(syoteInputField.text)) return;

        // Luodaan uusi tarina-olio päivämäärällä
        TarinaData uusiTarina = new TarinaData();
        uusiTarina.teksti = syoteInputField.text;
        // Tallennetaan päivämäärä muodossa: 18.3.2026 klo 10:30
        uusiTarina.pvm = DateTime.Now.ToString("d.M.yyyy 'klo' HH:mm");

        tarinat.Add(uusiTarina);
        TallennaTarinatJSON();

        NaytaPaneeli(kiitosViesti);
        tallennaNappi.SetActive(false);
        kirjoitaNappi.SetActive(true);
    }

    public void AvaaSelaa()
    {
        NaytaPaneeli(scrollPaneeli);
        PaivitaScrollTeksti();
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
        string kooste = "";
        
        // Käydään lista läpi lopusta alkuun, jotta uusin tarina on ylimpänä!
        for (int i = tarinat.Count - 1; i >= 0; i--)
        {
            kooste += "<color=#888888><size=80%>" + tarinat[i].pvm + "</size></color>\n";
            kooste += tarinat[i].teksti + "\n\n";
            kooste += "________________________\n\n";
        }
        scrollTeksti.text = kooste;
    }

    // Luokka yksittäiselle tarinalle
    [System.Serializable]
    public class TarinaData
    {
        public string teksti;
        public string pvm;
    }

    [System.Serializable]
    public class ListWrapper { public List<TarinaData> tarinat; }
}

