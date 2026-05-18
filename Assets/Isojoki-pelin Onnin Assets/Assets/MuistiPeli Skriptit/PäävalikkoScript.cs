using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PäävalikkoScript : MonoBehaviour
{
    [Header("Scene to load")]
    [SerializeField] private string paavalikkoSceneName = "1. Päävalikko";

    [Header("Optional: auto-wire UI button")]
    [SerializeField] private Button paavalikkoNappi;

    private void Awake()
    {
        if (paavalikkoNappi != null)
        {
            paavalikkoNappi.onClick.AddListener(AvaaPaavalikko);
        }
    }

    // Hook this up to the Button's OnClick() event.
    public void AvaaPaavalikko()
    {
        if (string.IsNullOrWhiteSpace(paavalikkoSceneName))
        {
            Debug.LogError($"{nameof(PäävalikkoScript)}: Scene name is empty.");
            return;
        }

        SceneManager.LoadScene(paavalikkoSceneName);
    }
}
