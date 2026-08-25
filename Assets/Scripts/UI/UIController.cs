using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script purely focused on catching Volume Slider events on the Options screen
/// and sending them to the AudioManager to process and save.
/// </summary>
public class UIController : MonoBehaviour
{
    [Tooltip("Slider responsible for dictating the music volume.")]
    public Slider musicSlider;
    
    [Tooltip("Slider responsible for dictating the effects (SFX) volume.")]
    public Slider sfxSlider;

    private void Start()
    {
        if (AudioManager.instance == null) return;

        // Synchronizes the visual position of the bars (Sliders) with the volume saved in the AudioManager
        if (musicSlider != null && AudioManager.instance.musicSource != null)
            musicSlider.value = AudioManager.instance.musicSource.volume;
            
        if (sfxSlider != null && AudioManager.instance.sfxSource != null)
            sfxSlider.value = AudioManager.instance.sfxSource.volume;
    }

    public void ToggleMusic()
    {
        Debug.Log("Changed");
        AudioManager.instance.ToggleMusic();
    }

    public void ToggleSFX() 
    { 
        AudioManager.instance.ToggleSFX();
    }

    public void MusicVolume()
    {
        AudioManager.instance.MusicVolume(musicSlider.value);
    }

    public void SFXVolume()
    {
        AudioManager.instance.SFXVolume(sfxSlider.value);
    }
}
