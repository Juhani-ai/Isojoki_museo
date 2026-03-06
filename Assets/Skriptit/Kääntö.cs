using UnityEngine;

public class KorttiKaaanto : MonoBehaviour
{
    private bool onKaannetty = false;
    private float kaantoNopeus = 2f; // Asteita per sekunti.

    void OnMouseDown()
    {
        // Käännä korttia klikatessa.
        onKaannetty = !onKaannetty;
    }

    void Update()
    {
        // Pyörivät korttia sujuvasti.
        float kohdeKulma = onKaannetty ? 180f : 0f;
        float nykyinenKulma = transform.eulerAngles.y;
        float uusiKulma = Mathf.LerpAngle(nykyinenKulma, kohdeKulma, kaantoNopeus * Time.deltaTime);
        transform.eulerAngles = new Vector3(0, uusiKulma, 0);
    }
}
