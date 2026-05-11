using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoppuruutuManager : MonoBehaviour
{
    [Header("Tekstit")]
    [SerializeField] private TMP_Text tulosText;
    [SerializeField] private TMP_Text pisteetText;
    [SerializeField] private TMP_Text ylakulmanPisteetText;

    [Header("Napit")]
    [SerializeField] private Button pelaaUudelleenNappi;
    [SerializeField] private Button kotiruutuNappi;

    [Header("Scenet")] 
    [SerializeField] private string kotisceneNimi;

    private static int pisteetAlussa = 0;
    private static bool tallennettu = false;
    private static int oikeinLaskuri = 0;

    void OnEnable()
    {
        int avattujenMaara = 0;
        string[] kaikki = EsineRekisteri.kaikkiEsineet;
        string[] ainaAuki = { "Kahvihuhmar", "Kappa", "Kauha", "Koristelupuu", "Leikkihevonen" };
        
        foreach (string id in kaikki)
        {
            bool onAinaAuki = System.Array.Exists(ainaAuki, e => e == id);
            bool onOstettu = PlayerPrefs.GetInt("Unlocked_" + id, 0) == 1 || 
                             PlayerPrefs.GetInt("Unlocked_" + id + " -kortti", 0) == 1;
            if (onAinaAuki || onOstettu) avattujenMaara++;
        }

        int kierrosPisteet = Pistemanageri.kokonaisPisteet - pisteetAlussa;

        tulosText.text = "Oikein: " + oikeinLaskuri + "/" + avattujenMaara;
        pisteetText.text = "Kierros pisteet: " + kierrosPisteet;

        if (ylakulmanPisteetText != null)
            ylakulmanPisteetText.text = "Pisteet: " + Pistemanageri.kokonaisPisteet;

        pelaaUudelleenNappi.onClick.AddListener(PelaaUudelleen);
        kotiruutuNappi.onClick.AddListener(PalaaKotiin);
    }

    void OnDisable()
    {
        pelaaUudelleenNappi.onClick.RemoveAllListeners();
        kotiruutuNappi.onClick.RemoveAllListeners();
    }

    void PelaaUudelleen()
    {
        tallennettu = false;
        oikeinLaskuri = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void PalaaKotiin()
    {
        tallennettu = false;
        oikeinLaskuri = 0;
        SceneManager.LoadScene(kotisceneNimi);
    }

    public static void TallennaPisteetAlussa()
    {
        if (!tallennettu)
        {
            pisteetAlussa = Pistemanageri.kokonaisPisteet;
            tallennettu = true;
        }
    }

    public static void LisaaOikein()
    {
        oikeinLaskuri++;
    }
}