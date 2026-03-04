using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonPulseEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    public float scaleFactor = 1.05f;
    public float pulseSpeed = 2.0f;

    private Coroutine pulseCoroutine;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseButton());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
        StartCoroutine(ReturnToOriginalScale());
    }

    private IEnumerator PulseButton()
    {
        while (true)
        {
            yield return StartCoroutine(SmoothScale(originalScale, originalScale * scaleFactor, pulseSpeed));
            yield return StartCoroutine(SmoothScale(originalScale * scaleFactor, originalScale, pulseSpeed));
        }
    }

    private IEnumerator ReturnToOriginalScale()
    {
        yield return StartCoroutine(SmoothScale(transform.localScale, originalScale, pulseSpeed * 1.5f));
    }

    private IEnumerator SmoothScale(Vector3 startScale, Vector3 endScale, float speed)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
    }
}
