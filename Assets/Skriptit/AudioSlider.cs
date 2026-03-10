using UnityEngine;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private string volumeKey = "AudioVolume";

    void Start()
    {
        // Lataa tallennettu äänenvoimakkuus tai käytä oletusarvoa (1.0).
        float savedVolume = PlayerPrefs.GetFloat(volumeKey, 1.0f);
        audioSource.volume = savedVolume;
        volumeSlider.value = savedVolume;

        // Lisää kuuntelija sliderille.
        volumeSlider.onValueChanged.AddListener(SetVolume);

        Debug.Log("AudioSlider alustettu. Volume: " + savedVolume);
    }

    void SetVolume(float volume)
    {
        // Päivitä AudioSourcen äänenvoimakkuus.
        audioSource.volume = volume;

        // Tallenna äänenvoimakkuus PlayerPrefsillä.
        PlayerPrefs.SetFloat(volumeKey, volume);
        PlayerPrefs.Save();

        Debug.Log("Äänenvoimakkuus asetettu: " + volume);
    }
}
