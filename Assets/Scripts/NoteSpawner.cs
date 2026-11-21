using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [SerializeField] private FallingNote[] notePrefabs;
    [SerializeField] private Drum[] drums;


    [Header("Positions")]
    [SerializeField] private float spawnY = 6f;
    [SerializeField] private float targetY = -3f;

    [Header("Chart")]
    [Tooltip("Zaznaczone = nuta na każdy beat (do testów). Odznaczone = tylko beaty z listy Hit Beats.")]
    [SerializeField] private bool spawnOnEveryBeat = true;
    [SerializeField] private int[] hitBeats;

    private HashSet<int> _chart;
    private float[] _xPositions;

    private void Start()
    {
        _xPositions = new float[drums.Length];

        for (int i = 0; i < drums.Length; i++)
        {
            _xPositions[i] = drums[i].transform.position.x;
        }

        _chart = new HashSet<int>(hitBeats ?? new int[0]);
        Conductor.Instance.OnBeat += HandleBeat;
    }

    private void OnDestroy()
    {
        if (Conductor.Instance != null)
            Conductor.Instance.OnBeat -= HandleBeat;
    }

    private void HandleBeat(int beat)
    {
        int steps = Conductor.Instance.StepsToTarget;
        int hitBeat = beat + steps; // beat, na którym nuta ma trafić w target

        if (hitBeat < 0)
            return;

        if (!spawnOnEveryBeat && !_chart.Contains(hitBeat))
            return;


        float xSpawnPosition = _xPositions[Random.Range(0, _xPositions.Length)];
        FallingNote note = Instantiate(notePrefabs[Random.Range(0, notePrefabs.Length)], new Vector3(xSpawnPosition, spawnY, 0f), Quaternion.identity);
        note.Init(hitBeat, steps, spawnY, targetY);
    }
}
