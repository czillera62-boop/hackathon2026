using System.Collections;
using UnityEngine;

public class HandsController : MonoBehaviour
{
    [Header("Bębny (te same co w PlayerStats)")]
    [SerializeField] private Drum[] drums;

    [Header("Animatory rąk (z plików .ase)")]
    [SerializeField] private Animator leftHandAnimator;
    [SerializeField] private Animator rightHandAnimator;

    [Header("Ustawienia animacji")]
    [Tooltip("Nazwa klipu animacji z pliku Aseprite (w obu plikach jest 'swing').")]
    [SerializeField] private string swingStateName = "swing";

    private Coroutine _leftRoutine;
    private Coroutine _rightRoutine;

    private void Start()
    {
        // Zatrzymujemy ręce na pierwszej klatce (stan spoczynku) przy starcie gry
        FreezeAtStart(leftHandAnimator);
        FreezeAtStart(rightHandAnimator);
    }

    private void OnEnable()
    {
        foreach (Drum drum in drums)
        {
            if (drum != null)
                drum.OnDrumHit += HandleDrumHit;
        }
    }

    private void OnDisable()
    {
        foreach (Drum drum in drums)
        {
            if (drum != null)
                drum.OnDrumHit -= HandleDrumHit;
        }
    }

    private void HandleDrumHit(Drum drum, bool hit, FallingNote.NoteType noteType, float distance)
    {
        // Drum1 i Drum2 to Lewa ręka
        if (drum.drumNumber == Drum.DrumNumber.Drum1 || drum.drumNumber == Drum.DrumNumber.Drum2)
        {
            PlayHandSwing(leftHandAnimator, ref _leftRoutine);
        }
        // Drum3 i Drum4 to Prawa ręka
        else if (drum.drumNumber == Drum.DrumNumber.Drum3 || drum.drumNumber == Drum.DrumNumber.Drum4)
        {
            PlayHandSwing(rightHandAnimator, ref _rightRoutine);
        }
    }

    private void PlayHandSwing(Animator anim, ref Coroutine routine)
    {
        if (anim == null)
            return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(PlayOnceAndReset(anim));
    }

    private IEnumerator PlayOnceAndReset(Animator anim)
    {
        anim.speed = 1f;
        // Odpala animację 'swing' od samego początku (klatka 0) nawet przy szybkim klikaniu
        anim.Play(swingStateName, 0, 0f);
        anim.Update(0f);

        yield return null;

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(info.length);

        FreezeAtStart(anim);
    }

    private void FreezeAtStart(Animator anim)
    {
        if (anim == null)
            return;

        anim.Play(swingStateName, 0, 0f);
        anim.Update(0f);
        anim.speed = 0f; // Zamraża animację na 1. klatce (ręka nad klawiaturą)
    }
}