using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoppuruutuManager : MonoBehaviour
{
    [Header("Tekstit")]
    [SerializeField] private TMP_Text tulosText;
    [SerializeField] private TMP_Text pisteetText;

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
        int avattujenMaara = EsineRekisteri.HaeAvatutEsineet().Count;
        int kierrosPisteet = Pistemanageri.kokonaisPisteet - pisteetAlussa;

        tulosText.text = "Oikein: " + oikeinLaskuri + "/" + avattujenMaara;
        pisteetText.text = "Kierros pisteet: " + kierrosPisteet;

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