using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
 

public class TimeScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   [SerializeField] TextMeshProUGUI timerText;
   [SerializeField] float remainingTime;

    [Header("Game Over")]
    [SerializeField] private bool restartOnGameOver = true;
    [SerializeField] private float restartDelaySeconds = 2f;
    [SerializeField] private string gameOverMessage = "Peli ohi";
    [SerializeField] private MuistipeliScoreScript scoreScript;
    private bool gameOverTriggered;
    
private void Awake()
    {
        if (scoreScript == null)
        {
            scoreScript = GetComponent<MuistipeliScoreScript>();
        }

        if (scoreScript == null)
        {
            scoreScript = FindAnyObjectByType<MuistipeliScoreScript>();
        }

        if (scoreScript == null)
        {
            Debug.LogError("GameManager: No MuistipeliScoreScript found. Score will not increase.");
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (gameOverTriggered) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0)
        {
            remainingTime = 0;
            GameOver();
            return;
        }
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);

        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void GameOver()
    {
        gameOverTriggered = true;

        if (timerText != null)
        {
            timerText.color = Color.red;
            timerText.text = gameOverMessage;
        }
        Debug.Log("Final score: " + scoreScript.GetScore() + ", pairs found: " + scoreScript.GetPairsFound());
        scoreScript.OnGameFinished();
        if (restartOnGameOver)
        {
            StartCoroutine(RestartAfterDelay());
        }
    }

    private System.Collections.IEnumerator RestartAfterDelay()
    {
        float delay = Mathf.Max(0f, restartDelaySeconds);
        if (delay > 0f)
        {
            // Use realtime so restart still happens if Time.timeScale is changed elsewhere.
            yield return new WaitForSecondsRealtime(delay);
        }

        // Reload current scene (simple restart)
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}
