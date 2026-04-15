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
    //[SerializeField] private string finalFormat = "Sait: {0} Pistettä!";
    //[SerializeField] private float finalCountStepDelay = 0.1f;

    private int score;
    private int pairsFound;
    private Coroutine finalCountRoutine;

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
        //UpdateScoreText();
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
        TextMeshProUGUI target = ResolveFinalTargetText();
        if (target == null)
        {
            Debug.LogWarning("MuistipeliScoreScript: No UI Text assigned for final score (finalScoreText/scoreText). Final score will not be visible.");
            return;
        }

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

        //string scoreTextValue = scoreText != null ? scoreText.text : $"{label}: {score}";
        target.text = $"Kokonaispisteet: {score}";
    }

    private TextMeshProUGUI ResolveFinalTargetText()
    {
        if (finalScoreText != null) return finalScoreText;
        if (scoreText != null) return scoreText;

        // Try to find a TMP text on this object or its children (including inactive).
        var local = GetComponentInChildren<TextMeshProUGUI>(true);
        if (local != null) return local;

        // As a last resort, search the scene for a likely score text.
        var allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (allTexts == null || allTexts.Length == 0) return null;

        TextMeshProUGUI best = null;
        int bestScore = int.MinValue;

        foreach (var t in allTexts)
        {
            if (t == null) continue;

            int s = 0;
            string n = t.gameObject.name.ToLowerInvariant();
            if (n.Contains("score")) s += 5;
            if (n.Contains("piste")) s += 5;
            if (n.Contains("pisteet")) s += 5;
            if (n.Contains("kokonais")) s += 3;
            if (n.Contains("final")) s += 3;
            if (n.Contains("loppu")) s += 3;
            if (t.GetComponentInParent<Canvas>() != null) s += 2;
            if (t.gameObject.activeInHierarchy) s += 1;

            if (s > bestScore)
            {
                bestScore = s;
                best = t;
            }
        }

        // Only use the heuristic match if it looked at least somewhat like a score text.
        return bestScore >= 5 ? best : null;
    }

    private void UpdateScoreText()
    {
        if (scoreText == null) return;
        scoreText.text = $"{label}: {score}";
    }
}
