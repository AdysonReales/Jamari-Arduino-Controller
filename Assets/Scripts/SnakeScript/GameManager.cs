using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    [Header("UI Texts")]
    public TMP_Text scoreText;
    public TMP_Text highScoreText;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public Button restartButton;
    public Button backToMenuButton;
    public RectTransform triangle;

    [Header("Triangle Offset (X/Y for UI)")]
    public Vector2 triangleOffsetRestart = new Vector2(-50f, 0f);
    public Vector2 triangleOffsetMenu = new Vector2(-50f, 0f);

    private int score;
    private int highScore;
    private bool isGameOver = false;

    private Button[] gameOverButtons;
    private int currentButtonIndex = 0;

    private void Start()
    {
        // Load high score
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isGameOver || gameOverButtons == null || gameOverButtons.Length == 0)
            return;

        // Navigate Game Over buttons with WASD/Arrow
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentButtonIndex = (currentButtonIndex - 1 + gameOverButtons.Length) % gameOverButtons.Length;
            HighlightCurrentButton();
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentButtonIndex = (currentButtonIndex + 1) % gameOverButtons.Length;
            HighlightCurrentButton();
        }

        // Press button
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            gameOverButtons[currentButtonIndex].onClick.Invoke();
        }
    }

    private void HighlightCurrentButton()
    {
        if (gameOverButtons == null || gameOverButtons.Length == 0) return;

        Button selectedButton = gameOverButtons[currentButtonIndex];
        if (triangle != null && selectedButton != null)
        {
            RectTransform btnRect = selectedButton.GetComponent<RectTransform>();

            Vector2 offset = Vector2.zero;
            if (selectedButton == restartButton) offset = triangleOffsetRestart;
            else if (selectedButton == backToMenuButton) offset = triangleOffsetMenu;

            triangle.anchoredPosition = btnRect.anchoredPosition + offset;
        }

        // Optional: select button for EventSystem
        EventSystem.current.SetSelectedGameObject(selectedButton.gameObject);
    }

    public void AddScore(int points)
    {
        if (isGameOver) return;

        score += points;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        UpdateUI();
    }

    public void ResetScore()
    {
        score = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
        if (highScoreText != null) highScoreText.text = "High Score: " + highScore;
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        gameOverButtons = new Button[] { restartButton, backToMenuButton };
        currentButtonIndex = 0;

        HighlightCurrentButton();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    
}
