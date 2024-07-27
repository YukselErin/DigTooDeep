using System.Collections;
using System.Collections.Generic;
using Adrenak.UniMic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DisableVoice : MonoBehaviour
{
    public bool micRecording = true;
    public bool micEnabled = true;
    public bool holdingKey = false;
    public TextMeshProUGUI textMesh;
    public string enabledString = "Mic enabled, hold F to mute.";
    public string disabledString = "Mic disabled, Hold F to unmute.";

    public void toggleState()
    {
        if (!Mic.Instance)
        {
            return;
        }
        micEnabled = !micEnabled;
        if (micEnabled)
        {
            ResumeRecord();

            textMesh.text = enabledString;
        }
        else
        {
            StopRecord();

            textMesh.text = disabledString;

        }
    }

    void Update()
    {
        if (!Mic.Instance)
        {
            return;
        }
        if (Input.GetKey(KeyCode.F))
        {
            holdingKey = true;
            if (micEnabled)
            {
                StopRecord();
            }
            else
            {
                ResumeRecord();
            }
        }
        else if (holdingKey)
        {
            holdingKey = false;

            if (micEnabled)
            {
                ResumeRecord();
            }
            else
            {
                StopRecord();
            }
        }
    }
    void StopRecord()
    {
        if (micRecording)
        {
            micRecording = false;
            Voice.Instance.agent.MuteSelf = true;
            Mic.Instance.StopRecording();
        }
    }
    void ResumeRecord()
    {
        if (!micRecording)
        {
            micRecording = true;
            Voice.Instance.agent.MuteSelf = false;
            Mic.Instance.ResumeRecording();
        }
    }
}
