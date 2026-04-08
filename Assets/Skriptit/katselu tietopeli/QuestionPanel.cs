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
    [SerializeField] private TMP_Text rightText;

    void Start()
    {
    
        nextButton.gameObject.SetActive(false);
        rightText.gameObject.SetActive(false);

        correctButton.onClick.AddListener(CorrectClicked);
        wrongButton1.onClick.AddListener(() => WrongClicked(wrongButton1));
        wrongButton2.onClick.AddListener(() => WrongClicked(wrongButton2));
        wrongButton3.onClick.AddListener(() => WrongClicked(wrongButton3));
       
    }

    void WrongClicked(Button btn)
    {
        btn.image.color = Color.red;
        btn.interactable = false;
    }

    void CorrectClicked()
    {
        correctButton.gameObject.SetActive(false);
        wrongButton1.gameObject.SetActive(false);
        wrongButton2.gameObject.SetActive(false);
        wrongButton3.gameObject.SetActive(false);

        nextButton.gameObject.SetActive(true);
        rightText.gameObject.SetActive(true);

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() => Debug.Log("Seuraava vaihe!"));
    }
}
