using UnityEngine;
using UnityEngine.UI;

public class MusicToggle : MonoBehaviour
{
    public AudioSource musicSource;
    public Image buttonImage;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    private bool isMuted = false;

    public void ToggleMusic()
    {
        isMuted = !isMuted;
        musicSource.mute = isMuted;

        // İkonu değiştir
        if (isMuted)
            buttonImage.sprite = musicOffSprite;
        else
            buttonImage.sprite = musicOnSprite;
    }
}