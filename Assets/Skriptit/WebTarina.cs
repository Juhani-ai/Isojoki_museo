using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;

public class TarinaManager : MonoBehaviour
{
    [Header("Palvelimen osoitteet")]
    public string palvelimenUrl = "https://isojokiseura.fi/peli/tallenna.php";
    public string jsonUrl = "https://isojokiseura.fi/peli/tarinat.json";

    [Header("UI Paneelit & Napit")]
    public GameObject syoteKentta;
    public GameObject kiitosViesti;
    public GameObject scrollPaneeli;
    public GameObject tallennaNappi;
    public GameObject kirjoitaNappi;

    [Header("Komponentit")]
    public TMP_InputField syoteInputField;
    public TMP_Text scrollTeksti;

    private List<TarinaData> tarinat = new List<TarinaData>();

    void Start()
    {
        // Ladataan tarinat heti alussa palvelimelta
        StartCoroutine(LataaTarinatPalvelimelta());
        AktivoiKirjoitustila();
    }

    public void AktivoiKirjoitustila()
    {
        syoteKentta.SetActive(true);
        kiitosViesti.SetActive(false);
        scrollPaneeli.SetActive(false);
        tallennaNappi.SetActive(true);
        kirjoitaNappi.SetActive(false);
        syoteInputField.text = "";
    }

    public void TallennaTarina()
    {
        if (string.IsNullOrWhiteSpace(syoteInputField.text)) return;

        TarinaData uusi = new TarinaData {
            teksti = syoteInputField.text,
            pvm = DateTime.Now.ToString("d.M.yyyy 'klo' HH:mm")
        };

        tarinat.Add(uusi);
        StartCoroutine(LahetaTarinatPalvelimelle());

        syoteKentta.SetActive(false);
        kiitosViesti.SetActive(true);
        tallennaNappi.SetActive(false);
        kirjoitaNappi.SetActive(true);
    }

    public void AvaaSelaa()
    {
        syoteKentta.SetActive(false);
        kiitosViesti.SetActive(false);
        scrollPaneeli.SetActive(true);
        tallennaNappi.SetActive(false);
        kirjoitaNappi.SetActive(true);
        
        // Päivitetään näkymä (uusin ensin)
        PaivitaScrollTeksti();
    }

    private IEnumerator LahetaTarinatPalvelimelle()
    {
        string json = JsonUtility.ToJson(new ListWrapper { tarinat = tarinat });
        
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(palvelimenUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError("Tallennusvirhe: " + www.error);
            else
                Debug.Log("Palvelin vastasi: " + www.downloadHandler.text);
        }
    }

    private IEnumerator LataaTarinatPalvelimelta()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(jsonUrl))
        {
            // Lisätään satunnainen numero URL-osoitteeseen, jotta selain ei lataa vanhaa versiota välimuistista
            www.url += "?t=" + UnityEngine.Random.Range(0, 1000000);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                ListWrapper wrapper = JsonUtility.FromJson<ListWrapper>(www.downloadHandler.text);
                if (wrapper != null && wrapper.tarinat != null) tarinat = wrapper.tarinat;
                Debug.Log("Ladattu " + tarinat.Count + " tarinaa.");
            }
        }
    }

    private void PaivitaScrollTeksti()
    {
        string kooste = "";
        for (int i = tarinat.Count - 1; i >= 0; i--)
        {
            kooste += "<color=#888888><size=80%>" + tarinat[i].pvm + "</size></color>\n";
            kooste += tarinat[i].teksti + "\n\n________________________\n\n";
        }
        scrollTeksti.text = kooste;
    }

    [System.Serializable] public class TarinaData { public string teksti; public string pvm; }
    [System.Serializable] public class ListWrapper { public List<TarinaData> tarinat; }
}

