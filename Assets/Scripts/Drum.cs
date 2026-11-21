using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Drum : MonoBehaviour
{
    [field: SerializeField]
    public DrumNumber drumNumber { get; private set; }

    public event Action<Drum, bool, FallingNote.NoteType, float> OnDrumHit;

    private InputSystem_Actions _actions;

    private void Awake()
    {
        _actions = new InputSystem_Actions();

        /*switch (drumNumber)
        {
            case DrumNumber.Drum1:
                _actions.Drums.Drum1.performed += 
        }*/
    }

    private void OnEnable()
    {
        _actions.Drums.Enable();
        GetDrumsActionByDrumNumber(drumNumber).performed += OnDrumClicked;
    }

    private void OnDrumClicked(InputAction.CallbackContext context)
    {
        Collider2D col = Physics2D.OverlapPoint(transform.position);
        if(col == null || col.tag != "Note")
        {
            OnDrumHit?.Invoke(this, false, FallingNote.NoteType.Good, 0f);
            return;
        }

        FallingNote note = col.GetComponent<FallingNote>();
        float distance = Mathf.Abs(transform.position.y - note.transform.position.y);

        OnDrumHit?.Invoke(this, true, note.noteType, distance);
        note.DestroyNote(true);
    }

    private void OnDisable()
    {
        _actions.Drums.Disable();
        GetDrumsActionByDrumNumber(drumNumber).performed -= OnDrumClicked;
    }

    public enum DrumNumber
    {
        Drum1,
        Drum2,
        Drum3,
        Drum4
    }

    private InputAction GetDrumsActionByDrumNumber(DrumNumber drumNumber)
    {
        switch (drumNumber)
        {
            case DrumNumber.Drum1:
                return _actions.Drums.Drum1;
            case DrumNumber.Drum2:
                return _actions.Drums.Drum2;
            case DrumNumber.Drum3:
                return _actions.Drums.Drum3;
            case DrumNumber.Drum4:
                return _actions.Drums.Drum4;
            default: return null;
        }
    }
}
