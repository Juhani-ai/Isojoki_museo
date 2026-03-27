using UnityEngine;
using TMP_Text = TMPro.TMP_Text;
using TMP_InputField = TMPro.TMP_InputField;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text.RegularExpressions;

public class TarinaManager : MonoBehaviour
{
    [Header("UI Paneelit")]
    public GameObject syoteKentta;
    public GameObject kiitosViesti;
    public GameObject scrollPaneeli;
    public GameObject asiatonViestiPaneeli;

    [Header("Napit")]
    public GameObject tallennaNappi;
    public GameObject kirjoitaNappi;

    [Header("Komponentit")]
    public TMP_InputField syoteInputField;
    public TMP_Text scrollTeksti;

    [Header("Asetukset")]
    public float virheNayttoaika = 3f;

    [Header("Äänet")]
    // Nyt vain AudioClipit, koska Master hoitaa sourcen
    public AudioClip onnistumisAani; 
    public AudioClip virheAani;      

    private string tiedostoPolku;
    private string filtteriPolku;
    private List<TarinaData> tarinat = new List<TarinaData>();
    private List<string> kielletytSanat = new List<string>();

    void Start()
    {
        #if UNITY_EDITOR
            tiedostoPolku = Path.Combine(Application.dataPath, "tarinat.json");
        #else
            string juuri = Path.GetDirectoryName(Application.dataPath);
            tiedostoPolku = Path.Combine(juuri, "tarinat.json");
        #endif

        filtteriPolku = Path.Combine(Application.streamingAssetsPath, "sensuuri.json");

        LataaTarinatPaikallisesti();
        TallennaTarinatPaikallisesti();

        LataaFiltteri();
        AktivoiKirjoitustila();
        
        if(asiatonViestiPaneeli != null) asiatonViestiPaneeli.SetActive(false);

        Debug.Log("<color=green>TARINAT TALLENTUVAT TÄNNE: </color>" + tiedostoPolku);
    }

    public void TallennaTarina()
    {
        if (syoteInputField == null) return;
        
        string teksti = syoteInputField.text;
        if (string.IsNullOrWhiteSpace(teksti)) return;

        // TARKISTETAAN SISÄLTÖ
        if (SisaltaakoTorkya(teksti))
        {
            syoteInputField.text = ""; 
            NaytaVirhe();
            
            // KORJATTU ÄÄNILOGIIKKA
            SoitaMasterinKautta(virheAani);
            return; 
        }

        // JOS TEKSTI ON OK, TALLENNETAAN
        TarinaData uusi = new TarinaData {
            teksti = teksti,
            pvm = DateTime.Now.ToString("d.M.yyyy 'klo' HH:mm")
        };

        tarinat.Add(uusi);
        TallennaTarinatPaikallisesti();

        // NÄYTETÄÄN KIITOSVIESTI
        syoteKentta.SetActive(false);
        kiitosViesti.SetActive(true);
        tallennaNappi.SetActive(false);
        kirjoitaNappi.SetActive(true);

        // KORJATTU ÄÄNILOGIIKKA
        SoitaMasterinKautta(onnistumisAani);
    }

    // UUSI APUMETODI ÄÄNELLE
    private void SoitaMasterinKautta(AudioClip klippi)
    {
        if (klippi != null && KaikkiYhdessaMaster.Instance != null)
        {
            // Etsitään Masterin SFX-source (se toinen koodilla luotu AudioSource)
            AudioSource[] sourcet = KaikkiYhdessaMaster.Instance.GetComponents<AudioSource>();
            if (sourcet.Length > 1) 
            {
                sourcet[1].PlayOneShot(klippi);
            }
        }
    }

