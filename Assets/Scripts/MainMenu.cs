using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public const string LevelScene = "Level";
    public const string MenuScene = "Menu";

    void Start() { Time.timeScale = 1f; }

    public void Play() { SceneManager.LoadScene(LevelScene); }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
