using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using JetBrains.Annotations;

public class Tietovisa : MonoBehaviour
{
    public List<Kysymykset> questions;

    int currentQuestionIndex = 0;
    int lives = 3;
    int score = 0;
    bool hintUsed = false;

    public RawImage questionImage;
    public TMP_Text[] answerTexts;

    public TMP_Text hintText;

    public GameObject popupPanel;
    public TMP_Text popupText;

    Kysymykset currentQuestion;

    private void Start()
    {
        LoadQuestions();
    }

    private void LoadQuestions()
    {
        hintUsed = false;

        currentQuestion = questions[currentQuestionIndex];

        questionImage.texture = currentQuestion.image;

        for (int i = 0; i < 4; i++)
        {
            answerTexts[i].text = currentQuestion.answers[i];
        }
    }

    public void SelectAnswer(int index)
    {
        if (index == currentQuestion.correctAnswerIndex)
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
    }

    public void ShowHint()
    {
        hintUsed = true;
        hintText.text = currentQuestion.hint;
    }

    public void ShowCorrectPopup()
    {
        popupText.text = "Oikein!\n\n" + currentQuestion.infoText;
        popupPanel.SetActive(true);
    }

    public void ShowWrongPopup()
    {
        string correct = currentQuestion.answers[currentQuestion.correctAnswerIndex];

        popupText.text = "Väärin!\nOikea vastaus on: " + correct +
            "\n\n" + currentQuestion.infoText;

        popupPanel.SetActive(true);
    }
}
 