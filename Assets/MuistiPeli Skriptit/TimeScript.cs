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
    private bool running;
    private bool gameOverTriggered;

    public event System.Action TimerExpired;
    
private void Start()
    {
        running = false;          // stay still until difficulty button pressed
        gameOverTriggered = false;
        UpdateTimerText();
    }

    public void StartTimer(float seconds)
    {
        remainingTime = Mathf.Max(0f, seconds);
        gameOverTriggered = false;
        running = true;

        if (timerText != null) timerText.color = Color.white;
        UpdateTimerText();
    }

    public void StopTimer()
    {
        running = false;
    }

    void Update()
    {
        if (!running || gameOverTriggered) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            UpdateTimerText();
            GameOver();
            return;
        }

        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        if (timerText != null)
            timerText.text = $"{minutes:00}:{seconds:00}";
    }

    // keep your existing GameOver() / RestartAfterDelay() as-is


    private void GameOver()
    {
        gameOverTriggered = true;

        TimerExpired?.Invoke();

        if (timerText != null)
        {
            timerText.color = Color.red;
            timerText.text = gameOverMessage;
        }
        
        if (scoreScript != null)
        {
            scoreScript.OnGameFinished();
        }
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
