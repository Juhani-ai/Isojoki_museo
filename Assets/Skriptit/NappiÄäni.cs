using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class NappiAani : MonoBehaviour
{
    void Start()
    {
        // Haetaan nappi-komponentti ja lisätään kuuntelija koodilla
        GetComponent<Button>().onClick.AddListener(Soita);
    }

    void Soita()
    {
        // Tämä etsii AINA sen hetkisen elossa olevan Instancen
        if (KaikkiYhdessaMaster.Instance != null)
        {
            KaikkiYhdessaMaster.Instance.SoitaKlikkaus();
        }
        else
        {
            Debug.LogWarning("AudioManageria ei löytynyt skenestä!");
        }
    }
}

