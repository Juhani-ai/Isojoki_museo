using UnityEngine;
using TMPro;
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

    private string tiedostoPolku;
    private string filtteriPolku;
    private List<TarinaData> tarinat = new List<TarinaData>();
    private List<string> kielletytSanat = new List<string>();

    void Start()
    {
        // POLKU-ASIOIDEN HOITO:
        // Editorissa: Assets/tarinat.json (Näkyy heti projektissa)
        // Buildissa: Pelin asennuskansio (Museon väen helppo löytää)
        #if UNITY_EDITOR
            tiedostoPolku = Path.Combine(Application.dataPath, "tarinat.json");
        #else
            string juuri = Path.GetDirectoryName(Application.dataPath);
            tiedostoPolku = Path.Combine(juuri, "tarinat.json");
        #endif

        filtteriPolku = Path.Combine(Application.streamingAssetsPath, "sensuuri.json");

        // Pakotetaan tiedoston luonti heti käynnistyksessä
        LataaTarinatPaikallisesti();
        TallennaTarinatPaikallisesti();

        LataaFiltteri();
        AktivoiKirjoitustila();
        
        if(asiatonViestiPaneeli != null) asiatonViestiPaneeli.SetActive(false);

        // Tulostetaan sijainti, jotta voit kopioida sen jos se on vieläkin hukassa
        Debug.Log("<color=green>TARINAT TALLENTUVAT TÄNNE: </color>" + tiedostoPolku);
    }

    public void TallennaTarina()
    {
        if (syoteInputField == null) return;
        
        string teksti = syoteInputField.text;
        if (string.IsNullOrWhiteSpace(teksti)) return;

        if (SisaltaakoTorkya(teksti))
        {
            syoteInputField.text = ""; // Tyhjennetään törkykenttä
            NaytaVirhe();
            return; 
        }

        TarinaData uusi = new TarinaData {
            teksti = teksti,
            pvm = DateTime.Now.ToString("d.M.yyyy 'klo' HH:mm")
        };

        tarinat.Add(uusi);
        TallennaTarinatPaikallisesti();

        syoteKentta.SetActive(false);
        kiitosViesti.SetActive(true);
        tallennaNappi.SetActive(false);
        kirjoitaNappi.SetActive(true);
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
            CancelInvoke("PiilotaVirhe");
            Invoke("PiilotaVirhe", virheNayttoaika);
        }
    }

    private void PiilotaVirhe()
    {
        if (asiatonViestiPaneeli != null) asiatonViestiPaneeli.SetActive(false);
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
        syoteKentta.SetActive(false);
        kiitosViesti.SetActive(false);
        scrollPaneeli.SetActive(true);
        if(asiatonViestiPaneeli != null) asiatonViestiPaneeli.SetActive(false);
        tallennaNappi.SetActive(false);
        kirjoitaNappi.SetActive(true);
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
            #if UNITY_EDITOR
                // Tämä pakottaa Unityn päivittämään Project-ikkunan
                UnityEditor.AssetDatabase.Refresh();
            #endif
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

