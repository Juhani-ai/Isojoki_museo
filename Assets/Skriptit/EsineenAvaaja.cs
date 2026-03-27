using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EsineenAvaaja : MonoBehaviour
{
    public string esineenID = "Pahkakuppi"; 
    public int hinta = 100;
    public GameObject lukkoPaneeli; 
    public Button ostaNappi;

    private Vector3 alkuperainenSkaala;
    private Image kortinKuva; 

    void Start() {
        alkuperainenSkaala = transform.localScale;
        kortinKuva = GetComponent<Image>();

        if (PlayerPrefs.GetInt("Unlocked_" + esineenID, 0) == 1) {
            AvaaLopullisesti(false); 
        }
    }

    public void YritaAvata() {
        if (Pistemanageri.kokonaisPisteet >= hinta) {
            Pistemanageri.kokonaisPisteet -= hinta;
            
            if (Pistemanageri.instance != null) {
                // Kutsutaan tallennusta ja efektiä Pistemanagerista
                Pistemanageri.TallennaPisteet();
                Pistemanageri.instance.KasitteleMuutos(true);
            }

            PlayerPrefs.SetInt("Unlocked_" + esineenID, 1);
            PlayerPrefs.Save();
            
            StartCoroutine(AvausEfekti());
        } else {
            if (Pistemanageri.instance != null) Pistemanageri.instance.KasitteleMuutos(false);
        }
    }

    IEnumerator AvausEfekti() {
        float kesto = 0.15f; 
        Vector3 tavoiteSkaala = alkuperainenSkaala * 1.3f;
        
        float aika = 0;
        while (aika < kesto) {
            float t = aika / kesto;
            transform.localScale = Vector3.Lerp(alkuperainenSkaala, tavoiteSkaala, t);
            if (kortinKuva != null) kortinKuva.color = Color.Lerp(Color.white, Color.yellow, t);
            aika += Time.deltaTime;
            yield return null;
        }

        AvaaLopullisesti(true);

        aika = 0;
        while (aika < kesto) {
            float t = aika / kesto;
            transform.localScale = Vector3.Lerp(tavoiteSkaala, alkuperainenSkaala, t);
            if (kortinKuva != null) kortinKuva.color = Color.Lerp(Color.yellow, Color.white, t);
            aika += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = alkuperainenSkaala;
        if (kortinKuva != null) kortinKuva.color = Color.white;
    }

    void AvaaLopullisesti(bool kaytaEfektia) {
        if (lukkoPaneeli != null) lukkoPaneeli.SetActive(false); 
        if (ostaNappi != null) ostaNappi.gameObject.SetActive(false); 
    }
}

