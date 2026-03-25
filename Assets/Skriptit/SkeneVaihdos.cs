using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Staattinen muuttuja säilyy, vaikka skripti tuhoutuisi skenevaihdossa
    private static string edellinenSkene = "1. PaaValikko";

    // Käytä tätä siirtymiseen (esim. Valikko -> Esineet)
    public void ChangeScene(string sceneName)
    {
        edellinenSkene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName);
        Debug.Log("Siirrytään kohtaukseen: " + sceneName + ". Muistetaan edellinen: " + edellinenSkene);
    }

    // "Älykäs" pakki-nappi
    public void GoBack()
    {
        Debug.Log("Palataan edelliseen kohtaukseen: " + edellinenSkene);
        SceneManager.LoadScene(edellinenSkene);
        
        // Nollataan muisti päävalikkoon paluun jälkeen, jottei jää "luuppia"
        if (edellinenSkene.Contains("PaaValikko")) 
        {
            edellinenSkene = "1. PaaValikko";
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("1. PaaValikko");
        Debug.Log("Palataan suoraan päävalikkoon.");
    }

    public void QuitApplication()
    {
        Debug.Log("Sovellus suljetaan...");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}

