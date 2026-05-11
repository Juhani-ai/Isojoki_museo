using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PanelManager : MonoBehaviour
{
    public GameObject loppuruutu;

    public GameObject[] allPanels;
    public Image fadeOverlay;
    public float fadeDuration = 0.2f;

    public float popupAnimDuration = 0.2f;
    public AnimationCurve popupScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private GameObject currentPanel;
    private bool isAnimatingPopup = false;

    // ── Scene-lataus ──────────────────────────────────────────────

    /// <summary>Lataa scenen nimen perusteella (fade-efektillä).</summary>
    
    void Start()
{
    // Aseta ensimmäinen aktiivinen panel currentPaneliksi
    foreach (GameObject panel in allPanels)
    {
        if (panel.activeSelf)
        {
            currentPanel = panel;
            break;
        }
    }
}
    public void LataaSceeni(string sceneName)
    {
        StartCoroutine(LataaSceeniCoroutine(sceneName));
    }

    /// <summary>Lataa scenen indeksin perusteella (fade-efektillä).</summary>
    public void LataaSceeni(int sceneIndex)
    {
        StartCoroutine(LataaSceeniCoroutine(sceneIndex));
    }

    private IEnumerator LataaSceeniCoroutine(string sceneName)
    {
        yield return StartCoroutine(Fade(0f, 1f));
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator LataaSceeniCoroutine(int sceneIndex)
    {
        yield return StartCoroutine(Fade(0f, 1f));
        SceneManager.LoadScene(sceneIndex);
    }

    // ── Paneelit ──────────────────────────────────────────────────

    public void ShowPanel(GameObject panel)
    {
        if (panel == null) return;
        if (panel == currentPanel) return;
        StartCoroutine(TransitionTo(panel));
    }

    public void HidePanel(GameObject panel)
    {
        if (panel == null) return;
        StartCoroutine(TransitionTo(null));
    }

    public void ShowPopup(GameObject popup)
    {
        if (popup == null) return;
        popup.SetActive(true);
        StartCoroutine(PopupAnimateIn(popup));
    }

    public void HidePopup(GameObject popup)
    {
        if (popup == null) return;
        StartCoroutine(PopupAnimateOut(popup));
    }

    public void TogglePopup(GameObject popup)
    {
        if (popup == null || isAnimatingPopup) return;
        if (popup.activeSelf)
            StartCoroutine(HidePopupSafe(popup));
        else
            StartCoroutine(ShowPopupSafe(popup));
    }

    private IEnumerator ShowPopupSafe(GameObject popup)
    {
        isAnimatingPopup = true;
        ShowPopup(popup);
        yield return new WaitForSeconds(popupAnimDuration);
        isAnimatingPopup = false;
    }

    private IEnumerator HidePopupSafe(GameObject popup)
    {
        isAnimatingPopup = true;
        HidePopup(popup);
        yield return new WaitForSeconds(popupAnimDuration);
        isAnimatingPopup = false;
    }

    public void NaytaSeuraavaAvattuPaneeli(GameObject nykyinen)
{
    List<string> avatut = EsineRekisteri.HaeAvatutEsineet();
    int nykyinenIndeksi = System.Array.IndexOf(allPanels, nykyinen);

    for (int i = nykyinenIndeksi + 1; i < allPanels.Length; i++)
    {
        QuestionPanel qp = allPanels[i].GetComponent<QuestionPanel>();
        string id = qp?.HaeEsineID();

        if (qp == null || string.IsNullOrEmpty(id) || 
            avatut.Contains(id) || 
            PlayerPrefs.GetInt("Unlocked_" + id, 0) == 1) // ← tarkistaa suoraan
        {
            ShowPanel(allPanels[i]);
            return;
        }
    }

    if (loppuruutu != null)
        ShowPanel(loppuruutu);
    else
        Debug.LogWarning("Loppuruutua ei ole asetettu PanelManagerissa!");
}

    private IEnumerator TransitionTo(GameObject nextPanel)
{
    yield return StartCoroutine(Fade(0f, 1f));
    yield return new WaitForSeconds(0.1f);

    if (currentPanel != null)
    {
        foreach (AudioPlayer ap in currentPanel.GetComponentsInChildren<AudioPlayer>())
        {
            ap.StopAudio();
        }
        currentPanel.SetActive(false);
    }

    if (nextPanel != null) nextPanel.SetActive(true);
    currentPanel = nextPanel;
    yield return StartCoroutine(Fade(1f, 0f));
}

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        Color c = fadeOverlay.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeOverlay.color = c;
            yield return null;
        }
        c.a = to;
        fadeOverlay.color = c;
    }

    private IEnumerator PopupAnimateIn(GameObject popup)
    {
        CanvasGroup cg = GetOrAddCanvasGroup(popup);
        float elapsed = 0f;
        while (elapsed < popupAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = popupScaleCurve.Evaluate(elapsed / popupAnimDuration);
            popup.transform.localScale = Vector3.Lerp(Vector3.one * 0.85f, Vector3.one, t);
            cg.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        popup.transform.localScale = Vector3.one;
        cg.alpha = 1f;
    }

    private IEnumerator PopupAnimateOut(GameObject popup)
    {
        CanvasGroup cg = GetOrAddCanvasGroup(popup);
        float elapsed = 0f;
        while (elapsed < popupAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = popupScaleCurve.Evaluate(elapsed / popupAnimDuration);
            popup.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.85f, t);
            cg.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        popup.SetActive(false);
        popup.transform.localScale = Vector3.one;
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        return cg;
    }
}