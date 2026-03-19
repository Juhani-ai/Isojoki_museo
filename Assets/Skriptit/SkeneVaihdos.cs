using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Metodi scenejen vaihtamiseen (esim. päävalikosta tarinoihin)
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        Debug.Log("Siirrytään kohtaukseen: " + sceneName);
    }

    // Metodi paluuseen päävalikkoon
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Scenes/1. PaaValikko");
        Debug.Log("Palataan päävalikkoon.");
    }

    // Metodi sovelluksen sulkemiseen
    public void QuitApplication()
    {
        Debug.Log("Sovellus suljetaan...");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Pysäytä Play-mode editorissa
        #else
            Application.Quit(); // Sulje sovellus buildissä
        #endif
    }
}
