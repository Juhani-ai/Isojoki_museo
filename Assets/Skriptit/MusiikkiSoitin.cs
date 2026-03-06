using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MusicPlayer : MonoBehaviour
{
    public AudioClip[] playlist;
    public float fadeDuration = 1.0f;
    public bool shuffle = false;

    private AudioSource audioSource;
    private List<int> shuffleIndices = new List<int>();
    private int currentTrackIndex = 0;
    private bool isFading = false;
    private Coroutine fadeCoroutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        InitializeShuffle();
        PlayTrack(currentTrackIndex);
    }

    void InitializeShuffle()
    {
        shuffleIndices.Clear();
        for (int i = 0; i < playlist.Length; i++)
            shuffleIndices.Add(i);
        ShuffleList(shuffleIndices);
    }

    void ShuffleList(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    void PlayTrack(int index)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutThenIn(playlist[index]));
    }

    IEnumerator FadeOutThenIn(AudioClip newClip)
    {
        isFading = true;
        float startVolume = audioSource.volume;

        // Fade out
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in
        while (audioSource.volume < startVolume)
        {
            audioSource.volume += startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        isFading = false;
    }

    public void NextTrack()
    {
        if (isFading) return;

        if (shuffle)
        {
            if (shuffleIndices.Count == 0)
                InitializeShuffle();
            currentTrackIndex = shuffleIndices[0];
            shuffleIndices.RemoveAt(0);
        }
        else
        {
            currentTrackIndex = (currentTrackIndex + 1) % playlist.Length;
        }

        PlayTrack(currentTrackIndex);
    }

    public void ToggleShuffle()
    {
        shuffle = !shuffle;
        InitializeShuffle();
    }
}
