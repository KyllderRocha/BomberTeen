using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple serialization class used by AudioManager to pair 
/// a string (name) to an AudioClip directly in the Unity Inspector.
/// </summary>
[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}
