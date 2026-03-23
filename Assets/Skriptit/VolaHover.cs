using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VolaHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Asetukset")]
    public GameObject sliderObjekti; // Vedä se 'Slider' tähän
    public float fadeNopeus = 8f;
    
    private CanvasGroup cg;
    private bool hiiriPaalla = false;
    private Slider sliderComponentti;

    void Awake()
    {
        // Varmistetaan että sliderilla on CanvasGroup häivytystä varten
        cg = sliderObjekti.GetComponent<CanvasGroup>();
        if (cg == null) cg = sliderObjekti.AddComponent<CanvasGroup>();
        
        sliderComponentti = sliderObjekti.GetComponent<Slider>();
        cg.alpha = 0; // Aloitetaan piilossa
    }

    void Start()
    {
        if (sliderComponentti != null && KaikkiYhdessaMaster.Instance != null)
        {
            // Asetetaan sliderin asento muistista
            sliderComponentti.value = PlayerPrefs.GetFloat("MasterVol", 0.5f);

            // KYTKETÄÄN PIUHA: Slider -> AudioManager
            sliderComponentti.onValueChanged.RemoveAllListeners();
            sliderComponentti.onValueChanged.AddListener(KaikkiYhdessaMaster.Instance.MuutaMasteria);
        }
    }

    void Update()
    {
        // Pehmeä häivytys
        float tavoite = hiiriPaalla ? 1f : 0f;
        cg.alpha = Mathf.MoveTowards(cg.alpha, tavoite, Time.deltaTime * fadeNopeus);
        
        // Estetään sliderin käyttö kun se on läpinäkyvä
        cg.blocksRaycasts = cg.alpha > 0.2f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hiiriPaalla = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hiiriPaalla = false;
    }
}

