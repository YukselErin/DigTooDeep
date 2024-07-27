using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UINeedsCursor : MonoBehaviour
{
    bool registeredNeedCursor = false;
    void OnEnable()
    {
        registeredNeedCursor = true;
        CursorManager.Instance.NeedCursor();
    }
    void OnDisable()
    {
        if (registeredNeedCursor)
            CursorManager.Instance.ReleaseNeedCursor();
        registeredNeedCursor = false;
    }


}
