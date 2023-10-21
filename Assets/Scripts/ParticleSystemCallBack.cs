using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// callback whenever particle system stop playing
/// </summary>
public class ParticleSystemCallBack : MonoBehaviour
{
    /// <summary>
    /// exposed event
    /// </summary>
    public event Action OnParticleSystemStoppedEvent = () => { };

    /// <summary>
    /// initialize the stop action callback
    /// </summary>
    private void Start()
    {
        var particleSystem = GetComponent<ParticleSystem>();
        var main = particleSystem.main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    /// <summary>
    /// this event function call by unity when particle system stop playing
    /// </summary>
    public void OnParticleSystemStopped() => OnParticleSystemStoppedEvent.Invoke();
}
