using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class TarinaManager : MonoBehaviour
{
    private string jsonPolku;
    private System.Collections.Generic.List<string> tarinat =
        new System.Collections.Generic.List<string>();

    void Start()
    {
        jsonPolku = Path.Combine(Application.streamingAssetsPath, "tarinat.json");
        // Alusta: Syote aktiivinen, muut ei
        SetPanelActive("Syote", true);
        SetPanelActive("Skrolli", false);
        SetPanelActive("Kiitos", false);
    }

    // Tallenna tarina ja näytä Kiitos
    public void TallennaTarina()
    {
        TMP_InputField inputField = GameObject.FindGameObjectWithTag("Syote").GetComponent<TMP_InputField>();
        if (string.IsNullOrEmpty(inputField.text)) return;

        tarinat.Add(inputField.text);
        StartCoroutine(TallennaTarinatJSON());

        // Vaihda paneelit
        SetPanelActive("Syote", false);
        SetPanelActive("Kiitos", true);
    }

    // Avaa Skrolli
    public void AvaaSelaa()
    {
        SetPanelActive("Kiitos", false);
        SetPanelActive("Skrolli", true);
        StartCoroutine(LataaJaNaytaTarinat());
    }

    // Apu-metodi paneelien aktivoimiseen
    private void SetPanelActive(string tag, bool active)
    {
        GameObject panel = GameObject.FindGameObjectWithTag(tag);
        if (panel != null) panel.SetActive(active);
    }

    // Tallenna JSON
    private IEnumerator TallennaTarinatJSON()
    {
        string json = JsonUtility.ToJson(new ListWrapper { tarinat = tarinat });
        if (Application.platform != RuntimePlatform.WebGLPlayer)
            File.WriteAllText(jsonPolku, json);
        yield return null;
    }

    // Lataa ja näytä tarinat
    private IEnumerator LataaJaNaytaTarinat()
    {
        UnityWebRequest request = UnityWebRequest.Get(jsonPolku);
        yield return request.SendWebRequest();

        TMP_Text tarinaNaytto = GameObject.FindGameObjectWithTag("Skrolli").GetComponentInChildren<TMP_Text>();
        if (request.result == UnityWebRequest.Result.Success)
        {
            tarinat = JsonUtility.FromJson<ListWrapper>(request.downloadHandler.text).tarinat;
            string tarinaTeksti = "";
            foreach (string tarina in tarinat)
                tarinaTeksti += "- " + tarina + "\n\n";
            tarinaNaytto.text = tarinaTeksti;
        }
        else
        {
            tarinaNaytto.text = "Tarinoita ei voitu ladata.";
        }
    }

    [System.Serializable]
    public class ListWrapper { public System.Collections.Generic.List<string> tarinat; }
}
