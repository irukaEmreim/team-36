using UnityEngine;
using UnityEngine.SceneManagement;

public class menuscript : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("story");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
