using DG.Tweening;
using UnityEngine;

/// <summary>
/// Pokazuje jeden z obrazów WiFi (wifi_loss_1/2/3) zale¿nie od punktów gracza.
/// Ka¿dy obraz to osobny obiekt (instancja pliku .ase) z w³asn¹ animacj¹ - skrypt tylko w³¹cza ten w³aœciwy.
/// Ten komponent wrzuæ na pusty obiekt-rodzic, którego dzieæmi s¹ wszystkie obrazy.
/// </summary>
public class WiFi : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;

    [Header("Obrazy")]
    [Tooltip("Pokazywany, dopóki punkty nie przekrocz¹ pierwszego progu. Puste = wtedy nic nie jest widoczne.")]
    [SerializeField] private GameObject startImage;

    [Tooltip("Obiekty w kolejnoœci progów: wifi_loss_1, wifi_loss_2, wifi_loss_3.")]
    [SerializeField] private GameObject[] stageImages;

    [Tooltip("Obraz i-ty w³¹cza siê, gdy punkty > próg i-ty.")]
    [SerializeField] private int[] thresholds = { 1000, 2000, 3000 };

    [Tooltip("Zaznaczone = obraz nigdy nie cofa siê po spadku punktów (brak migania na granicy progu).")]
    [SerializeField] private bool onlyGoUp = true;

    [Header("Efekt przy zmianie")]
    [SerializeField] private bool playPunch = true;
    [SerializeField, Min(0f)] private float punchScale = 0.2f;
    [SerializeField, Min(0.01f)] private float punchDuration = 0.3f;

    private int _currentStage = -1; // -1 = przed pierwszym progiem
    private bool _initialized;

    private void OnEnable()
    {
        if (playerStats == null)
        {
            Debug.LogError("WiFi: nie przypisano PlayerStats.", this);
            return;
        }

        playerStats.OnPointsChanged += HandlePointsChanged;
        Apply(GetStage(playerStats.punkty), false);
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnPointsChanged -= HandlePointsChanged;
    }

    private void HandlePointsChanged(int points)
    {
        Apply(GetStage(points), playPunch);
    }

    // Zwraca indeks najwy¿szego przekroczonego progu (-1, jeœli ¿adnego).
    private int GetStage(int points)
    {
        int maxStage = Mathf.Min(thresholds.Length, stageImages.Length) - 1;

        for (int i = maxStage; i >= 0; i--)
        {
            if (points > thresholds[i])
                return i;
        }
        return -1;
    }

    private void Apply(int stage, bool animate)
    {
        if (_initialized)
        {
            if (stage == _currentStage)
                return;

            if (onlyGoUp && stage < _currentStage)
                return;
        }

        _initialized = true;
        _currentStage = stage;

        if (startImage != null)
            startImage.SetActive(stage < 0);

        for (int i = 0; i < stageImages.Length; i++)
        {
            if (stageImages[i] != null)
                stageImages[i].SetActive(i == stage);
        }

        if (animate)
        {
            transform.DOComplete(); // dokoñcz poprzedni efekt, ¿eby skala siê nie rozjecha³a
            transform.DOPunchScale(Vector3.one * punchScale, punchDuration);
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}