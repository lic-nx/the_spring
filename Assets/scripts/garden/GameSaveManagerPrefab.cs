// // This script is used to create a prefab for GameSaveManager in the Unity editor
// // Attach this to an empty GameObject in your scene and it will automatically
// // add the GameSaveManager component

// using UnityEngine;

// [RequireComponent(typeof(GameSaveManager))]
// public class GameSaveManagerPrefab : MonoBehaviour
// {
//     private void Awake()
//     {
//         // This ensures the GameSaveManager is properly initialized
//         var saveManager = GetComponent<GameSaveManager>();
//         if (saveManager != null)
//         {
//             // The GameSaveManager will handle its own singleton pattern
//             Debug.Log("[GameSaveManagerPrefab] GameSaveManager initialized.");
//         }
//     }
// }