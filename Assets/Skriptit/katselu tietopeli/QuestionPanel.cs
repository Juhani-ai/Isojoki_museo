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
    [SerializeField] private TMP_Text rightText;
    [SerializeField] private TMP_Text wrongText;
    [SerializeField] private Button answerButton;
    [SerializeField] private TMP_Text answerText;
    [SerializeField] private RawImage answerImage;
    [SerializeField] private TMP_Text answerbuttonText;

    void Start()
    {
 
        nextButton.interactable = false;

        correctButton.onClick.AddListener(CorrectClicked);
        wrongButton1.onClick.AddListener(() => WrongClicked(wrongButton1));
        wrongButton2.onClick.AddListener(() => WrongClicked(wrongButton2));
        wrongButton3.onClick.AddListener(() => WrongClicked(wrongButton3));
        nextButton.onClick.AddListener(() => Debug.Log("Toimii"));
        
        answerButton.onClick.AddListener(AnswerClicked);
       
    }

    void WrongClicked(Button btn)
    {
        nextButton.interactable = true;
    
        correctButton.gameObject.SetActive(false);
        wrongButton1.gameObject.SetActive(false);
        wrongButton2.gameObject.SetActive(false);
        wrongButton3.gameObject.SetActive(false);

        nextButton.image.color = Color.white;
        nextButton.onClick.AddListener(() => Debug.Log("Toimii"));

        wrongText.gameObject.SetActive(true);
        answerButton.gameObject.SetActive(true);
        answerText.gameObject.SetActive(true);
        answerImage.gameObject.SetActive(true);
    }

    void CorrectClicked()
    {
        rightText.gameObject.SetActive(true);

        nextButton.interactable = true;
        correctButton.gameObject.SetActive(false);
        wrongButton1.gameObject.SetActive(false);
        wrongButton2.gameObject.SetActive(false);
        wrongButton3.gameObject.SetActive(false);

        nextText.color = Color.black;
        nextButton.image.color = Color.white;
        rightText.gameObject.SetActive(true);

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() => Debug.Log("Toimii "));
    }

    void AnswerClicked()
    {
        if (answerbuttonText.text == "Paljasta oikea vastaus")
        {
            answerbuttonText.text = "Piilota oikea vastaus";
            answerButton.image.color = Color.red;

            answerImage.gameObject.SetActive(false);
        }

        else if (answerbuttonText.text == "Piilota oikea vastaus")
        {
            answerbuttonText.text = "Paljasta oikea vastaus";
            answerButton.image.color = Color.green;

            answerImage.gameObject.SetActive(true);
        }
       
    }
}
