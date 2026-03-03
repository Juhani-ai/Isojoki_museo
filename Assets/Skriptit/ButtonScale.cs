using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    public float scaleFactor = 1.05f;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * scaleFactor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }
}
