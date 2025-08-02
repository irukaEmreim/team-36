using UnityEngine;
using System.Collections;
using NPC_Scripts;

public class DynamicMusicManager : MonoBehaviour
{
    public AudioSource musicSource;

    public AudioClip morningMusic; // Roaming + Sport + Breakfast
    public AudioClip poolMusic1;
    public AudioClip lunchMusic;
    public AudioClip poolMusic2;
    public AudioClip dinnerMusic;
    public AudioClip breakfastMusic;

    private GameTimeManager.DayActivity lastActivity;
    private Coroutine fadeCoroutine;

    void Start()
    {
        lastActivity = GameTimeManager.DayActivity.None;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
    }

    void Update()
    {
        var current = GameTimeManager.Instance.CurrentActivity;
        if (current != lastActivity)
        {
            lastActivity = current;
            SwitchMusicForActivity(current);
        }
    }

    void SwitchMusicForActivity(GameTimeManager.DayActivity activity)
    {
        AudioClip newClip = null;
        float currentTime = GameTimeManager.Instance.CurrentTime;

        // 0-2dk arası sabah müziği
        if (currentTime < 120f)
        {
            newClip = morningMusic;
        }
        else
        {
            switch (activity)
            {
                case GameTimeManager.DayActivity.Breakfast:
                    newClip = breakfastMusic; // Artık ayrı kahvaltı müziği
                    break;

                case GameTimeManager.DayActivity.PoolOrSit:
                    newClip = (currentTime < 360f) ? poolMusic1 : poolMusic2;
                    break;

                case GameTimeManager.DayActivity.Lunch:
                    newClip = lunchMusic;
                    break;

                case GameTimeManager.DayActivity.Dinner:
                    newClip = dinnerMusic;
                    break;

                default:
                    break;
            }
        }

        if (newClip != null && musicSource.clip != newClip)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeToNewClip(newClip));
        }
    }

    IEnumerator FadeToNewClip(AudioClip newClip)
    {
        float fadeOutDuration = 1.5f;
        float fadeInDuration = 1.5f;

        // Fade out
        for (float t = 0; t < fadeOutDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            yield return null;
        }

        musicSource.volume = 1f;
    }
}
