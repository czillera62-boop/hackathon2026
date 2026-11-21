using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int punkty = 0;

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

    private void OnNoteHit(Drum drum, bool hit, FallingNote.NoteType noteType, float distance)
    {
        if (!hit)
        {
            punkty--;
            //Miss!
            return;
        }

        if (noteType == FallingNote.NoteType.Good)
        {
            Debug.Log(distance);

            if (distance < 0.1f)
            {
                punkty += 100;
                Debug.Log("PERFECT!");
            }
            else if (distance < 0.2f)
            {
                punkty += 10;
                Debug.Log("GOOD!");
            }
            else
            {
                punkty += 1;
                Debug.Log("Znoœnie");
            }
        }
        else
        {
            punkty--;
            //Burger
        }
    }
}

