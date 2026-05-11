using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioPlayer : MonoBehaviour
{
    public AudioClip clip;
    private AudioSource audioSource;
    private bool isPlaying = false;
    private Button button;
    private TMP_Text buttonText;
    private Image buttonImage;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = clip;

        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TMP_Text>();
        buttonImage = GetComponent<Image>();

        SetStopped();
    }

    void Update()
    {
        if (isPlaying && !audioSource.isPlaying)
        {
            SetStopped();
        }
    }

    public void ToggleAudio()
    {
        if (isPlaying)
        {
            audioSource.Stop();
            SetStopped();
        }
        else
        {
            audioSource.Play();
            SetPlaying();
        }
    }

    public void StopAudio()
    {
        SetStopped();
    }

    private void SetPlaying()
    {
        isPlaying = true;
        buttonText.text = "Sammuta audio";
        buttonImage.color = new Color(0.8f, 0.2f, 0.2f);
    }

    private void SetStopped()
    {
        isPlaying = false;
        if (audioSource != null) audioSource.Stop();
        if (buttonText != null) buttonText.text = "Soita audio";
        if (buttonImage != null) buttonImage.color = new Color(0.2f, 0.7f, 0.3f);
    }
}