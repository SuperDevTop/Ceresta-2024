using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSound : MonoBehaviour
{
    private bool isSoundOn = true;
    private bool isMusicOn = true;

    [Header("Icons to toggle")]
    public GameObject soundOffIcon;
    public GameObject soundOnIcon;
    public GameObject musicOffIcon;
    public GameObject musicOnIcon;

    private void Start()
    {
        
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1; // Default is on
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1; // Default is on
        UpdateSoundState();
        UpdateMusicState();
    }

    public void ToggleSoundCall()
    {
        isSoundOn = !isSoundOn;
        UpdateSoundState();
        PlayerPrefs.SetInt("SoundOn", isSoundOn ? 1 : 0);
    }
    public void ToggleMusicCall()
    {
        isMusicOn = !isMusicOn;
        UpdateMusicState();
        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 1 : 0);
    }

    private void UpdateSoundState()
    {
        AudioListener.pause = !isSoundOn;
        if (isSoundOn)
        {
            soundOnIcon.SetActive(true);
            soundOffIcon.SetActive(false);
        }
        else
        {
            soundOnIcon.SetActive(false);
            soundOffIcon.SetActive(true);
        }
    }
    private void UpdateMusicState()
    {
        AudioListener.pause = !isSoundOn;
        if (isMusicOn)
        {
            musicOnIcon.SetActive(true);
            musicOffIcon.SetActive(false);
        }
        else
        {
            musicOnIcon.SetActive(false);
            musicOffIcon.SetActive(true);
        }
    }
}
