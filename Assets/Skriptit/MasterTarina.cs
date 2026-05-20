/*using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

public class TarinaManager : MonoBehaviour
{
    [Header("Palvelimen osoitteet")]
    // KORJATTU: Suhteelliset polut absoluuttisten sijaan!
    public string palvelimenUrl = "tallenna.php";
    public string jsonUrl = "tarinat.json";

    [Header("UI Paneelit")]
    public GameObject syoteKentta;
    public GameObject kiitosViesti;
    public GameObject scrollPaneeli;
    public GameObject asiatonViestiPaneeli;

    [Header("Napit ja Komponentit")]
    public GameObject tallennaNappi;
    public GameObject kirjoitaNappi;
    public TMP_InputField syoteInputField;
    public TMP_Text scrollTeksti;

    [Header("Äänet")]
    public AudioSource aaniLahde;
    public AudioClip kiitosAani;
    public AudioClip sensuuriAani;

    [Header("Asetukset")]
    public float virheNayttoaika = 3f;

    [Header("Sensuuri (Kovakoodattu WebGL:ää varten)")]
    private List<string> kielletytSanat = new List<string> {
        "vittu", "v-i-t-t-u", "vitun", "vittuun", "vituska", "saatana", "s-a-a-t-a-n-a", "saatanan", 
        "saatanasta", "saatanalle", "saatanaan", "paska", "p-a-s-k-a", "paskan", "paskaksi", "paskaan", 
        "perkele", "p-e-r-k-e-l-e", "perkeleen", "perkeleeseen", "perkeleellinen", "huora", "h-u-o-r-a", 
        "huoran", "huoralle", "huoralta", "kyrpä", "k-y-r-p-ä", "kyrpää", "kyrvästä", "kyrpään", "neekeri", 
        "n-e-e-k-e-r-i", "neekerin", "neekeriltä", "vinosilmä", "v-i-n-o-s-i-l-m-ä", "vinosilmän", 
        "vinosilmältä", "vinosilmälle", "ählämi", "ä-h-l-ä-m-i", "ählämin", "ählämille", "ählämiltä", 
        "ryssä", "r-y-s-s-ä", "ryssän", "ryssältä", "ryssälle", "mutiainen", "m-u-t-i-a-i-n-e-n", 
       "mutiaisen", "mutiaiselta", "mutiaiselle", "mutiaisella", "homo", "h-o-m-o", "homon", "homotti", 
       "homolta", "homolle", "homolla", "lesbo", "l-e-s-b-o", "lesbolta", "lesbon", "lesbolle", "perse", 
       "p-e-r-s-e", "perseen", "perseestä", "persettä", "perseeseen", "helvetti", "h-e-l-v-e-t-t-i", 
       "helvetin", "helvettiin", "helvettiä", "helvetistä", "ime kyrpää", "imekyrpää", "ime  kyrpää", 
       "fuck", "f-u-c-k", "fucking", "shit", "s-h-i-t", "shitting", "cunt", "c-u-n-t", "pussy", 
       "p-u-s-s-y", "pussies", "eat shit", "eatshit", "eat  shit", "fuck me", "fuckme", "fuck  me", 
       "dick", "d-i-c-k", "suck my dick", "suckmydick", "suck  my  dick", "fuck my pussy", 
       "fuckmypussy", "fuck  my  pussy", "fan", "fitta", "kuk", "arsle", "fanken", "fy fan", 
       "jävlar", "jävla", "jävel", "jävligt", "helvete", "skit", "skitbra", "skitsnack", 
       "jävla skit", "fasiken", "jäklar", "håll käften"
    };

    private List<TarinaData> tarinat = new List<TarinaData>();

    void Start()
    {
        EtsiAudioLahde();
        StartCoroutine(LataaTarinatPalvelimelta());
        AktivoiKirjoitustila();
        if(asiatonViestiPaneeli != null) asiatonViestiPaneeli.SetActive(false);
    }

    private void EtsiAudioLahde()
    {
        // Jos viite on Missing tai null, etsitään se aktiivisesti
        if (aaniLahde == null)
        {
            aaniLahde = UnityEngine.Object.FindAnyObjectByType<AudioSource>();
            if (aaniLahde != null) Debug.Log("GIMMO: AudioSource kytketty dynaamisesti!");
        }
    }

    public void TallennaTarina()
    {
        // Varmistetaan haku vielä juuri ennen soittoa
        EtsiAudioLahde();

        if (syoteInputField == null) return;
        
        string teksti = syoteInputField.text;
        if (string.IsNullOrWhiteSpace(teksti)) return;

        if (SisaltaakoTorkya(teksti))
        {
            if (aaniLahde != null && sensuuriAani != null) 
            {
                aaniLahde.PlayOneShot(sensuuriAani);
                Debug.Log("GIMMO: Soitetaan sensuuriääni.");
            }
            syoteInputField.text = ""; 
            NaytaVirhe();
            return; 
        }

        if (aaniLahde != null && kiitosAani != null) 
        {
            aaniLahde.PlayOneShot(kiitosAani);
            Debug.Log("GIMMO: Soitetaan kiitosääni.");
        }

        TarinaData uusi = new TarinaData {
            teksti = teksti,
            pvm = DateTime.Now.ToString("d.M.yyyy 'klo' HH:mm")
        };

        tarinat.Add(uusi);
        StartCoroutine(LahetaTarinatPalvelimelle());

        if(syoteKentta != null) syoteKentta.SetActive(false);
        if(kiitosViesti != null) kiitosViesti.SetActive(true);
        if(tallennaNappi != null) tallennaNappi.SetActive(false);
        if(kirjoitaNappi != null) kirjoitaNappi.SetActive(true);
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
        if(syoteKentta != null) syoteKentta.SetActive(false);
        if(kiitosViesti != null) kiitosViesti.SetActive(false);
        if(scrollPaneeli != null) scrollPaneeli.SetActive(true);
        if(tallennaNappi != null) tallennaNappi.SetActive(false);
        if(kirjoitaNappi != null) kirjoitaNappi.SetActive(true);
        PaivitaScrollTeksti();
    }

    private IEnumerator LahetaTarinatPalvelimelle()
    {
        string json = JsonUtility.ToJson(new ListWrapper { tarinat = tarinat });
        UnityWebRequest www = UnityWebRequest.PostWwwForm(palvelimenUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();
        
        StartCoroutine(LataaTarinatPalvelimelta());
    }

    private IEnumerator LataaTarinatPalvelimelta()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(jsonUrl + "?t=" + UnityEngine.Random.Range(0, 10000)))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                ListWrapper wrapper = JsonUtility.FromJson<ListWrapper>(www.downloadHandler.text);
                if (wrapper != null && wrapper.tarinat != null) tarinat = wrapper.tarinat;
            }
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
}*/

