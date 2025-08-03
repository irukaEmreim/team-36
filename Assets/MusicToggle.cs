using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MusicToggle : MonoBehaviour
{
    public AudioSource musicSource;
    public Image buttonImage;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    private bool isMuted = false;

    private void Awake()
    {
        if (FindObjectsOfType<MusicToggle>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(musicSource.gameObject); // BU SATIRI EKLE
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "testser")
        {
            // Oyun sahnesi açıldığında müziği durdur ve objeyi yok et
            musicSource.Stop();
            Destroy(musicSource.gameObject); // MusicPlayer objesini de sil
            Destroy(this.gameObject); // kontrol scriptini de sil
        }
    }


    public void ToggleMusic()
    {
        isMuted = !isMuted;
        musicSource.mute = isMuted;

        if (isMuted)
            buttonImage.sprite = musicOffSprite;
        else
            buttonImage.sprite = musicOnSprite;
    }
}