using System.Collections;
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
    [SerializeField] private int[] thresholds = { 1000, 2000 };

    /// <summary>Progi punktów - u¿ywane te¿ przez BrzuchLevel.</summary>
    public int[] Thresholds => thresholds;

    [Tooltip("Zaznaczone = obraz nigdy nie cofa siê po spadku punktów (brak migania na granicy progu).")]
    [SerializeField] private bool onlyGoUp = true;

    [Header("Animacja obrazu")]
    [Tooltip("Zaznaczone = animacja z pliku .ase zagra RAZ w momencie zmiany, po czym obraz zostaje widoczny na pierwszej klatce. " +
             "Odznaczone = animacja zapêtla siê bez koñca (wifi_loss_3 ma wtedy puste klatki, wiêc bêdzie znikaæ).")]
    [SerializeField] private bool playOnce = true;

    [Header("Efekt przy zmianie")]
    [SerializeField] private bool playPunch = true;
    [SerializeField, Min(0f)] private float punchScale = 0.2f;
    [SerializeField, Min(0.01f)] private float punchDuration = 0.3f;

    private int _currentStage = -1; // -1 = przed pierwszym progiem
    private bool _initialized;
    private Coroutine _playOnceRoutine;

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

        if (_playOnceRoutine != null)
        {
            StopCoroutine(_playOnceRoutine);
            _playOnceRoutine = null;
        }

        if (startImage != null)
            startImage.SetActive(stage < 0);

        for (int i = 0; i < stageImages.Length; i++)
        {
            if (stageImages[i] != null)
                stageImages[i].SetActive(i == stage);
        }

        if (playOnce && stage >= 0 && stageImages[stage] != null)
        {
            Animator anim = stageImages[stage].GetComponentInChildren<Animator>(true);
            if (anim != null)
                _playOnceRoutine = StartCoroutine(PlayOnceAndFreeze(anim));
        }

        if (animate)
        {
            transform.DOComplete(); // dokoñcz poprzedni efekt, ¿eby skala siê nie rozjecha³a
            transform.DOPunchScale(Vector3.one * punchScale, punchDuration);
        }
    }

    // Odtwarza animacjê od pocz¹tku jeden raz, a potem zatrzymuje j¹ na pierwszej klatce.
    private IEnumerator PlayOnceAndFreeze(Animator anim)
    {
        anim.speed = 1f; // po poprzednim zatrzymaniu (speed = 0) trzeba odblokowaæ animacjê
        yield return null; // poczekaj jedn¹ klatkê, a¿ Animator wczyta stan po w³¹czeniu obiektu

        if (anim == null || !anim.gameObject.activeInHierarchy)
            yield break;

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(info.length);

        if (anim == null || !anim.gameObject.activeInHierarchy)
            yield break;

        anim.Play(info.fullPathHash, 0, 0f); // wróæ na pierwsz¹ klatkê
        anim.Update(0f);
        anim.speed = 0f;                     // i j¹ zamroŸ
        _playOnceRoutine = null;
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}