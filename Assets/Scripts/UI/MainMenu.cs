using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    AudioManager audioManager;
    public static bool isPaused = false;
    public GameObject pauseMenuUI;

    void Awake()
    {
        var manager = GameObject.FindGameObjectWithTag("Audio");
        audioManager = manager.GetComponent<AudioManager>();
    }

    void Start()
    {
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ContinueGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {

        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Freeze game time
        isPaused = true;
    }

    public void ContinueGame()
    {
        if (audioManager != null)
            audioManager.PlaySFX(audioManager.button);
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Resume game time
        isPaused = false;
    }

    public void ReturnToMainMenu()
    {
        if (audioManager != null)
            audioManager.PlaySFX(audioManager.button);
        SceneManager.LoadScene("MainMenu");
    }
}
