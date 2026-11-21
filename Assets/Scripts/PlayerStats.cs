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
            AddPoints(-5);
            //Miss!
            return;
        }

        if (noteType == FallingNote.NoteType.Good) // Good notes below
        {
            Debug.Log(distance);

            if (distance < 0.08f) //Perfect!
            {
                AddPoints(100);
                Debug.Log("Perfect!");
            }
            else if (distance < 0.3f) //Good!
            {
                AddPoints(25);
                Debug.Log("Good!");
            }
            else //Meh
            {
                AddPoints(5);
                Debug.Log("Meh!");
            }
        }
        else if (noteType == FallingNote.NoteType.Bad) // Bad notes below
        {
            if (distance < 0.08f) //Bad Perfect!
            {
                AddPoints(-100);
                Debug.Log("Bad Perfect!");
            }
            else if (distance < 0.3f)
            {
                AddPoints(-25);
                Debug.Log("Bad Good!");
            }
            else //Meh
            {
                AddPoints(-5);
                Debug.Log("Bad Meh!");
            }

        }
    }
}