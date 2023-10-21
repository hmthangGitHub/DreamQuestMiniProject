using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// class to handle operation of place holder block
/// </summary>
public class BlockPlaceHolderManager : MonoBehaviour
{
    /// <summary>
    /// camera to take ray cast from
    /// </summary>
    [SerializeField] private Camera camera;
    /// <summary>
    /// place holders
    /// </summary>
    [SerializeField] private BlockInstance[] placeHolders;
    /// <summary>
    /// audio player to play vfx when move place holder
    /// </summary>
    [SerializeField] private AudioPlayer audioPlayer;
    
    /// <summary>
    /// make cache place holder to faster look up
    /// </summary>
    private Dictionary<BlockType, BlockInstance> placeHoldersDictionary;
    private Dictionary<BlockType, BlockInstance> PlaceHoldersDictionary 
        => placeHoldersDictionary ??= placeHolders.ToDictionary(x => x.BlockType);

    /// <summary>
    /// if in this frame we are placing a block or not
    /// </summary>
    private bool isPlacingBlock;
    /// <summary>
    /// current block type is placing
    /// </summary>
    private BlockType currentPlacingBlockType;
    /// <summary>
    /// current placeholder transform to less memory allocate
    /// </summary>
    private Transform currentPlaceHolderTransform;
    /// <summary>
    /// current placeholder Animator
    /// </summary>
    private Animator currentAnimator;
    /// <summary>
    /// a block size is (1, 1, 1), offset it by haft size to make a block appear in the middle of grid
    /// </summary>
    private readonly float blockOffSet = 0.5f;
    /// <summary>
    /// cache the animator state
    /// </summary>
    private readonly int blinkingHash = Animator.StringToHash("Blinking");
    /// <summary>
    /// cache the animator state
    /// </summary>
    private readonly int notAvailableHash = Animator.StringToHash("NotAvailable");
    /// <summary>
    /// saved the last place holder position to compare to current position
    /// </summary>
    private Vector3 lastPlaceHolderPosition = Vector3.positiveInfinity;
    
    /// <summary>
    /// exposed event to let game manager know just place a block
    /// </summary>
    public event Action<BlockType, Vector3> OnPlaceBlock = (_, _) => { };
    /// <summary>
    /// exposed event to let game manager know just remove a block
    /// </summary>
    public event Action<BlockType, Vector3> OnRemoveBlock = (_, _) => { };
    /// <summary>
    /// query whether the current position is available
    /// </summary>
    public Func<Vector3, bool> IsAvailable { get; set; }

    /// <summary>
    /// expose event to notify just begin drag a block from UI
    /// </summary>
    /// <param name="blockType"></param>
    public void OnBeginDrag(BlockType blockType)
    {
        currentPlacingBlockType = blockType;
        isPlacingBlock = true;
        currentPlaceHolderTransform = PlaceHoldersDictionary[blockType].gameObject.transform;
        currentAnimator = PlaceHoldersDictionary[blockType].GetComponent<Animator>();
    }

    private void Update()
    {
        HandleAddBlock();
        HandleRemoveBlock();
    }

    private void HandleRemoveBlock()
    {
        // only process remove block if we are not placing other block, click right mouse button, and mouse is not over any GUI
        if (isPlacingBlock) return;
        if (!Input.GetMouseButtonUp(1)) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        var (isHit, hit) = QueryMouseRayCastHit();
        if (isHit)
        {
            // if raycast from mouse to the world hit a block instance
            if (hit.transform.TryGetComponent<BlockInstance>(out var blockInstance))
            {
                // then remove it by its type and position
                OnRemoveBlock.Invoke(blockInstance.BlockType, hit.transform.position);
            }
        }
    }

    // return hit info from mouse to the world
    private (bool isHit, RaycastHit hit) QueryMouseRayCastHit()
    {
        var cameraRay = camera.ScreenPointToRay(Input.mousePosition);
        return ((Physics.Raycast(cameraRay, out var hit), hit));
    }

    private void HandleAddBlock()
    {
        // only process when we are placing a block
        if (!isPlacingBlock) return;
        HandleMovingPlaceHolder();
        HandleBlockPlacement();
    }

    private void HandleBlockPlacement()
    {
        // only process if just release left mouse
        if (!Input.GetMouseButtonUp(0)) return;
        
        // if current position is available and the mouse is not over any GUI
        if (IsAvailable(currentPlaceHolderTransform.position) && !EventSystem.current.IsPointerOverGameObject())
        {
            // then place a block of currentPlacingBlockType, at current position
            OnPlaceBlock.Invoke(currentPlacingBlockType, currentPlaceHolderTransform.position);
        }

        // set the isPlacingBlock back to false
        isPlacingBlock = false;
        // set the place holder invisible again, wait for user to begin drag
        currentPlaceHolderTransform.gameObject.SetActive(false);
    }

    private void HandleMovingPlaceHolder()
    {
        // only process when left mouse button is down
        if (!Input.GetMouseButton(0)) return;
        // if not over any GUI
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // set place holder visible when the mouse is not over any GUI
            currentPlaceHolderTransform.gameObject.SetActive(true);
            
            var (isHit, hit) = QueryMouseRayCastHit();
            if (isHit)
            {
                var position = GetBlockPositionBaseOnHitInfo(hit);
                currentPlaceHolderTransform.position = position;
                // is current position is available to place?
                SetPlaceHolderStatus(position);
                // play sfx when moving place holder
                PlayMoveSfx(position);
            }
        }
        else
        {
            // set place holder inVisible when the mouse over GUI
            currentPlaceHolderTransform.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// from hitInfo, get block position corresponds to it
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>
    private Vector3 GetBlockPositionBaseOnHitInfo(RaycastHit hit)
    {
        var position = hit.point;
        position.y = hit.collider.bounds.max.y;
        
        // offset the block by half of its size in xz plane, and place the block on top of collider by half size
        position = new Vector3(Mathf.Floor(position.x + blockOffSet), 
            Mathf.Floor(position.y) + blockOffSet,
            Mathf.Floor(position.z + blockOffSet)); 
        return position;
    }

    /// <summary>
    /// play move sfx if need
    /// </summary>
    /// <param name="position">current place holder position</param>
    private void PlayMoveSfx(Vector3 position)
    {
        // only play sfx if last frame position is different from current frame
        if (lastPlaceHolderPosition != position)
        {
            audioPlayer.PlayClip(AudioPlayer.Move);
            lastPlaceHolderPosition = position;
        }
    }

    /// <summary>
    /// if the current position of place holder is valid, play blinking animation
    /// if not play not available animation to let user know
    /// </summary>
    /// <param name="position"></param>
    private void SetPlaceHolderStatus(Vector3 position)
    {
        currentAnimator.Play(IsAvailable(position) ? blinkingHash : notAvailableHash);
    }
}
