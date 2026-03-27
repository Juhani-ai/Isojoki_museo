using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pistemanageri : MonoBehaviour, IPointerClickHandler
{
   public static Pistemanageri instance;
   public static int kokonaisPisteet = 0;
  
   [Header("UI-Viitteet")]
   public GameObject varmistusPaneeli;
   public TMP_Text pisteNaytto;

   [Header("Äänet")]
   public AudioSource audioSource;
   public AudioClip kolikkoAani;
   public AudioClip ostoAani;
   public AudioClip virheAani;

   [Header("Efektit")]
   public float hytkyVoimakkuus = 1.25f;
   public float hytkyKesto = 0.2f;
   private Vector3 alkuperainenKoko;
   private Color alkuperainenVari;

   void Awake() {
       if (instance == null) {
           instance = this;
           DontDestroyOnLoad(gameObject);
           kokonaisPisteet = PlayerPrefs.GetInt("MuseoPisteet", 0);
       } else {
           Destroy(gameObject);
       }
   }

   void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
   void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

   void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
       varmistusPaneeli = null;
       pisteNaytto = null;
       StartCoroutine(EtsiUudenSkenenObjektit());
   }

   IEnumerator EtsiUudenSkenenObjektit() {
       yield return new WaitForEndOfFrame();
       GameObject[] kaikki = Resources.FindObjectsOfTypeAll<GameObject>();
      
       foreach (GameObject go in kaikki) {
           if (go.name == "VaroitusPaneeli") {
               varmistusPaneeli = go;
               varmistusPaneeli.SetActive(false);
           }
           if (go.name == "Pistelaskuri") {
               pisteNaytto = go.GetComponent<TMP_Text>();
               if (pisteNaytto != null) {
                   alkuperainenKoko = pisteNaytto.transform.localScale;
                   alkuperainenVari = pisteNaytto.color;
               }
           }
           if (go.name == "Reset -nappi") {
               Button b = go.GetComponent<Button>();
               if (b != null) {
                   b.onClick.RemoveAllListeners();
                   b.onClick.AddListener(NaytaVarmistus);
               }
           }
       }

       if (varmistusPaneeli != null) {
           Button[] paneelinNapit = varmistusPaneeli.GetComponentsInChildren<Button>(true);
           foreach (Button b in paneelinNapit) {
               if (b.name == "Nollaa -nappi") {
                   b.onClick.RemoveAllListeners();
                   b.onClick.AddListener(NollaaKokoPeli);
               }
               if (b.name == "Peruuta -nappi") {
                   b.onClick.RemoveAllListeners();
                   b.onClick.AddListener(PiilotaVarmistus);
               }
           }
       }
       PaivitaUI();
   }

   public void NaytaVarmistus() { if (varmistusPaneeli != null) varmistusPaneeli.SetActive(true); }
   public void PiilotaVarmistus() { if (varmistusPaneeli != null) varmistusPaneeli.SetActive(false); }

   public void NollaaKokoPeli() {
       PlayerPrefs.DeleteAll();
       PlayerPrefs.Save();
       kokonaisPisteet = 0;
       SceneManager.LoadScene(SceneManager.GetActiveScene().name);
   }

   public void PaivitaUI() { if (pisteNaytto != null) pisteNaytto.text = "Pisteet: " + kokonaisPisteet; }

   public static void LisaaPisteita(int maara) {
       kokonaisPisteet += maara;
       if (instance != null) instance.KasitteleMuutos(true);
       TallennaPisteet();
   }

   // TÄMÄ PUUTTUI JA AIHEUTTI VIRHEEN:
   public static void TallennaPisteet() {
       PlayerPrefs.SetInt("MuseoPisteet", kokonaisPisteet);
       PlayerPrefs.Save();
   }

   public void KasitteleMuutos(bool onnistui) {
       PaivitaUI();
       if (audioSource != null) {
           AudioClip clip = onnistui ? kolikkoAani : virheAani;
           if (clip != null) audioSource.PlayOneShot(clip);
       }
       if (pisteNaytto != null) {
           StopAllCoroutines();
           StartCoroutine(Efekti(onnistui));
       }
   }

   IEnumerator Efekti(bool onnistui) {
       if (pisteNaytto == null) yield break;
       if (!onnistui) {
           pisteNaytto.color = Color.red;
           pisteNaytto.transform.localScale = alkuperainenKoko * 1.15f;
           yield return new WaitForSeconds(0.15f);
           pisteNaytto.color = alkuperainenVari;
           pisteNaytto.transform.localScale = alkuperainenKoko;
           yield break;
       }

       float aikaaKulunut = 0;
       float valkeNopeus = 0.05f;
       Color oranssiVari = new Color(1f, 0.64f, 0f); 
       Color keltainenVari = Color.yellow;
       pisteNaytto.transform.localScale = alkuperainenKoko * hytkyVoimakkuus;

       while (aikaaKulunut < hytkyKesto) {
           pisteNaytto.color = oranssiVari;
           yield return new WaitForSeconds(valkeNopeus);
           pisteNaytto.color = Color.white;
           yield return new WaitForSeconds(valkeNopeus);
           pisteNaytto.color = keltainenVari;
           yield return new WaitForSeconds(valkeNopeus);
           aikaaKulunut += valkeNopeus * 3;
       }
       pisteNaytto.color = alkuperainenVari;
       pisteNaytto.transform.localScale = alkuperainenKoko;
   }

   public void OnPointerClick(PointerEventData eventData) { LisaaPisteita(10); }
}

