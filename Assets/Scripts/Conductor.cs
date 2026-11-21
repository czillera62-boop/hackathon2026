using System;
using UnityEngine;

/// <summary>
/// Trzyma rytm gry w oparciu o zegar audio (AudioSettings.dspTime).
/// Beat 0 = moment, w którym zaczyna się muzyka (+ firstBeatOffset).
/// Beaty przed muzyką (pre-roll) mają indeksy ujemne: -stepsToTarget ... -1.
/// </summary>
public class Conductor : MonoBehaviour
{
    public static Conductor Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField, Min(1f)] private float bpm = 120f;

    [Tooltip("Ile sekund w pliku audio minie zanim zacznie się pierwszy beat (0 = utwór startuje idealnie na beacie).")]
    [SerializeField, Min(0f)] private float firstBeatOffset = 0f;

    [Tooltip("Kalibracja opóźnienia audio w sekundach. Dodatnia wartość = obraz opóźniony względem dźwięku.")]
    [SerializeField] private float audioLatencyOffset = 0f;

    [SerializeField] private bool playOnStart = true;

    [Header("Note timing")]
    [Tooltip("Ile kroków (beatów) nuta spada od spawnu do targetu.")]
    [SerializeField, Min(1)] private int stepsToTarget = 4;

    [Tooltip("Czas trwania jednego kroku nuty jako ułamek beatu (0.3 = 30% beatu).")]
    [SerializeField, Range(0.05f, 1f)] private float moveDurationInBeats = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool logBeats;

    /// <summary>Odpala się dokładnie na beacie. Parametr: indeks beatu (ujemny w pre-rollu).</summary>
    public event Action<int> OnBeat;

    /// <summary>
    /// Odpala się MoveDuration PRZED beatem. Tween rozpoczęty w tym evencie kończy się dokładnie na beacie.
    /// </summary>
    public event Action<int> OnBeatAhead;

    public double BeatDuration { get; private set; }
    public int StepsToTarget => stepsToTarget;
    public float MoveDuration => (float)(moveDurationInBeats * BeatDuration);
    public bool IsRunning { get; private set; }

    /// <summary>Pozycja w utworze w sekundach (0 = beat 0, ujemna w pre-rollu).</summary>
    public double SongPosition => AudioSettings.dspTime - _dspBeat0 - audioLatencyOffset;
    public double SongPositionInBeats => SongPosition / BeatDuration;

    private const double StartMargin = 0.2; // zapas na zaplanowanie PlayScheduled

    private double _dspBeat0;
    private int _nextBeat;
    private int _nextAheadBeat;

    private void Awake()
    {
        Instance = this;
        BeatDuration = 60.0 / bpm;

        if (musicSource != null)
            musicSource.playOnAwake = false;
    }

    private void Start()
    {
        if (playOnStart)
            StartConductor();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void StartConductor()
    {
        if (musicSource == null || musicSource.clip == null)
        {
            Debug.LogError("Conductor: brak AudioSource albo AudioClip.", this);
            return;
        }

        BeatDuration = 60.0 / bpm;

        // Czas, jaki musi minąć od teraz do beatu 0:
        // nuta spada stepsToTarget beatów + ostatni krok zaczyna się MoveDuration wcześniej + zapas.
        double preRoll = stepsToTarget * BeatDuration + MoveDuration + firstBeatOffset + StartMargin;

        _dspBeat0 = AudioSettings.dspTime + preRoll;
        musicSource.PlayScheduled(_dspBeat0 - firstBeatOffset);

        _nextBeat = -stepsToTarget;
        _nextAheadBeat = -stepsToTarget;
        IsRunning = true;
    }

    private void Update()
    {
        if (!IsRunning)
            return;

        double pos = SongPosition;
        double moveDur = MoveDuration;

        // Przetwarzamy eventy chronologicznie (także gdy klatka się przytnie i minęło kilka beatów).
        while (true)
        {
            double tBeat = _nextBeat * BeatDuration;
            double tAhead = _nextAheadBeat * BeatDuration - moveDur;

            if (tBeat <= tAhead)
            {
                if (pos < tBeat) break;
                FireBeat(_nextBeat);
                _nextBeat++;
            }
            else
            {
                if (pos < tAhead) break;
                OnBeatAhead?.Invoke(_nextAheadBeat);
                _nextAheadBeat++;
            }
        }
    }

    private void FireBeat(int beat)
    {
        if (logBeats)
            Debug.Log($"Beat {beat}");

        OnBeat?.Invoke(beat);
    }
}
