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

    int lives = 3;
    int score = 0;
    bool hintUsed = false;

    public RawImage questionImage;
    public Button[] answerButtons;
    public TMP_Text[] answerTexts;

    public TMP_Text hintText;

    public GameObject popupPanel;
    public TMP_Text popupText;

    private void Start()
    {
        items = new List<QuizItemData>(Resources.LoadAll<QuizItemData>("QuizItems"));

        GenerateQuestion();
    }

    public void GenerateQuestion()
    {
        hintUsed = false;

        correctItem = items[Random.Range(0, items.Count)];

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
    }

    public void SelectAnswer(int index)
    {
        if (answerItems[index] == correctItem)
        {
            int points = hintUsed ? 5 : 10;
            score += points;

            ShowCorrectPopup();
        }
        else
        {
            lives--;
            ShowWrongPopup();
        }
        Debug.Log("Clicked: " + answerItems[index].itemName);
        Debug.Log("Correct: " + correctItem.itemName);
    }

    public void ShowHint()
    {
        hintUsed = true;
        hintText.text = correctItem.hint;
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
        if (lives <= 0)
        {
            // GameOver();
            return;
        }

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
}
 