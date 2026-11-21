using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ScoreUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private PlayerStats playerStats;

    [Header("Score Label Settings")]
    [SerializeField] private string scoreLabelName = "ScoreLabel";
    [SerializeField] private string prefix = "Score: ";

    [Header("Restart Button Settings")]
    [SerializeField] private string restartButtonName = "RestartButton";

    private Label _scoreLabel;
    private Button _restartButton;
    private bool _isGameOver;

    private void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null || playerStats == null)
        {
            Debug.LogError("ScoreUI: Missing UIDocument or PlayerStats reference!", this);
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;

        // 1. Setup Score Label
        _scoreLabel = root.Q<Label>(scoreLabelName);
        if (_scoreLabel == null)
            Debug.LogError($"ScoreUI: Could not find Label '{scoreLabelName}' in UXML!", this);

        // 2. Setup Restart Button
        _restartButton = root.Q<Button>(restartButtonName);
        if (_restartButton != null)
        {
            _restartButton.style.display = DisplayStyle.None;
            _restartButton.clicked += RestartGame;
        }
        else
        {
            Debug.LogError($"ScoreUI: Could not find Button '{restartButtonName}' in UXML!", this);
        }

        // Subscribe to score changes and initialize UI
        playerStats.OnPointsChanged += HandlePointsChanged;
        HandlePointsChanged(playerStats.punkty);
    }

    private void OnDisable()
    {
        if (_restartButton != null)
            _restartButton.clicked -= RestartGame;

        if (playerStats != null)
            playerStats.OnPointsChanged -= HandlePointsChanged;
    }

    private void HandlePointsChanged(int currentPoints)
    {
        if (_scoreLabel != null)
            _scoreLabel.text = $"{prefix}{currentPoints}";

        if (!_isGameOver && currentPoints < 0)
            TriggerGameOver();
    }

    private void TriggerGameOver()
    {
        _isGameOver = true;

        if (_restartButton != null)
            _restartButton.style.display = DisplayStyle.Flex;

        // Freeze gameplay, tweens, and audio (pauses Conductor's dspTime)
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}