using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Pokazuje sprite popupu (Miss / Bad / Meh / Good / Perfect) nad bêbnem, którego dotyczy trafienie.
/// Nas³uchuje:
///  - Drum.OnDrumHit            -> trafienie (Perfect / Good / Meh), klikniêcie w z³¹ nutê (Bad) albo w pustkê (Miss)
///  - FallingNote.OnNoteMissed  -> nuta Good spad³a bez trafienia (Miss)
/// Na ka¿dy bêben przypada maks. jeden popup naraz - nowy zastêpuje poprzedni.
/// </summary>
public class HitPopupManager : MonoBehaviour
{
    private enum HitRating
    {
        Miss,
        Bad,
        Meh,
        Good,
        Perfect
    }

    [Header("Bêbny (te same co w PlayerStats)")]
    [SerializeField] private Drum[] drums;

    [Header("Sprity popupów (Sprites/Pop upy)")]
    [Tooltip("miss.ase")]
    [SerializeField] private Sprite missSprite;
    [Tooltip("Popup po klikniêciu w z³¹ nutê (Bad). Puste = u¿yty zostanie sprite Miss.")]
    [SerializeField] private Sprite badSprite;
    [Tooltip("m.ase")]
    [SerializeField] private Sprite mehSprite;
    [Tooltip("g.ase")]
    [SerializeField] private Sprite goodSprite;
    [Tooltip("p.ase")]
    [SerializeField] private Sprite perfectSprite;

    [Header("Progi dystansu (zgodne z PlayerStats)")]
    [SerializeField] private float perfectDistance = 0.08f;
    [SerializeField] private float goodDistance = 0.3f;

    [Header("Pozycja i wygl¹d")]
    [Tooltip("Przesuniêcie popupu wzglêdem bêbna.")]
    [SerializeField] private Vector2 offset = new Vector2(0f, 1.2f);
    [SerializeField] private float popupScale = 1f;
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 50;

    [Header("Animacja")]
    [SerializeField, Min(0.01f)] private float popInDuration = 0.15f;
    [SerializeField, Min(0f)] private float holdDuration = 0.35f;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.25f;
    [Tooltip("O ile jednostek popup unosi siê w górê przez ca³¹ animacjê.")]
    [SerializeField] private float riseDistance = 0.5f;

    private class ActivePopup
    {
        public GameObject go;
        public Sequence seq;
    }

    private readonly Dictionary<Drum, ActivePopup> _active = new Dictionary<Drum, ActivePopup>();

    private void OnEnable()
    {
        foreach (Drum drum in drums)
        {
            if (drum != null)
                drum.OnDrumHit += HandleDrumHit;
        }

        FallingNote.OnNoteMissed += HandleNoteMissed;
    }

    private void OnDisable()
    {
        foreach (Drum drum in drums)
        {
            if (drum != null)
                drum.OnDrumHit -= HandleDrumHit;
        }

        FallingNote.OnNoteMissed -= HandleNoteMissed;
    }

    private void OnDestroy()
    {
        foreach (ActivePopup popup in _active.Values)
            popup.seq?.Kill();

        _active.Clear();
    }

    // Parametry jak w PlayerStats.OnNoteHit.
    private void HandleDrumHit(Drum drum, bool hit, FallingNote.NoteType noteType, float distance)
    {
        if (!hit)
        {
            ShowPopup(drum, HitRating.Miss);
            return;
        }

        if (noteType == FallingNote.NoteType.Bad)
        {
            ShowPopup(drum, HitRating.Bad);
            return;
        }

        HitRating rating;
        if (distance < perfectDistance)
            rating = HitRating.Perfect;
        else if (distance < goodDistance)
            rating = HitRating.Good;
        else
            rating = HitRating.Meh;

        ShowPopup(drum, rating);
    }

    // Nuta Good przelecia³a poni¿ej missY - popup pokazujemy nad najbli¿szym bêbnem (po osi X).
    private void HandleNoteMissed(FallingNote note)
    {
        Drum drum = GetNearestDrum(note.transform.position.x);
        if (drum != null)
            ShowPopup(drum, HitRating.Miss);
    }

    private Drum GetNearestDrum(float x)
    {
        Drum nearest = null;
        float best = float.MaxValue;

        foreach (Drum drum in drums)
        {
            if (drum == null)
                continue;

            float d = Mathf.Abs(drum.transform.position.x - x);
            if (d < best)
            {
                best = d;
                nearest = drum;
            }
        }

        return nearest;
    }

    private Sprite GetSprite(HitRating rating)
    {
        switch (rating)
        {
            case HitRating.Perfect: return perfectSprite;
            case HitRating.Good: return goodSprite;
            case HitRating.Meh: return mehSprite;
            case HitRating.Bad: return badSprite != null ? badSprite : missSprite;
            default: return missSprite;
        }
    }

    private void ShowPopup(Drum drum, HitRating rating)
    {
        Sprite sprite = GetSprite(rating);
        if (sprite == null)
        {
            Debug.LogWarning($"HitPopupManager: brak sprite'a dla {rating}.", this);
            return;
        }

        // Zast¹p poprzedni popup na tym bêbnie.
        if (_active.TryGetValue(drum, out ActivePopup old))
        {
            old.seq?.Kill();
            if (old.go != null)
                Destroy(old.go);
            _active.Remove(drum);
        }

        Vector3 startPos = drum.transform.position + (Vector3)offset;

        GameObject go = new GameObject($"Popup_{rating}");
        go.transform.position = startPos;
        go.transform.localScale = Vector3.zero;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = sortingOrder;

        float total = popInDuration + holdDuration + fadeDuration;

        Sequence seq = DOTween.Sequence();
        seq.Append(go.transform.DOScale(popupScale, popInDuration).SetEase(Ease.OutBack));
        seq.Join(go.transform.DOMoveY(startPos.y + riseDistance, total).SetEase(Ease.OutQuad));
        seq.AppendInterval(holdDuration);
        seq.Append(sr.DOFade(0f, fadeDuration));
        seq.SetLink(go);
        seq.OnComplete(() =>
        {
            if (_active.TryGetValue(drum, out ActivePopup current) && current.go == go)
                _active.Remove(drum);

            Destroy(go);
        });

        _active[drum] = new ActivePopup { go = go, seq = seq };
    }
}