using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionPanel : MonoBehaviour
{
    [Header ("Kysymykset")]
    [SerializeField] private TMP_Text questionText;

    [SerializeField] private Button correctButton;
    [SerializeField] private Button wrongButton1;
    [SerializeField] private Button wrongButton2;
    [SerializeField] private Button wrongButton3;

    [SerializeField] private string esineID;

    [Header ("Seuraava")]
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextText;
    
    [Header ("Vastaus")]
    [SerializeField] private Button answerButton;
    [SerializeField] private TMP_Text answerbuttonText;

    [Header ("Pisteet")]
    [SerializeField] private TMP_Text pointText;

    [Header ("Objekti")]
    [SerializeField] private GameObject rotationObject;

    [Header ("Manageri")]
    [SerializeField] private PanelManager panelManager;

    private bool onVastattu = false; 

    void Start()
    {
        LoppuruutuManager.TallennaPisteetAlussa();
        
        nextButton.interactable = false;

        correctButton.onClick.AddListener(CorrectClicked);
        wrongButton1.onClick.AddListener(() => WrongClicked(wrongButton1));
        wrongButton2.onClick.AddListener(() => WrongClicked(wrongButton2));
        wrongButton3.onClick.AddListener(() => WrongClicked(wrongButton3));
        nextButton.onClick.AddListener(() => Debug.Log("Toimii"));
        nextButton.onClick.AddListener(NextClicked);
        
        answerButton.onClick.AddListener(AnswerClicked);

        pointText.text = "Pisteet: " + Pistemanageri.kokonaisPisteet;
    }

    void WrongClicked(Button btn)
    {
        if (onVastattu) return;
        onVastattu = true;

        if (Pistemanageri.kokonaisPisteet > 0)
        {
            Pistemanageri.LisaaPisteita(-5);
        }
        
        pointText.text = "Pisteet: " + Pistemanageri.kokonaisPisteet;

        nextButton.interactable = true;

        SetButtonInteractable(correctButton, false);
        SetButtonInteractable(wrongButton1, false);
        SetButtonInteractable(wrongButton2, false);
        SetButtonInteractable(wrongButton3, false);

        answerButton.gameObject.SetActive(true);
        answerButton.interactable = true;

        btn.image.color = Color.red;
    }

    void CorrectClicked()
    {
        if (onVastattu) return; 
        onVastattu = true;

        LoppuruutuManager.LisaaOikein(); // ← LISÄTTY

        Debug.Log("Pisteet ennen: " + Pistemanageri.kokonaisPisteet);
        Pistemanageri.LisaaPisteita(10);
        Debug.Log("Pisteet jälkeen: " + Pistemanageri.kokonaisPisteet);

        pointText.text = "Pisteet: " + Pistemanageri.kokonaisPisteet;

        nextButton.interactable = true;

        SetButtonInteractable(correctButton, false);
        SetButtonInteractable(wrongButton1, false);
        SetButtonInteractable(wrongButton2, false);
        SetButtonInteractable(wrongButton3, false);

        correctButton.image.color = Color.green;
    }

    void AnswerClicked()
    {
        if (answerbuttonText.text == "Paljasta oikea vastaus")
        {
            ColorUtility.TryParseHtmlString("#736A53", out Color paljastettuVari);
            answerbuttonText.text = "Piilota oikea vastaus";
            answerButton.image.color = paljastettuVari;

            correctButton.image.color = Color.green;
        }
        else if (answerbuttonText.text == "Piilota oikea vastaus")
        {
            ColorUtility.TryParseHtmlString("#DCC896", out Color alkuperainenVari);

            answerbuttonText.text = "Paljasta oikea vastaus";
            answerButton.image.color = alkuperainenVari;
            
            correctButton.image.color = alkuperainenVari;
        } 
    }

    void SetButtonInteractable(Button btn, bool interactable)
    {
        btn.interactable = interactable;
        
        ColorBlock cb = btn.colors;
        cb.disabledColor = new Color(
            cb.disabledColor.r,
            cb.disabledColor.g,
            cb.disabledColor.b,
            0.8f
        );
        btn.colors = cb;
    }

    public string HaeEsineID()
    {
        return esineID;
    }

    void NextClicked()
    {
        rotationObject.gameObject.SetActive(false);
        panelManager.NaytaSeuraavaAvattuPaneeli(this.gameObject);
    }
}