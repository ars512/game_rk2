using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject emberPrefab, gloomPrefab;
    public Image[] hearts;
    public Text scoreText, pauseTitle;
    public GameObject pausePanel, resumeButton;
    public Sprite heartFull, heartEmpty;
    public Moth moth;

    int lives = 3, score;
    bool over;
    float nextSpawn = 1f, invulnerableUntil;

    void Awake() { Instance = this; Time.timeScale = 1f; }
    void OnDestroy() { Time.timeScale = 1f; }

    void Start()
    {
        pausePanel.SetActive(false);
        Refresh();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && !over)
            if (pausePanel.activeSelf) Resume(); else Pause();

        if (over || pausePanel.activeSelf) return;
        nextSpawn -= Time.deltaTime;
        if (nextSpawn > 0) return;

        // чем больше очков, тем гуще тени (но не бесконечно)
        float difficulty = Mathf.Min(score / 300f, 1f);
        nextSpawn = Random.Range(0.9f, 1.6f) - 0.4f * difficulty;
        bool gloom = Random.value < 0.4f + 0.2f * difficulty;
        var prefab = gloom ? gloomPrefab : emberPrefab;
        float y = gloom ? moth.groundY : Random.Range(moth.groundY + 1.5f, moth.groundY + 5.5f);
        var d = Instantiate(prefab, new Vector3(14f, y, 0), Quaternion.identity).GetComponent<Drifter>();
        d.speed = Random.Range(4f, 6f) + 3f * difficulty;
    }

    public void AddScore(int n) { score += n; Refresh(); }

    public void Hurt()
    {
        if (Time.time < invulnerableUntil || over) return;
        invulnerableUntil = Time.time + 1f;
        lives--;
        Refresh();
        if (lives <= 0) GameOver();
    }

    void Refresh()
    {
        scoreText.text = "SCORE " + score;
        for (int i = 0; i < hearts.Length; i++) hearts[i].sprite = i < lives ? heartFull : heartEmpty;
    }

    void GameOver()
    {
        over = true;
        pauseTitle.text = "LIGHTS OUT  " + score;
        resumeButton.SetActive(false);
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Pause()
    {
        if (over) return;
        pauseTitle.text = "PAUSED";
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Restart() { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    public void ToMenu() { SceneManager.LoadScene(MainMenu.MenuScene); }
}
