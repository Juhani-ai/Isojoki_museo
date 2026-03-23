using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System;

public class TarinaManager : MonoBehaviour
{
    [Header("UI Paneelit")]
    public GameObject syoteKentta;
    public GameObject kiitosViesti;
    public GameObject scrollPaneeli;
    
    [Header("Napit")]
    public GameObject tallennaNappi;
    public GameObject kirjoitaNappi;

    [Header("Komponentit")]
    public TMP_InputField syoteInputField;
    public TMP_Text scrollTeksti;

    private string tiedostoPolku;
    private List<TarinaData> tarinat = new List<TarinaData>();

    void Start()
    {
        // Tallennetaan suoraan koneen levylle (StreamingAssets tai PersistentDataPath)
        tiedostoPolku = Path.Combine(Application.streamingAssetsPath, "tarinat.json");

        // Varmistetaan että kansio on olemassa
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }

        LataaTarinatPaikallisesti();
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
        TallennaTarinatPaikallisesti();

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
        
        PaivitaScrollTeksti();
    }

    private void TallennaTarinatPaikallisesti()
    {
        try 
        {
            string json = JsonUtility.ToJson(new ListWrapper { tarinat = tarinat }, true);
            File.WriteAllText(tiedostoPolku, json);
            Debug.Log("Tallennettu polkuun: " + tiedostoPolku);
        }
        catch (Exception e) 
        {
            Debug.LogError("Tallennus kusi: " + e.Message);
        }
    }

    private void LataaTarinatPaikallisesti()
    {
        if (File.Exists(tiedostoPolku))
        {
            try 
            {
                string json = File.ReadAllText(tiedostoPolku);
                ListWrapper wrapper = JsonUtility.FromJson<ListWrapper>(json);
                if (wrapper != null && wrapper.tarinat != null) 
                {
                    tarinat = wrapper.tarinat;
                }
            }
            catch (Exception e) 
            {
                Debug.LogError("Lataus kusi: " + e.Message);
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

    [System.Serializable] public class TarinaData { public string teksti; public string pvm; }
    [System.Serializable] public class ListWrapper { public List<TarinaData> tarinat; }
}

