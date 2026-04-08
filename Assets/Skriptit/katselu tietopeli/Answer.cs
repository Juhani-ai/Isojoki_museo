using UnityEngine;
using UnityEngine.UI;

public class Answer : MonoBehaviour
{
    [SerializeField] private Text questionText;

    // Ensimmäiset neljä nappia
    [SerializeField] private Button wrongButton1;
    [SerializeField] private Button wrongButton2;
    [SerializeField] private Button wrongButton3;
    [SerializeField] private Button correctButton;

    // Uusi nappi ja uusi teksti (näkyvät oikean jälkeen)
    [SerializeField] private Button nextButton;
    [SerializeField] private Text nextText;

    void Start()
    {
        // Aluksi vain kysymys ja neljä nappia näkyvissä
        nextButton.gameObject.SetActive(false);
        nextText.gameObject.SetActive(false);

        // Lisätään nappeihin click-kuuntelijat
        wrongButton1.onClick.AddListener(() => WrongClicked(wrongButton1));
        wrongButton2.onClick.AddListener(() => WrongClicked(wrongButton2));
        wrongButton3.onClick.AddListener(() => WrongClicked(wrongButton3));
        correctButton.onClick.AddListener(CorrectClicked);
    }

    void WrongClicked(Button btn)
    {
        // Väärä nappi muuttuu punaiseksi
        btn.image.color = Color.red;
        btn.interactable = false; // estetään uudelleen klikkaaminen
    }

    void CorrectClicked()
    {
        // Piilotetaan vanhat napit
        wrongButton1.gameObject.SetActive(false);
        wrongButton2.gameObject.SetActive(false);
        wrongButton3.gameObject.SetActive(false);
        correctButton.gameObject.SetActive(false);

        // Näytetään uusi nappi ja teksti
        nextButton.gameObject.SetActive(true);
        nextText.gameObject.SetActive(true);

        nextText.text = "Oikein! Tässä uusi vaihtoehto.";
        nextButton.GetComponentInChildren<Text>().text = "Jatka";
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() => Debug.Log("Seuraava vaihe!"));
    }
}