    private bool SisaltaakoTorkya(string syote)
    {
        if (kielletytSanat == null || kielletytSanat.Count == 0) return false;

        foreach (string sana in kielletytSanat)
        {
            if (string.IsNullOrWhiteSpace(sana)) continue;
            string pattern = sana.Contains(" ") ? Regex.Escape(sana) : @"\b" + Regex.Escape(sana) + @"\b";
            if (Regex.IsMatch(syote, pattern, RegexOptions.IgnoreCase)) return true;
        }
        return false;
    }

    private void NaytaVirhe()
    {
        if (asiatonViestiPaneeli != null)
        {
            asiatonViestiPaneeli.SetActive(true);
            if (syoteKentta != null) syoteKentta.SetActive(false); 

            CancelInvoke("PiilotaVirhe");
            Invoke("PiilotaVirhe", virheNayttoaika);
        }
    }

    private void PiilotaVirhe()
    {
        if (asiatonViestiPaneeli != null) asiatonViestiPaneeli.SetActive(false);
        if (syoteKentta != null) syoteKentta.SetActive(true);
    }

    public void AktivoiKirjoitustila()
    {
        if(syoteKentta != null) syoteKentta.SetActive(true);
        if(kiitosViesti != null) kiitosViesti.SetActive(false);
        if(scrollPaneeli != null) scrollPaneeli.SetActive(false);
        if(asiatonViestiPaneeli != null) asiatonViestiPaneeli.SetActive(false);
        if(tallennaNappi != null) tallennaNappi.SetActive(true);
        if(kirjoitaNappi != null) kirjoitaNappi.SetActive(false);
        if(syoteInputField != null) syoteInputField.text = "";
    }

    public void AvaaSelaa()
    {
        if(syoteKentta != null) syoteKentta.SetActive(false);
        if(kiitosViesti != null) kiitosViesti.SetActive(false);
        if(scrollPaneeli != null) scrollPaneeli.SetActive(true);
        if(asiatonViestiPaneeli != null) asiatonViestiPaneeli.SetActive(false);
        if(tallennaNappi != null) tallennaNappi.SetActive(false);
        if(kirjoitaNappi != null) kirjoitaNappi.SetActive(true);
        PaivitaScrollTeksti();
    }

    private void LataaFiltteri()
    {
        if (File.Exists(filtteriPolku))
        {
            try {
                string json = File.ReadAllText(filtteriPolku);
                FiltteriWrapper wrapper = JsonUtility.FromJson<FiltteriWrapper>(json);
                if (wrapper != null) kielletytSanat = wrapper.sanat;
            } catch (Exception e) { Debug.LogError("Filtteri virhe: " + e.Message); }
        }
    }

    private void TallennaTarinatPaikallisesti()
    {
        try {
            string json = JsonUtility.ToJson(new ListWrapper { tarinat = tarinat }, true);
            File.WriteAllText(tiedostoPolku, json);
        } catch (Exception e) { Debug.LogError("Tallennus kusi: " + e.Message); }
    }

    private void LataaTarinatPaikallisesti()
    {
        if (File.Exists(tiedostoPolku))
        {
            try {
                string json = File.ReadAllText(tiedostoPolku);
                ListWrapper wrapper = JsonUtility.FromJson<ListWrapper>(json);
                if (wrapper != null) tarinat = wrapper.tarinat;
            } catch (Exception e) { Debug.LogError("Lataus kusi: " + e.Message); }
        }
    }

    private void PaivitaScrollTeksti()
    {
        if (scrollTeksti == null) return;
        string kooste = "";
        for (int i = tarinat.Count - 1; i >= 0; i--)
        {
            kooste += "<color=#888888><size=80%>" + tarinat[i].pvm + "</size></color>\n";
            kooste += tarinat[i].teksti + "\n\n________________________\n\n";
        }
        scrollTeksti.text = kooste;
    }

    [Serializable] public class TarinaData { public string teksti; public string pvm; }
    [Serializable] public class ListWrapper { public List<TarinaData> tarinat; }
    [Serializable] public class FiltteriWrapper { public List<string> sanat; }
}

