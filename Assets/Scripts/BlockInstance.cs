using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// class to wrap a block instance
/// </summary>
public class BlockInstance : MonoBehaviour
{
    /// <summary>
    /// block type
    /// </summary>
    [field : SerializeField] public BlockType BlockType { get; private set; }
    /// <summary>
    /// Where to spawn vfx when create
    /// </summary>
    [field : SerializeField] public Transform SpawnVFXPosition { get; private set; }
    /// <summary>
    /// Where to spawn vfx when remove
    /// </summary>
    [field : SerializeField] public Transform DestroyVFXPosition { get; private set; }
    [SerializeField] private Animator animator;
    private int appearAnimationHash = Animator.StringToHash("BlockInstanceAppear");
    
    /// <summary>
    /// exposed event to handling spawn event
    /// </summary>
    public event Action OnSpawnEvent = () => { };

    /// <summary>
    /// call back from animator to make timing better,
    /// there is no animation attach to destroy event, hook to on mouse click event
    /// </summary>
    public void OnSpawn() => OnSpawnEvent.Invoke();

    public void PlaySpawnAnimation()
    {
        animator.Play(appearAnimationHash);
    }
}