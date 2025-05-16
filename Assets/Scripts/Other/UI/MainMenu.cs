using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    AudioManager audioManager;

    void Awake()
    {
        var manager = GameObject.FindGameObjectWithTag("Audio");

        if(manager != null)
            audioManager = manager.GetComponent<AudioManager>();
    }
    public void PlayGame()
    {
        if (audioManager != null)
            audioManager.PlaySFX(audioManager.button);
        SceneManager.LoadSceneAsync("Main");
    }

    public void QuitGame()
    {
        if (audioManager != null)
            audioManager.PlaySFX(audioManager.button);
        Application.Quit();
    }
}

