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

    private int score;
    private int pairsFound;

    private void Awake()
    {
        // Convenience: if this script is attached to the same GameObject as the TMP text,
        // auto-wire it so the user doesn't have to drag the reference.
        if (scoreText == null)
        {
            scoreText = GetComponent<TextMeshProUGUI>();
        }
    }

    private void Start()
    {
        if (scoreText == null)
        {
            Debug.LogWarning("MuistipeliScoreScript: scoreText is not assigned. Score will update internally but won't be visible on UI.");
        }
        UpdateScoreText();
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

    private void UpdateScoreText()
    {
        if (scoreText == null) return;
        scoreText.text = $"{label}: {score}";
    }
}
