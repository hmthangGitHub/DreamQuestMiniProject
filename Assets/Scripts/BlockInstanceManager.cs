using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// class to handle creation and remove 3d block instance, store block object pools
/// </summary>
public class BlockInstanceManager : MonoBehaviour
{

    [SerializeField] private BlockInstance[] blockPrefabs;
    [SerializeField] private VFXHandler vfxHandler;
    [SerializeField] private AudioPlayer audioPlayer;

    /// <summary>
    /// different type of block have its object pool, distinguish by BlockType, store in Dictionary for fast look up
    /// </summary>
    private Dictionary<BlockType, ObjectPool<BlockInstance>> blockObjectPoolDictionary;
    /// <summary>
    /// every block in the world have different position, so take position as a key to in Dictionary for fast look up
    /// </summary>
    private readonly Dictionary<Vector3, BlockInstance> blockInstanceDictionary = new();

    /// <summary>
    /// put in a awake function to make sure its run before GameManager.Start, could be a exposed function to be called
    /// </summary>
    private void Awake()
    {
        // initialize object pool dictionary with 3 different type
        blockObjectPoolDictionary = new()
        {
            {
                BlockType.Red,
                new ObjectPool<BlockInstance>(
                    () => OnCreateBlock(BlockType.Red), 
                    OnGetBlock, 
                    OnBlockRelease,
                    OnBlockDestroy)
            },
            {
                BlockType.Green,
                new ObjectPool<BlockInstance>(
                    () => OnCreateBlock(BlockType.Green), 
                    OnGetBlock, 
                    OnBlockRelease,
                    OnBlockDestroy)
            },
            {
                BlockType.Blue,
                new ObjectPool<BlockInstance>(
                    () => OnCreateBlock(BlockType.Blue), 
                    OnGetBlock, 
                    OnBlockRelease,
                    OnBlockDestroy)
            },
        };
    }
    
    private void OnBlockDestroy(BlockInstance blockInstance)
    {
        Destroy(blockInstance.gameObject);
    }

    /// <summary>
    /// callback from object pool, this callback called whenever a block get sent back into the pool
    /// </summary>
    /// <param name="blockInstance"></param>
    private void OnBlockRelease(BlockInstance blockInstance)
    {
        // set inactive to make it inVisible in the scene
        blockInstance.gameObject.SetActive(false);
    }

    /// <summary>
    /// call back from object pool, this callback called once per instance created
    /// </summary>
    /// <param name="blockType">base on block type we can instantiate the right object</param>
    /// <returns></returns>
    private BlockInstance OnCreateBlock(BlockType blockType)
    {
        // find the block prefab base on type, could be inefficient by looping through the prefab, can make faster by converting blockPrefabs to Dictionary
        // but in this case only 3 element so it would be fine
        var block = Instantiate(blockPrefabs.First(x => x.BlockType == blockType));
        
        // whenever a block is spawn by placing in 3d scene, create a spawn vfx, and play sfx
        block.OnSpawnEvent += () =>
        {
            vfxHandler.CreateSpawnVFX(block.SpawnVFXPosition.position);
            audioPlayer.PlayClip(AudioPlayer.Place);
        };
        return block;
    }
    
    /// <summary>
    /// callback from object pool, this callback call multiple time whenever the block is get from pool
    /// </summary>
    /// <param name="blockInstance"></param>
    private void OnGetBlock(BlockInstance blockInstance)
    {
        // set active to visible in the scene
        blockInstance.gameObject.SetActive(true);
    }

    /// <summary>
    /// expose method to add a block in scene
    /// </summary>
    /// <param name="blockInSceneData">block data</param>
    /// <param name="isPlayInAnimation">whether we need to play animation or not, when first time loaded, we dont need to play animation</param>
    public void AddBlockInScene(BlockInSceneData blockInSceneData, bool isPlayInAnimation)
    {
        // get block from the the pool and set position
        var blockInstance = blockObjectPoolDictionary[blockInSceneData.blockType].Get();
        blockInstance.transform.position = blockInSceneData.position;
        if (isPlayInAnimation)
        {
            blockInstance.PlaySpawnAnimation();
        }
        // add block to a cache to fast look up
        blockInstanceDictionary.Add(blockInstance.transform.position, blockInstance);
    }

    /// <summary>
    /// exposed method to remove a block in scene
    /// </summary>
    /// <param name="position">position of block</param>
    public void RemoveBlockInScene(Vector3 position)
    {
        // fast look up the block at the position
        var blockInstance = blockInstanceDictionary[position];
        // send to block into the pool
        blockObjectPoolDictionary[blockInstance.BlockType].Release(blockInstance);
        // remove from the cache
        blockInstanceDictionary.Remove(position);
    }
}