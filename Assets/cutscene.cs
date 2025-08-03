using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class cutscene : MonoBehaviour
{
    public RawImage image;
    public Texture[] scenes;
    public float fadeDuration = 1f;
    public float showDuration = 3f;

    public VideoPlayer videoPlayer;

    private bool videoFinished = false;

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished; // Video bittiğinde tetiklenecek fonksiyonu bağladık
        }

        StartCoroutine(PlayStory());
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        videoFinished = true;
    }

    IEnumerator PlayStory()
    {
        foreach (Texture tex in scenes)
        {
            image.texture = tex;
            yield return StartCoroutine(FadeImage(0f, 1f));
            yield return new WaitForSeconds(showDuration);
            yield return StartCoroutine(FadeImage(1f, 0f));
        }

        // 🎬 Video bitene kadar bekle
        if (videoPlayer != null)
        {
            while (!videoFinished)
            {
                yield return null;
            }
        }

        // Şimdi geçebiliriz
        SceneManager.LoadScene("day-1");
    }

    IEnumerator FadeImage(float from, float to)
    {
        float t = 0f;
        Color color = image.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / fadeDuration);
            image.color = new Color(color.r, color.g, color.b, a);
            yield return null;
        }
    }
}