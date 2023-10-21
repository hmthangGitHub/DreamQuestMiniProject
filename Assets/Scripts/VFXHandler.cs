using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Handle creation and pooling of vfx
/// </summary>
public class VFXHandler : MonoBehaviour
{
    /// <summary>
    /// particle to play when spawn a block
    /// </summary>
    [SerializeField] private ParticleSystemCallBack spawnParticleSystemPrefab;
    /// <summary>
    /// particle to play when destroy a block
    /// </summary>
    [SerializeField] private ParticleSystemCallBack destroyParticleSystemPrefab;
    /// <summary>
    /// particle system pools
    /// </summary>
    private Dictionary<ParticleSystemCallBack, ObjectPool<ParticleSystemCallBack>> particleSystemPoolDictionary;

    private void Start()
    {
        particleSystemPoolDictionary = new()
        {
            {
                spawnParticleSystemPrefab, new ObjectPool<ParticleSystemCallBack>(
                    () => OnCreateBlock(spawnParticleSystemPrefab),
                    OnGetVFX,
                    OnRelease,
                    OnParticleDestroy)
            },
            {
                destroyParticleSystemPrefab, new ObjectPool<ParticleSystemCallBack>(
                    () => OnCreateBlock(destroyParticleSystemPrefab),
                    OnGetVFX,
                    OnRelease,
                    OnParticleDestroy)
            }
        };
    }
    
    private void OnParticleDestroy(ParticleSystemCallBack particleSystemCallBack)
    {
        Destroy(particleSystemCallBack.gameObject);
    }

    /// <summary>
    /// callback from unity object pool, call every time particle system sent back into the pool
    /// </summary>
    /// <param name="particleSystemCallBack"></param>
    private void OnRelease(ParticleSystemCallBack particleSystemCallBack)
    {
        // set it invisible when sent it back into the pool
        particleSystemCallBack.gameObject.SetActive(false);
    }

    /// <summary>
    /// callback from unity object pool, call once per particle system create
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    private ParticleSystemCallBack OnCreateBlock(ParticleSystemCallBack prefab)
    {
        // instantiate the right prefab
        var particleSystemCallBack = Instantiate(prefab);
        
        // when particle system stop playing, put it back into the pool
        particleSystemCallBack.OnParticleSystemStoppedEvent += () =>
        {
            particleSystemPoolDictionary[prefab].Release(particleSystemCallBack);
        };
        return particleSystemCallBack;
    }
    
    /// <summary>
    /// callback from unity object pool
    /// </summary>
    /// <param name="particleSystem"></param>
    private void OnGetVFX(ParticleSystemCallBack particleSystem)
    {
        // set it visible when first take it from the pool
        particleSystem.gameObject.SetActive(true);
    }

    /// <summary>
    /// Create spawn vfx at position
    /// </summary>
    /// <param name="position"></param>
    public void CreateSpawnVFX(Vector3 position)
    {
        // get from the pool
        var particleSystem = particleSystemPoolDictionary[spawnParticleSystemPrefab].Get();
        // then set its position
        particleSystem.transform.position = position;
    }
    
    public void CreateDestroyVFX(Vector3 position)
    {
        // get from the pool
        var particleSystem = particleSystemPoolDictionary[destroyParticleSystemPrefab].Get();
        // then set its position
        particleSystem.transform.position = position;
    }
}
