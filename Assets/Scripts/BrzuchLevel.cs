using DG.Tweening;
using UnityEngine;

/// <summary>
/// Zmienia wygl¹d brzucha zale¿nie od punktów gracza (analogicznie do WiFi).
/// Ka¿dy etap brzucha to osobny obiekt na scenie - skrypt w³¹cza ten w³aœciwy.
/// Progi mog¹ pochodziæ ze skryptu WiFi (wspólne progi) albo z w³asnej tablicy.
/// Ten sam obiekt mo¿e byæ jednoczeœnie w Start Image i w Stage Images - to nie psuje widocznoœci.
/// </summary>
public class BrzuchLevel : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;

    [Header("Progi")]
    [Tooltip("Jeœli przypisane, brzuch u¿ywa progów z tego komponentu WiFi (wspólne progi). Puste = w³asne progi poni¿ej.")]
    [SerializeField] private WiFi thresholdsSource;

    [Tooltip("W³asne progi - u¿ywane tylko, gdy Thresholds Source jest puste.")]
    [SerializeField] private int[] ownThresholds = { 1000, 2000, 3000 };

    [Header("Obrazy brzucha")]
    [Tooltip("Pokazywany, dopóki punkty nie przekrocz¹ pierwszego progu. Puste = wtedy nic nie jest widoczne.")]
    [SerializeField] private GameObject startImage;

    [Tooltip("Obiekty NA SCENIE (nie z okna Project) w kolejnoœci progów.")]
    [SerializeField] private GameObject[] stageImages;

    [Tooltip("Zaznaczone = brzuch nigdy siê nie zmniejsza po spadku punktów.")]
    [SerializeField] private bool onlyGoUp = false;

    [Header("Skalowanie (opcjonalne)")]
    [Tooltip("Obiekt skalowany wraz z punktami. Puste = skalowanie wy³¹czone.")]
    [SerializeField] private Transform scaleTarget;
    [SerializeField] private int scaleMinPoints = 0;
    [SerializeField] private int scaleMaxPoints = 3000;
    [SerializeField] private float scaleMin = 1f;
    [SerializeField] private float scaleMax = 1.5f;
    [SerializeField, Min(0.01f)] private float scaleDuration = 0.25f;

    [Header("Efekt przy zmianie etapu")]
    [SerializeField] private bool playPunch = true;
    [SerializeField, Min(0f)] private float punchScale = 0.2f;
    [SerializeField, Min(0.01f)] private float punchDuration = 0.3f;

    private int _currentStage = -1; // -1 = przed pierwszym progiem
    private bool _initialized;
    private Vector3 _baseScale = Vector3.one;

    private int[] Thresholds
    {
        get
        {
            if (thresholdsSource != null && thresholdsSource.Thresholds != null)
                return thresholdsSource.Thresholds;
            return ownThresholds;
        }
    }

    private void Awake()
    {
        if (scaleTarget != null)
            _baseScale = scaleTarget.localScale;
    }

    private void OnEnable()
    {
        if (playerStats == null)
        {
            Debug.LogError("BrzuchLevel: nie przypisano PlayerStats.", this);
            return;
        }

        playerStats.OnPointsChanged += HandlePointsChanged;
        ApplyStage(GetStage(playerStats.punkty), false);
        ApplyScale(playerStats.punkty, false);
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnPointsChanged -= HandlePointsChanged;
    }

    private void HandlePointsChanged(int points)
    {
        ApplyStage(GetStage(points), playPunch);
        ApplyScale(points, true);
    }

    // Zwraca indeks najwy¿szego przekroczonego progu (-1, jeœli ¿adnego).
    private int GetStage(int points)
    {
        int[] thresholds = Thresholds;
        int maxStage = Mathf.Min(thresholds.Length, stageImages.Length) - 1;

        for (int i = maxStage; i >= 0; i--)
        {
            if (points > thresholds[i])
                return i;
        }
        return -1;
    }

    private void ApplyStage(int stage, bool animate)
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

        // Który obiekt ma byæ widoczny.
        GameObject active = stage < 0 ? startImage : (stage < stageImages.Length ? stageImages[stage] : null);

        // Najpierw wy³¹cz wszystkie, potem w³¹cz w³aœciwy - dziêki temu ten sam obiekt
        // wpisany w kilka pól nie wy³¹czy siê przypadkiem po w³¹czeniu.
        if (startImage != null)
            startImage.SetActive(false);

        foreach (GameObject img in stageImages)
        {
            if (img != null)
                img.SetActive(false);
        }

        if (active != null)
            active.SetActive(true);

        if (animate)
        {
            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * punchScale, punchDuration);
        }
    }

    private void ApplyScale(int points, bool animate)
    {
        if (scaleTarget == null || scaleMaxPoints <= scaleMinPoints)
            return;

        float t = Mathf.InverseLerp(scaleMinPoints, scaleMaxPoints, points);
        Vector3 target = _baseScale * Mathf.Lerp(scaleMin, scaleMax, t);

        scaleTarget.DOKill();
        if (animate)
            scaleTarget.DOScale(target, scaleDuration).SetEase(Ease.OutQuad);
        else
            scaleTarget.localScale = target;
    }

    private void OnDestroy()
    {
        transform.DOKill();
        if (scaleTarget != null)
            scaleTarget.DOKill();
    }
}
