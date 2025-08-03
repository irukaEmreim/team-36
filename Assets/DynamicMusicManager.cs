using UnityEngine;
using System.Collections;

public class DynamicMusicManager : MonoBehaviour
{
    public AudioSource musicSource;

    public AudioClip morningMusic;
    public AudioClip breakfastMusic;
    public AudioClip poolMusic1;
    public AudioClip lunchMusic;
    public AudioClip dinnerMusic;
    public AudioClip poolMusic2;

    public float timer = 0f;
    private Coroutine fadeCoroutine;
    private int currentPhase = -1; // -1 başlatıcı değer

    void Start()
    {
        musicSource.loop = true;
        musicSource.playOnAwake = false;


        AudioClip firstClip = GetClipForPhase(0);
    if (firstClip != null)
    {
        musicSource.clip = firstClip;
        musicSource.volume = 1f;
        musicSource.Play();
        currentPhase = 0;
        Debug.Log("Başlangıç müziği başlatıldı: " + firstClip.name);
    }

    }

    void Update()
    {
        timer += Time.deltaTime;
        SwitchMusicForActivity();
    }

    void SwitchMusicForActivity()
    {
        int newPhase = GetCurrentPhase();

        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;

            AudioClip newClip = GetClipForPhase(currentPhase);
            if (newClip != null && musicSource.clip != newClip)
            {
                if (fadeCoroutine != null)
                    StopCoroutine(fadeCoroutine);

                fadeCoroutine = StartCoroutine(FadeToNewClip(newClip));
            }
        }
    }

    int GetCurrentPhase()
    {
        float t = timer;

        if (t < 120f) return 0;
        else if (t < 180f) return 1;
        else if (t < 240f) return 2;
        else if (t < 300f) return 3;
        else if (t < 360f) return 4;
        else if (t < 480f) return 5;
        else return 0;
    }

    AudioClip GetClipForPhase(int phase)
    {
        switch (phase)
        {
            case 0: return morningMusic;
            case 1: return breakfastMusic;
            case 2: return poolMusic1;
            case 3: return lunchMusic;
            case 4: return dinnerMusic;
            case 5: return poolMusic2;
            default: return null;
        }
    }

    IEnumerator FadeToNewClip(AudioClip newClip)
    {
        float fadeOutDuration = 1.5f;
        float fadeInDuration = 1.5f;

        // Fade out
        for (float t = 0; t < fadeOutDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0.2f, 0f, t / fadeOutDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, 0.2f, t / fadeInDuration);
            yield return null;
        }

        musicSource.volume = 0.2f;
    }
}
