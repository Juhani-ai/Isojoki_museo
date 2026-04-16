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

    private void Awake()
    {
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
        scoreText = null;
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Kokonaispisteet: {score}";
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText == null) return;
        scoreText.text = $"{label}: {score}";
    }
}
