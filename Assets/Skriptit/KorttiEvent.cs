using UnityEngine;

public class KorttiEventit : MonoBehaviour
{
    public GameObject rawImage; // Vedä RawImage tähän Inspectorissa.
    public GameObject textTMP;  // Vedä TextMeshPro tähän Inspectorissa.

    public void PiilotaElementit()
    {
        rawImage.SetActive(false);
        textTMP.SetActive(false);
    }

    public void NaytaElementit()
    {
        rawImage.SetActive(true);
        textTMP.SetActive(true);
    }
}
