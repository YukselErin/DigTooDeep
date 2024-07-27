using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public AudioClip highlightAudio;
    public float playMaxCD;
    float lastTimeplayMaxCD;
    public int playMax;
    public float playCD = 0.1f;
    float lastTimeplayCD;
    void PlaySound(AudioClip audioClip)
    {
        if (playCD + lastTimeplayMaxCD < Time.time)
        {
            lastTimeplayMaxCD = Time.time;
            GetComponent<AudioSource>().PlayOneShot(audioClip);
        }

    }
    public void HighlightSFX() { PlaySound(highlightAudio); }
    public AudioClip ClickAudio;
    public void ClickSFX() { GetComponent<AudioSource>().PlayOneShot(ClickAudio); }

    public void quitGame(){
        Application.Quit();
    }
}
