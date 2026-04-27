using System.Collections;
using TMPro;
using UnityEngine;

public class MuistipeliScoreScript : MonoBehaviour
{
    [Header("Scoring")]
    [SerializeField] private int pointsPerPair = 10;

    [Header("Text")]
    [SerializeField] private string label = "Pisteet";

    [Header("UI (optional)")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Final Score (optional)")]
    [SerializeField] private TextMeshProUGUI finalScoreText;

    private int score;
    private int pairsFound;
    private Coroutine finalCountRoutine;
    private bool gameFinished;

    public void SetPointsPerPair(int points)
    {
        pointsPerPair = Mathf.Max(0, points);
    }

    public void ResetScore()
    {
        score = 0;
        pairsFound = 0;
        gameFinished = false;

        if (scoreText == null)
        {
            var scoreGO = GameObject.FindGameObjectWithTag("ScoreText");
            if (scoreGO != null)
            {
                scoreText = scoreGO.GetComponent<TextMeshProUGUI>();
            }
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "";
        }

        UpdateScoreText();
    }

    private void Awake()
    {
        scoreText = null;
        // Convenience: if not assigned in the Inspector, try to find the TMP text by tag.
        if (scoreText == null)
        {
            var scoreGO = GameObject.FindGameObjectWithTag("ScoreText");
            if (scoreGO != null)
            {
                scoreText = scoreGO.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    private void Start()
    {
        if (scoreText == null)
        {
            Debug.LogWarning("MuistipeliScoreScript: scoreText is not assigned. Score will update internally but won't be visible on UI.");
        }
    }

    public void OnPairMatched()
    {
        pairsFound++;
        score += Mathf.Max(0, pointsPerPair);
        UpdateScoreText();

        Debug.Log($"Pairs found: {pairsFound}, Score: {score}");
    }

    public int GetScore()
    {
        return score;
    }

    public int GetPairsFound()
    {
        return pairsFound;
    }

    public void OnGameFinished()
    {
        if (gameFinished) return;
        gameFinished = true;

        // If the final score text is on an end panel that starts disabled, make sure it becomes visible.
        if (finalScoreText != null && !finalScoreText.gameObject.activeInHierarchy)
        {
            finalScoreText.gameObject.SetActive(true);
        }

        if (finalCountRoutine != null)
        {
            StopCoroutine(finalCountRoutine);
            finalCountRoutine = null;
        }
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Kokonaispisteet: {score}";
        }
    }

    public void ShowCombinedTotal(int totalScore, string labelOverride = null)
    {
        if (finalScoreText == null) return;

        if (!finalScoreText.gameObject.activeInHierarchy)
        {
            finalScoreText.gameObject.SetActive(true);
        }

        string usedLabel = string.IsNullOrWhiteSpace(labelOverride)
            ? "Kokonaispisteet (kaikki tasot)"
            : labelOverride;

        finalScoreText.text = $"{usedLabel}: {Mathf.Max(0, totalScore)}";
    }

    private void UpdateScoreText()
    {
        if (scoreText == null) return;
        scoreText.text = $"Pisteet: {score}";
    }
}
