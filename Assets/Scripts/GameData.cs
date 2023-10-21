using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// class to save game data
/// </summary>
[Serializable]
public class GameData
{
    /// <summary>
    /// All blocks in scene
    /// </summary>
    [field : SerializeField] public BlockInSceneData[] BlockInSceneDatas { get; private set; }
    /// <summary>
    /// // blocks left in inventory
    /// </summary>
    [SerializeField] private InventoryData[] blockInInventoryDatas; 

    /// <summary>
    /// make a dictionary cache of InventoryData by make BlockType as key to faster look up 
    /// </summary>
    private Dictionary<BlockType, InventoryData> blockInInventoryDictionary;
    private Dictionary<BlockType, InventoryData> BlockInInventoryDictionary
        => blockInInventoryDictionary ??= blockInInventoryDatas.ToDictionary(x => x.blockType);

    /// <summary>
    /// make a dictionary cache of BlockInSceneData by make its position as key to faster look up 
    /// </summary>
    private Dictionary<Vector3, BlockInSceneData> blockInSceneDictionary;
    private Dictionary<Vector3, BlockInSceneData> BlockInSceneDataDictionary
        => blockInSceneDictionary ??= BlockInSceneDatas.ToDictionary(x => x.position);

    /// <summary>
    /// add block to the game data
    /// </summary>
    /// <param name="blockInSceneData"></param>
    public void AddBlock(BlockInSceneData blockInSceneData)
    {
        BlockInSceneDataDictionary.Add(blockInSceneData.position, blockInSceneData);
    }
    
    /// <summary>
    /// remove block from game data
    /// </summary>
    /// <param name="position"></param>
    public void RemoveBlock(Vector3 position)
    {
        BlockInSceneDataDictionary.Remove(position);
    }
    
    /// <summary>
    /// check the position is available to place another block
    /// </summary>
    /// <param name="position">block position in world space</param>
    /// <returns></returns>
    public bool IsPositionAvailable(Vector3 position)
    {
        return !BlockInSceneDataDictionary.ContainsKey(position);
    }

    /// <summary>
    /// get block quantity
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public int GetBlockQuantity(BlockType type)
    {
        return BlockInInventoryDictionary[type].quantity;
    }
    
    /// <summary>
    /// set block quantity
    /// </summary>
    /// <param name="type">block type</param>
    /// <param name="quantity">quantity</param>
    public void SetBlockQuantity(BlockType type, int quantity)
    {
        BlockInInventoryDictionary[type].quantity = quantity;
    }

    /// <summary>
    /// return the serialized version of game data to save in disk
    /// </summary>
    /// <returns></returns>
    public string Serialize()
    {
        BlockInSceneDatas = BlockInSceneDataDictionary.Values.ToArray();
        return JsonUtility.ToJson(this);
    }

    /// <summary>
    /// deserialized json data into object
    /// </summary>
    /// <param name="json">can be null</param>
    /// <returns></returns>
    public static GameData Deserialize(string json)
    {
        return json != default
            ? JsonUtility.FromJson<GameData>(json)
            : new () // if json data is null, treat as new game
            {
                BlockInSceneDatas = Array.Empty<BlockInSceneData>(),
                blockInInventoryDatas = new[]
                {
                    new InventoryData()
                    {
                        blockType = BlockType.Red,
                        quantity = 10
                    },
                    new InventoryData()
                    {
                        blockType = BlockType.Green,
                        quantity = 10
                    },
                    new InventoryData()
                    {
                        blockType = BlockType.Blue,
                        quantity = 10
                    }
                },
            };
    }
}

/// <summary>
/// class to save block state, currently only have position
/// </summary>
[Serializable]
public class BlockInSceneData
{
    public BlockType blockType;
    public Vector3 position;
}

/// <summary>
/// class to save inventory state, currently only have quantity
/// </summary>
[Serializable]
public class InventoryData
{
    public BlockType blockType;
    public int quantity;
}

/// <summary>
/// Type of blocks
/// </summary>
public enum BlockType
{
    Red,
    Green,
    Blue,
}
