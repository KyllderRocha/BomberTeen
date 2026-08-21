using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// Global audio manager (Singleton). Persists across all scenes.
    /// Controls the volume and playback of music and sound effects (SFX).
    /// </summary>
    public static AudioManager instance;

    [Header("Audio Libraries")]
    [Tooltip("List of all background music in the game.")]
    public Sound[] musicSounds;
    [Tooltip("List of all quick sound effects (e.g. explosion, clicks).")]
    public Sound[] sfxSounds;
    
    [Header("Audio Emitters (Sources)")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private void Awake()
    {
        // Ensures there is only one active AudioManager in the entire game (Singleton Pattern)
        if (instance == null)
        {
            instance = this; 
            DontDestroyOnLoad(gameObject); // Audio will not be cut when changing screens (e.g. returning to Menu)

            // Attempts to retrieve the volume settings previously saved by the player on the HD (PlayerPrefs)
            if (PlayerPrefs.HasKey("MusicVolume"))
            {
                var music = PlayerPrefs.GetFloat("MusicVolume");
                musicSource.volume = music;
            }
            else
            {
                musicSource.volume = 0.5f; // Default volume if it's the first access
            }

            if (PlayerPrefs.HasKey("SFXVolume"))
            {
                var sfx = PlayerPrefs.GetFloat("SFXVolume");
                sfxSource.volume = sfx;
            }
            else
            {
                sfxSource.volume = 0.5f;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayMusic(Constants.Audio.Theme);
    }

    /// <summary> Starts playing a background music continuously (looping). </summary>
    public void PlayMusic( string name)
    {
        // Searches for the sound by the exact name typed inside the array configured in the Inspector
        Sound s = Array.Find(musicSounds, x => x.name == name);
        if (s == null) 
        {
            Debug.Log("Sound Not Found: " + name);
        }
        else
        {
            musicSource.clip = s.clip;
            musicSource.Play();
        }
    }

    /// <summary> Plays a sound effect quickly without interrupting the previous sound of the same category (PlayOneShot). </summary>
    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);
        if (s == null)
        {
            Debug.Log("Sound Not Found: " + name);
        }
        else
        {
            sfxSource.PlayOneShot(s.clip);
        }
    }

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
    }

    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
    }

    /// <summary> Records and changes the music volume in real time (Triggered by the Options Sliders). </summary>
    public void MusicVolume(float volume)
    {
        musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume); // Saves the float number to the user's HD

    }

    /// <summary> Records and changes the sound effects volume in real time. </summary>
    public void SFXVolume(float volume)
    {
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}
