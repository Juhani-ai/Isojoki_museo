using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
    public GameObject[] allPanels;
    public Image fadeOverlay;
    public float fadeDuration = 0.2f;

    // Popup-asetukset
    public float popupAnimDuration = 0.2f;
    public AnimationCurve popupScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private GameObject currentPanel;

    // 🔹 LISÄTTY
    private bool isAnimatingPopup = false;

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

    // Popup — ei koko ruudun tummennusta
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

    // 🔹 LISÄTTY: Toggle-toiminto
    public void TogglePopup(GameObject popup)
    {
        if (popup == null || isAnimatingPopup) return;

        if (popup.activeSelf)
            StartCoroutine(HidePopupSafe(popup));
        else
            StartCoroutine(ShowPopupSafe(popup));
    }

    // 🔹 LISÄTTY: turvalliset coroutinet
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

    // --- Koko ruudun siirtymä ---

    private IEnumerator TransitionTo(GameObject nextPanel)
    {
        yield return StartCoroutine(Fade(0f, 1f));

        yield return new WaitForSeconds(0.1f);

        if (currentPanel != null)
            currentPanel.SetActive(false);

        if (nextPanel != null)
            nextPanel.SetActive(true);

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

    // --- Popup animaatiot ---

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
        if (cg == null)
            cg = obj.AddComponent<CanvasGroup>();
        return cg;
    }
}