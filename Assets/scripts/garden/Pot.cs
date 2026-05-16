using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class Pot : MonoBehaviour, IDraggable
{
    // Helper to set allowed positions from external scripts (e.g., ZoneSelector).
    internal void SetAllowedPositions(Vector2[] positions) => _allowedPositions = positions;
    // Fixed positions where this pot may be placed. Assigned in the inspector.
    [SerializeField] private Vector2[] _allowedPositions;
    public Vector2[] AllowedPositions => _allowedPositions;

    // The flower currently planted in this pot (null if empty).
    [SerializeField] private Flower _currentFlower;
    public Flower CurrentFlower => _currentFlower;

    // Prefab reference for the flower that will be instantiated when a seed is bought.
    [SerializeField] private GameObject _flowerPrefab;
    public GameObject FlowerPrefab => _flowerPrefab;

    /// <summary>
    /// Plant a flower into this pot. Throws if the pot is already occupied.
    /// </summary>
    public void PlantFlower(Flower flower)
    {
        if (CurrentFlower != null)
            throw new InvalidOperationException("Pot already occupied.");
        _currentFlower = flower;
        flower.transform.SetParent(transform);
    }

    /// <summary>
    /// Remove the current flower from the pot and destroy its GameObject.
    /// </summary>
    public void RemoveFlower()
    {
        if (_currentFlower != null)
        {
            Destroy(_currentFlower.gameObject);
            _currentFlower = null;
        }
    }

    // IDraggable implementation – delegates visual feedback to FlowerCareSystem.
    public void OnDragStart() { /* No action needed on start */ }

    public void OnDrag()
    {
        var cam = Camera.main;
        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        FlowerCareSystem.Instance.ShowDropZoneFeedback(this, mouseWorld);
    }

    public void OnDragEnd()
    {
        var cam = Camera.main;
        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 closest = FindClosestAllowedPosition(mouseWorld);
        transform.position = closest;
        FlowerCareSystem.Instance.HideDropZoneFeedback();
    }

    private Vector2 FindClosestAllowedPosition(Vector2 target)
    {
        if (AllowedPositions == null || AllowedPositions.Length == 0) return transform.position;
        Vector2 best = AllowedPositions[0];
        float min = Vector2.Distance(target, best);
        foreach (var pos in AllowedPositions)
        {
            float d = Vector2.Distance(target, pos);
            if (d < min) { min = d; best = pos; }
        }
        return best;
    }
}