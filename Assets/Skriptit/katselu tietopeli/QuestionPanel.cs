using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text questionText;

    [SerializeField] private Button correctButton;
    [SerializeField] private Button wrongButton1;
    [SerializeField] private Button wrongButton2;
    [SerializeField] private Button wrongButton3;

    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextText;
    [SerializeField] private TMP_Text wrongText;
    [SerializeField] private Button answerButton;
    [SerializeField] private TMP_Text answerText;
    [SerializeField] private TMP_Text answerbuttonText;

    [SerializeField] private Button endButton;

    private bool onVastattu = false; 

    void Start()
    {
 
        nextButton.interactable = false;

        correctButton.onClick.AddListener(CorrectClicked);
        wrongButton1.onClick.AddListener(() => WrongClicked(wrongButton1));
        wrongButton2.onClick.AddListener(() => WrongClicked(wrongButton2));
        wrongButton3.onClick.AddListener(() => WrongClicked(wrongButton3));
        nextButton.onClick.AddListener(() => Debug.Log("Toimii"));
        
        answerButton.onClick.AddListener(AnswerClicked);
       
        endButton.onClick.AddListener(EndClicked);
    }

    void WrongClicked(Button btn)
    {
        if (onVastattu) return;
        onVastattu = true;

        Pistemanageri.LisaaPisteita(-5);

        nextButton.interactable = true;

        correctButton.interactable = false;
        wrongButton1.interactable = false;
        wrongButton2.interactable = false;
        wrongButton3.interactable = false;

        answerButton.gameObject.SetActive(true);

        btn.image.color = Color.red;

        nextButton.image.color = Color.white;

    }

    void CorrectClicked()
    {
        if (onVastattu) return;
        onVastattu = true;
        
        Pistemanageri.LisaaPisteita(10);

        correctButton.interactable = false;
        wrongButton1.interactable = false;
        wrongButton2.interactable = false;
        wrongButton3.interactable = false;

        correctButton.image.color = Color.green;

        nextText.color = Color.black;
        nextButton.image.color = Color.white;

        nextButton.onClick.RemoveAllListeners();
        
        nextButton.interactable = true;
    }

    void AnswerClicked()
    {
        if (answerbuttonText.text == "Paljasta oikea vastaus")
        {
            answerbuttonText.text = "Piilota oikea vastaus";
            answerButton.image.color = Color.grey;

            correctButton.image.color = Color.green;
        }

        else if (answerbuttonText.text == "Piilota oikea vastaus")
        {
            answerbuttonText.text = "Paljasta oikea vastaus";
            answerButton.image.color = Color.white;

            correctButton.image.color = Color.white;
        } 
       
    }

    void EndClicked()
    {
        Pistemanageri.TallennaPisteet();
    }
}
