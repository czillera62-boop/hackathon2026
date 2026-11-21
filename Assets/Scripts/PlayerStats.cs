using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int punkty = 0;

    /// <summary>Odpala siê po ka¿dej zmianie punktów. Parametr: aktualna liczba punktów.</summary>
    public event Action<int> OnPointsChanged;

    [SerializeField]
    private Drum[] drums;

    private void OnEnable()
    {
        foreach (Drum drum in drums)
        {
            drum.OnDrumHit += OnNoteHit;
        }
    }

    private void OnDisable()
    {
        foreach (Drum drum in drums)
        {
            drum.OnDrumHit -= OnNoteHit;
        }
    }

    private void AddPoints(int amount)
    {
        punkty += amount;
        OnPointsChanged?.Invoke(punkty);
    }

    private void OnNoteHit(Drum drum, bool hit, FallingNote.NoteType noteType, float distance)
    {
        if (!hit)
        {
            AddPoints(-1);
            //Miss!
            return;
        }

        if (noteType == FallingNote.NoteType.Good)
        {
            Debug.Log(distance);

            if (distance < 0.1f)
            {
                AddPoints(100);
                Debug.Log("PERFECT!");
            }
            else if (distance < 0.2f)
            {
                AddPoints(10);
                Debug.Log("GOOD!");
            }
            else
            {
                AddPoints(1);
                Debug.Log("Znoœnie");
            }
        }
        else
        {
            AddPoints(-1);
            //Burger
        }
    }
}