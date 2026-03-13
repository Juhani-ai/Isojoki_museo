using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;

    [Header("Musiikkiasetukset")]
    public AudioClip[] playlist;
    public float fadeDuration = 1.0f;
    public bool shuffle = true;

    [Header("UI-klikkausääni")]
    public AudioClip buttonClickSound;
    public Button[] buttonsWithClickSound;

    private AudioSource audioSource;
    private AudioSource uiAudioSource;
    private int currentIndex = -1;
    private Coroutine fadeRoutine;
    private bool isFading = false;

    void Awake()
    {
        // Singleton, joka säilyy scenevaihdon yli
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        uiAudioSource = gameObject.AddComponent<AudioSource>();

        // Lisää kuuntelijat napeille
        foreach (Button button in buttonsWithClickSound)
        {
            if (button != null)
                button.onClick.AddListener(PlayButtonClickSound);
        }
    }

    void Start()
    {
        PlayNextTrack();
    }

    void Update()
    {
        if (!audioSource.isPlaying && !isFading && audioSource.clip != null)
            PlayNextTrack();
    }

    public void PlayNextTrack()
    {
        if (playlist == null || playlist.Length == 0) return;

        int newIndex;
        if (shuffle)
        {
            do { newIndex = Random.Range(0, playlist.Length); }
            while (playlist.Length > 1 && newIndex == currentIndex);
        }
        else
        {
            newIndex = (currentIndex + 1) % playlist.Length;
        }

        currentIndex = newIndex;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeOutAndPlayNewTrack(playlist[currentIndex]));
    }

    IEnumerator FadeOutAndPlayNewTrack(AudioClip nextTrack)
    {
        isFading = true;
        float startVolume = audioSource.volume;
        float t = 0f;

        // Fade out
        while (t < fadeDuration && audioSource.isPlaying)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = nextTrack;
        audioSource.volume = startVolume;
        audioSource.Play();

        isFading = false;
    }

    public void ToggleShuffle()
    {
        shuffle = !shuffle;
    }

    public void PlayButtonClickSound()
    {
        uiAudioSource.PlayOneShot(buttonClickSound);
    }
}
