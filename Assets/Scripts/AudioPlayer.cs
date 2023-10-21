using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// class to handle sfx,
/// every sfx will have a key attach to it
/// only need to call exposed function to play correspond sfx>
public class AudioPlayer : MonoBehaviour
{
    /// <summary>
    /// List of sfx keys
    /// </summary>
    public static readonly string Move = "Move";
    public static readonly string Place = "Place";
    public static readonly string Remove = "Remove";
    
    /// <summary>
    /// paring the sfx and its key
    /// </summary>
    [Serializable]
    public class AudioContainer
    {
        public string key;
        public AudioClip clip;
    }

    /// <summary>
    /// listing audio clips pair
    /// </summary>
    [SerializeField] private AudioContainer[] audioContainers;
    [SerializeField] private AudioSource audioSource;
    
    /// <summary>
    /// Make a cache by indexing audio clip's key faster look up
    /// </summary>
    private Dictionary<string, AudioClip> audioContainerDictionary;
    private Dictionary<string, AudioClip> AudioContainerDictionary => 
        audioContainerDictionary ??= audioContainers.ToDictionary(x => x.key, x=> x.clip);

    /// <summary>
    /// Play SFX by its key
    /// </summary>
    /// <param name="key"></param>
    public void PlayClip(string key)
    {
        audioSource.PlayOneShot(AudioContainerDictionary[key]);
    }
}
