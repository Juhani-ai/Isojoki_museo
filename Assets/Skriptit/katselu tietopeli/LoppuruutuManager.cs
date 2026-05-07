using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoppuruutuManager : MonoBehaviour
{
    [Header("Tekstit")]
    [SerializeField] private TMP_Text tulosText;

    [Header("Napit")]
    [SerializeField] private Button pelaaUudelleenNappi;
    [SerializeField] private Button kotiruutuNappi;

    [Header("Scenet")] 
    [SerializeField] private string kotisceneNimi;

    void OnEnable()
    {
        int avattujenMaara = EsineRekisteri.HaeAvatutEsineet().Count;

        int oikeinMaara = Pistemanageri.kokonaisPisteet / 10;
        tulosText.text = "Oikein: " + oikeinMaara + "/" + avattujenMaara;

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
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void PalaaKotiin()
        {
            SceneManager.LoadScene(kotisceneNimi);
        }
}