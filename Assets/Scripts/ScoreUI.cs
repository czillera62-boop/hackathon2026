using UnityEngine;
using UnityEngine.UIElements;

public class ScoreUI : MonoBehaviour
{
    [Header("Referencje")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private PlayerStats playerStats;

    [Header("Ustawienia UI Toolkit")]
    [Tooltip("Dokładna nazwa (Name) elementu Label wpisana w UI Builderze.")]
    [SerializeField] private string scoreLabelName = "ScoreLabel";
    [SerializeField] private string prefix = "Score: ";

    private Label _scoreLabel;

    private void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null || playerStats == null)
        {
            Debug.LogError("ScoreUI: Brak przypisanego UIDocument lub PlayerStats!", this);
            return;
        }

        // Pobranie korzenia UI i wyszukanie Labela po nazwie
        VisualElement root = uiDocument.rootVisualElement;
        _scoreLabel = root.Q<Label>(scoreLabelName);

        if (_scoreLabel == null)
        {
            Debug.LogError($"ScoreUI: Nie znaleziono elementu Label o nazwie '{scoreLabelName}' w dokumencie UXML!", this);
            return;
        }

        // Podpięcie się pod event zmiany punktów z Twojego skryptu PlayerStats
        playerStats.OnPointsChanged += UpdateScoreText;

        // Ustawienie początkowej wartości przy starcie
        UpdateScoreText(playerStats.punkty);
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnPointsChanged -= UpdateScoreText;
    }

    private void UpdateScoreText(int currentPoints)
    {
        if (_scoreLabel != null)
            _scoreLabel.text = $"{prefix}{currentPoints}";
    }
}