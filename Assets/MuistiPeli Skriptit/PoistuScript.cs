/*
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoistuScript : MonoBehaviour
{
    [Header("Where to go when exiting the minigame")]
    [SerializeField] private string exitSceneName = "YourNewSceneName";

    [Header("Things to stop (assign in Inspector)")]
    [SerializeField] private TimeScript timeScript;
    [SerializeField] private MuistipeliScoreScript scoreScript;
    [SerializeField] private GameManager gameManager;

    public void ExitToScene()
    {
        // Stop timer + prevent game over restart logic from firing
        if (timeScript != null)
        {
            timeScript.StopTimer();
            timeScript.StopAllCoroutines();
            timeScript.enabled = false;
        }

        // Stop scoring updates
        if (scoreScript != null)
            scoreScript.enabled = false;

        // Optional: stop input/game logic immediately
        if (gameManager != null)
            gameManager.enabled = false;

        SceneManager.LoadScene(exitSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
*/