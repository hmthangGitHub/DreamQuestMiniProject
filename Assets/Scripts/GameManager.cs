using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Entry point of the game
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    ///  3 block ui handler
    /// </summary>
    [SerializeField] private BlockUI[] blockUIs;
    /// <summary>
    /// block place holder handler
    /// </summary>
    [SerializeField] private BlockPlaceHolderManager blockPlaceHolderManager;
    /// <summary>
    /// vfx handler
    /// </summary>
    [SerializeField] private VFXHandler vfxHandler;
    /// <summary>
    /// audio handler
    /// </summary>
    [SerializeField] private AudioPlayer audioPlayer;
    /// <summary>
    /// 3d block instance manager
    /// </summary>
    [SerializeField] private BlockInstanceManager blockInstanceManager;

    /// <summary>
    /// GameData
    /// </summary>
    private GameData currentGameData;

    /// <summary>
    /// Entry Point of the game:
    /// </summary>
    private void Start()
    {
        // Load saved GameData 
        currentGameData = GameDataLoader.Load();
        // Initialize saved game state
        // Initialize ui
        SetUpBlocksUI();
        // Initialize 3d scene
        SetUpBlockInstances();
        // set up in game change event
        SetUpBlockPlaceHolderCallBack();
    }

    private void SetUpBlockPlaceHolderCallBack()
    {
        // set up what to do when place a block
        blockPlaceHolderManager.OnPlaceBlock += (blockType, position) =>
        {
            var blockInSceneData = new BlockInSceneData()
            {
                position = position,
                blockType = blockType
            };
            
            // add block to the game data
            currentGameData.AddBlock(blockInSceneData);
            
            // change to block quantity by -1
            var currentQuantity = currentGameData.GetBlockQuantity(blockType) - 1;
            currentGameData.SetBlockQuantity(blockType, currentQuantity);

            // add block in 3d scene
            blockInstanceManager.AddBlockInScene(blockInSceneData, isPlayInAnimation: true);
            // change block ui
            blockUIs.First(x => x.BlockType == blockType)
                    .SetQuantity(currentQuantity);
        };

        // set up what to do when remove a block
        blockPlaceHolderManager.OnRemoveBlock += (blockType, position) =>
        {
            // remove block from game data
            currentGameData.RemoveBlock(position);
            
            // change to block quantity by -1
            var currentQuantity = currentGameData.GetBlockQuantity(blockType) + 1;
            currentGameData.SetBlockQuantity(blockType, currentQuantity);

            // remove block in 3d scene
            blockInstanceManager.RemoveBlockInScene(position);
            // change block ui
            blockUIs.First(x => x.BlockType == blockType).SetQuantity(currentQuantity);
            
            // spawn a vfx and play sfx
            vfxHandler.CreateDestroyVFX(position);
            audioPlayer.PlayClip(AudioPlayer.Remove);
        };

        // callback to check if current position is available to place a block
        blockPlaceHolderManager.IsAvailable += currentGameData.IsPositionAvailable;
    }

    /// <summary>
    /// place all block in the scene in saved game data
    /// </summary>
    private void SetUpBlockInstances()
    {
        foreach (var blockInSceneData in currentGameData.BlockInSceneDatas)
        {
            blockInstanceManager.AddBlockInScene(blockInSceneData, isPlayInAnimation: false);
        }
    }

    /// <summary>
    /// set block quantity to ui
    /// </summary>
    private void SetUpBlocksUI()
    {
        foreach (var blockUI in blockUIs)
        {
            // set quantity to ui
            blockUI.SetQuantity(currentGameData.GetBlockQuantity(blockUI.BlockType));
            // hook to the ui event
            blockUI.OnBeginToDrag += OnBeginDrag;
        }
    }

    /// <summary>
    /// when ever a mouse down click at UI
    /// </summary>
    /// <param name="blockType"></param>
    private void OnBeginDrag(BlockType blockType)
    {
        // check if the current block have anything left
        if (currentGameData.GetBlockQuantity(blockType) > 0)
        {
            // allow place holder to process
            blockPlaceHolderManager.OnBeginDrag(blockType);
        }
    }
    
    void Update()
    {
        // check if Escape key is pressed
        if (Input.GetKeyUp(KeyCode.Escape))
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }


    /// <summary>
    /// when ever application quit, save the current game data
    /// </summary>
    private void OnApplicationQuit()
    {
        GameDataLoader.Save(currentGameData);
    }
}
