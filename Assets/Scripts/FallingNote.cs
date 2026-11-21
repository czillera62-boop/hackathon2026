using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class FallingNote : MonoBehaviour
{
    public enum MoveMode
    {
        Stepped, // krok na każdy beat (DOTween)
        Smooth   // płynny ruch zależny od pozycji w utworze
    }

    public enum NoteType
    {
        Good,
        Bad
    }

    /// <summary>
    /// Odpala się raz, gdy nuta typu Good spadnie poniżej missY bez trafienia.
    /// </summary>
    public static event System.Action<FallingNote> OnNoteMissed;

    [SerializeField] private MoveMode moveMode = MoveMode.Stepped;
    [SerializeField] private Ease stepEase = Ease.OutQuad;

    [Header("Miss")]
    [Tooltip("Poniżej tej wysokości (y) nuta znika, a nieuderzona nuta Good liczy się jako miss.")]
    [SerializeField] private float missY = 0f;

    [Header("Fade out (po miss)")]
    [Tooltip("Czas zanikania nuty po ominięciu (sekundy).")]
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.25f;

    [Tooltip("Do jakiej skali (mnożnik) nuta się zmniejsza podczas zanikania. 1 = bez zmiany.")]
    [SerializeField, Range(0f, 1f)] private float fadeEndScale = 0.7f;

    [field: SerializeField]
    public NoteType noteType { get; private set; }

    private int _hitBeat;
    private int _spawnBeat;
    private int _steps;
    private float _startY;
    private float _targetY;

    private Tween _tween;
    private bool _initialized;
    private bool _subscribed;
    private bool _missReported;
    private bool _fading;
    private Sequence _fadeSequence;

    public int HitBeat => _hitBeat;

    /// <summary>
    /// Wywoływane przez spawner zaraz po Instantiate (na beacie hitBeat - steps).
    /// </summary>
    public void Init(int hitBeat, int steps, float startY, float targetY)
    {
        _hitBeat = hitBeat;
        _steps = steps;
        _spawnBeat = hitBeat - steps;
        _startY = startY;
        _targetY = targetY;

        Vector3 p = transform.position;
        p.y = startY;
        transform.position = p;

        _initialized = true;

        if (moveMode == MoveMode.Stepped)
        {
            Conductor.Instance.OnBeatAhead += HandleBeatAhead;
            _subscribed = true;
        }
    }

    private void Update()
    {
        if (!_initialized || _fading)
            return;

        if (moveMode == MoveMode.Smooth)
        {
            float t = (float)((Conductor.Instance.SongPositionInBeats - _spawnBeat) / _steps);
            t = Mathf.Clamp(t, 0, Mathf.Infinity);

            Vector3 p = transform.position;
            p.y = _startY - (_startY - _targetY) * t;
            //p.y = Mathf.Lerp(_startY, _targetY, t);
            transform.position = p;
        }

        // Sprawdzamy realną pozycję nuty (także w trybie Stepped, w trakcie tweena).
        if (transform.position.y < missY)
        {
            ReportMiss();
            FadeOutAndDestroy();
        }
    }

    // Wywołane MoveDuration przed beatem "beat" - tween kończy się dokładnie na tym beacie.
    private void HandleBeatAhead(int beat)
    {
        int step = beat - _spawnBeat; // numer kroku: 1..steps
        /*if (step < 1 || step > _steps)
            return;*/

        // Cel liczony absolutnie, więc ewentualny poślizg nie kumuluje się.
        //float y = Mathf.Lerp(_startY, _targetY, (float)step / _steps);

        float y = _startY - (_startY - _targetY) * ((float)step / _steps);

        _tween?.Kill(true); // dokończ poprzedni krok (snap do jego celu)
        _tween = transform.DOMoveY(y, Conductor.Instance.MoveDuration).SetEase(stepEase);

        /*if (step == _steps)
            Unsubscribe();*/

    }

    // Zgłasza miss (tylko raz i tylko dla nut Good).
    private void ReportMiss()
    {
        if (_missReported)
            return;

        _missReported = true;

        if (noteType == NoteType.Good)
            OnNoteMissed?.Invoke(this);
    }

    // Animacja zanikania ominiętej nuty: przezroczystość + lekkie zmniejszenie, potem Destroy.
    private void FadeOutAndDestroy()
    {
        if (_fading)
            return;

        _fading = true;

        // Nuta przestaje się ruszać i nie da się jej już trafić.
        Unsubscribe();
        _tween?.Kill();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        _fadeSequence = DOTween.Sequence();

        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
            _fadeSequence.Join(sr.DOFade(0f, fadeDuration));

        _fadeSequence.Join(transform.DOScale(transform.localScale * fadeEndScale, fadeDuration).SetEase(Ease.InQuad));
        _fadeSequence.OnComplete(() => Destroy(gameObject));
    }

    public void DestroyNote(bool hitCorrectly)
    {
        if (hitCorrectly)
        {
            //Tu ladny efekt
        }

        Unsubscribe();
        Destroy(gameObject);
    }

    private void Unsubscribe()
    {
        if (!_subscribed)
            return;

        if (Conductor.Instance != null)
            Conductor.Instance.OnBeatAhead -= HandleBeatAhead;

        _subscribed = false;
    }

    private void OnDestroy()
    {
        Unsubscribe();
        _tween?.Kill();
        _fadeSequence?.Kill();
    }
}