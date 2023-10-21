using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class to manage block ui
/// </summary>
public class BlockUI : MonoBehaviour, IPointerDownHandler
{
    // block type can be set in inspector to specify what UI is this block type associated to,
    // ideally to be in dedicated enum to separated from Logic
    [field : SerializeField] public BlockType BlockType { get; private set; }
    /// <summary>
    /// Block quantity
    /// </summary>
    [SerializeField] private TextMeshProUGUI quantityText;
    /// <summary>
    /// exposed event to notify when a mouse down happened
    /// </summary>
    public event Action<BlockType> OnBeginToDrag = _ => { };

    /// <summary>
    /// set quantity
    /// </summary>
    /// <param name="quantity"></param>
    public void SetQuantity(int quantity)
    {
        quantityText.text = quantity.ToString();
    }

    /// <summary>
    /// implement OnPointerDown function to handle the mouse down and pass that event
    /// </summary>
    /// <param name="eventData"></param>
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        OnBeginToDrag.Invoke(BlockType);
    }
}