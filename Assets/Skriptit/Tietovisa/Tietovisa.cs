using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.Android;

public class Tietovisa : MonoBehaviour
{
    public List<QuizItemData> items;
    QuizItemData correctItem;
    List<QuizItemData> answerItems = new List<QuizItemData>();
    List<QuizItemData> remainingItems = new List<QuizItemData>();

    int lives = 3;
    int score = 0;
    bool hintUsed = false;
    public TMP_Text scoreText;
    public TMP_Text livesText;

    public RawImage questionImage;
    public Button[] answerButtons;
    public TMP_Text[] answerTexts;

    public GameObject nextButton;
    public GameObject replayButton;
    public GameObject menuButton;

    public GameObject hintButton;
    public TMP_Text hintText;

    public GameObject popupPanel;
    public TMP_Text popupText;

    int maxQuestions = 10;
    int questionsAsked = 0;

    private void Start()
    {
        items = new List<QuizItemData>(Resources.LoadAll<QuizItemData>("QuizItems"));

        remainingItems = new List<QuizItemData>(items);

        UpdateUI();
    }

    public void StartGame()
    {
        GenerateQuestion();
    }

    public void GenerateQuestion()
    {
        hintUsed = false;

        int randIndex = Random.Range(0, remainingItems.Count);
        correctItem = remainingItems[randIndex];

        remainingItems.RemoveAt(randIndex);

        questionImage.texture = correctItem.image;

        answerItems.Clear();
        answerItems.Add(correctItem);



        while (answerItems.Count < 4)
        {
            QuizItemData randomItem = items[Random.Range(0,items.Count)];

            if (!answerItems.Contains(randomItem))
            {
                answerItems.Add(randomItem);
            }
        }

        Shuffle(answerItems);

        for (int i = 0; i < 4; i++)
        {
            int index = i;

            answerTexts[i].text = answerItems[i].itemName;

            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => SelectAnswer(index));
        }

        hintText.text = "";
        hintButton.SetActive(true);
    }

    public void SelectAnswer(int index)
    {
        if (answerItems[index] == correctItem)
        {
            int points = hintUsed ? 10 : 15;
            score += points;

            Pistemanageri.LisaaPisteita(score);

            ShowCorrectPopup();
        }
        else
        {
            lives--;
            lives = Mathf.Max(0, lives);
            ShowWrongPopup();
        }

        UpdateUI(); 

        Debug.Log("Clicked: " + answerItems[index].itemName);
        Debug.Log("Correct: " + correctItem.itemName);
    }

    public void ShowHint()
    {
        hintUsed = true;
        hintText.text = correctItem.hint;
        hintButton.SetActive(false);
    }

    public void ShowCorrectPopup()
    {
        popupText.text = "Oikein!\n\n" + correctItem.info;
        popupPanel.SetActive(true);
    }

    public void ShowWrongPopup()
    {
        string correct = correctItem.itemName;

        popupText.text = "V‰‰rin!\nOikea vastaus on: " + correct +
            "\n\n" + correctItem.info;

        popupPanel.SetActive(true);
    }

    public void NextQuestion()
    {
        popupPanel.SetActive(false);

        if (lives <= 0)
        {
            Lose();
            Debug.Log("Lose");
            return;
        }

        if (questionsAsked >= maxQuestions)
        {
            Win();
            Debug.Log("Win (reached question limit");
            return;
        }

        if (remainingItems.Count == 0)
        {
            Win();
            Debug.Log("Win (no more questions");
            return;
        }
        questionsAsked++;
        GenerateQuestion();
    }

    void Shuffle(List<QuizItemData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);

            QuizItemData temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    void UpdateUI()
    {
        scoreText.text = $"Pisteet: {score}";
        livesText.text = $"Elamat: {lives}";
    }

    public void Lose()
    {
        popupText.text = $"Voi harmi, elamat loppui.\n\nPisteet: {score}";
        popupPanel.SetActive(true);
        nextButton.SetActive(false);
        replayButton.SetActive(true);
        menuButton.SetActive(true);
    }

    public void Win()
    {
        popupText.text = $"Onneksi olkoon!\n\nPisteet: {score}";
        popupPanel.SetActive(true);
        nextButton.SetActive(false);
        replayButton.SetActive(true);
        menuButton.SetActive(true);
    }

    public void Replay()
    {
        Pistemanageri.TallennaPisteet();
        ResetGame();
        GenerateQuestion();
    }
    public void ReturnToMenu()
    {
        Pistemanageri.TallennaPisteet();
        ResetGame();
    }

    void ResetGame()
    {
        score = 0;
        lives = 3;
        questionsAsked = 0;

        items = new List<QuizItemData>(Resources.LoadAll<QuizItemData>("QuizItems"));
        remainingItems = new List<QuizItemData>(items);

        UpdateUI();

        nextButton.SetActive(true);
        replayButton.SetActive(false);
        menuButton.SetActive(false);
        popupPanel.SetActive(false);
    }
}
 