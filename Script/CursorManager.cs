using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class CursorManager : MonoBehaviour

{
    public int noOfUINEedingCursor = 0;
    public bool cursorNeeded = false;
    public static CursorManager Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;

        }
    }
    public void NeedCursor()
    {
        noOfUINEedingCursor++;
        SetCursorState();
    }
    public void ReleaseNeedCursor()
    {
        noOfUINEedingCursor--;
        if (noOfUINEedingCursor < 0)
            noOfUINEedingCursor = 0;
        SetCursorState();
    }
    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState();
    }

    private void SetCursorState()
    {
        cursorNeeded = true;
        if (noOfUINEedingCursor == 0)
        {
            cursorNeeded = false;
        }

        Cursor.lockState = cursorNeeded ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
