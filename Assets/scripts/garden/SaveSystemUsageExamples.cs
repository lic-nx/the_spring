// // =========================================================================
// // SaveSystemUsageExamples.cs
// // 
// // This file contains examples of how to use the new GameSaveManager
// // for saving and loading game state in the garden system.
// //
// // IMPORTANT: This file is for documentation purposes only.
// // Copy the relevant code snippets to your actual game scripts.
// // =========================================================================

// using UnityEngine;
// using System.Collections.Generic;

// /// <summary>
// /// Example class showing how to use the GameSaveManager
// /// </summary>
// public class SaveSystemUsageExamples : MonoBehaviour
// {
//     // =========================================================================
//     // BASIC USAGE EXAMPLES
//     // =========================================================================

//     // void Example_SaveZoneState()
//     // {
//     //     // Get the zone component (PotZoneArea implements iPotDropArea)
//     //     PotZoneArea zone = GetComponent<PotZoneArea>();
        
//     //     // Save the zone state when a pot is dropped
//     //     // This is automatically handled in PotZoneArea.OnPotDrop()
//     //     if (zone != null)
//     //     {
//     //         GameSaveManager.Instance.SaveZoneState(zone);
//     //     }
//     // }

//     // void Example_LoadZoneState()
//     // {
//     //     // Load the zone state when the game starts
//     //     // This is automatically handled in PotZoneArea.Start()
//     //     PotZoneArea zone = GetComponent<PotZoneArea>();
//     //     if (zone != null)
//     //     {
//     //         GameSaveManager.Instance.LoadZoneState(zone);
//     //     }
//     // }

//     void Example_SavePotState()
//     {
//         // Get the pot component
//         Pot pot = GetComponent<Pot>();
        
//         // Save the pot state (including its flower if any)
//         if (pot != null)
//         {
//             pot.SaveState(); // This calls GameSaveManager.Instance.SavePotState(this)
//         }
//     }

//     void Example_SaveFlowerState()
//     {
//         // Get the flower component
//         Flower flower = GetComponent<Flower>();
        
//         // Save the flower state
//         if (flower != null)
//         {
//             flower.SaveState(); // This calls GameSaveManager.Instance.SaveFlowerState(this)
//         }
//     }

//     // =========================================================================
//     // ADVANCED USAGE EXAMPLES
//     // =========================================================================

//     void Example_SaveCompleteGameState()
//     {
//         // Save everything at once (zones, pots, flowers, currency)
//         GameSaveManager.Instance.SaveGameState();
//     }

//     void Example_LoadCompleteGameState()
//     {
//         // Load everything at once
//         GameSaveManager.Instance.LoadGameState();
//     }

//     void Example_AutoSaveSetup()
//     {
//         // Configure auto-save in the GameSaveManager inspector:
//         // - Set autoSaveEnabled to true
//         // - Set autoSaveDelay to desired seconds between saves
        
//         // Or configure programmatically:
//         if (GameSaveManager.Instance != null)
//         {
//             GameSaveManager.Instance.autoSaveEnabled = true;
//             GameSaveManager.Instance.autoSaveDelay = 5f; // Save every 5 seconds
//         }
//     }

//     void Example_ClearSaveData()
//     {
//         // Clear save data for a specific zone
//         GameSaveManager.Instance.ClearZoneSaveData("zone_1");
        
//         // Clear all save data
//         GameSaveManager.Instance.ClearAllSaveData();
//     }

//     void Example_CheckSaveData()
//     {
//         // Check if a zone has save data
//         bool hasData = GameSaveManager.Instance.HasZoneSaveData("zone_1");
        
//         if (hasData)
//         {
//             Debug.Log("Zone has saved data");
//         }
//         else
//         {
//             Debug.Log("Zone has no saved data");
//         }
//     }

//     // =========================================================================
//     // MANUAL SAVE/LOAD FOR SPECIAL CASES
//     // =========================================================================

//     void Example_ManualPotSave()
//     {
//         Pot pot = GetComponent<Pot>();
//         if (pot != null)
//         {
//             // Create pot data manually
//             var potData = new GameSaveManager.PotData
//             {
//                 potId = pot.gameObject.name,
//                 spriteName = pot.GetComponent<SpriteRenderer>()?.sprite?.name ?? "Unknown",
//                 zoneId = pot.CurrentZone != null ? (pot.CurrentZone as PotZoneArea)?.zoneId : "",
//                 position = pot.transform.position,
//                 isOccupied = pot.CurrentFlower != null,
//                 flowerId = pot.CurrentFlower != null ? pot.CurrentFlower.gameObject.name : ""
//             };
            
//             // Save using the manager
//             GameSaveManager.Instance.SavePotData(potData);
//         }
//     }

//     void Example_ManualFlowerSave()
//     {
//         Flower flower = GetComponent<Flower>();
//         if (flower != null)
//         {
//             // Create flower data manually
//             var flowerData = new GameSaveManager.FlowerData
//             {
//                 flowerId = flower.gameObject.name,
//                 prefabName = flower.Conditions?.name ?? "Unknown",
//                 spriteName = flower.GetComponent<SpriteRenderer>()?.sprite?.name ?? "Unknown",
//                 position = flower.transform.position,
//                 currentStageIndex = GameSaveManager.Instance.GetPrivateField<int>(flower, "_currentStageIndex"),
//                 timeSinceLastWatering = GameSaveManager.Instance.GetPrivateField<float>(flower, "_timeSinceLastWatering"),
//                 timeSinceLastSunGeneration = GameSaveManager.Instance.GetPrivateField<float>(flower, "_timeSinceLastSunGeneration"),
//                 needWater = GameSaveManager.Instance.GetPrivateField<bool>(flower, "_needWater"),
//                 needFertilize = GameSaveManager.Instance.GetPrivateField<bool>(flower, "_needFertilize"),
//                 isFullyGrown = GameSaveManager.Instance.GetPrivateField<bool>(flower, "_isFullyGrown"),
//                 careEventCount = GameSaveManager.Instance.GetPrivateField<int>(flower, "_careEventCount"),
//                 growthConditionsName = flower.Conditions?.name ?? "Default",
//                 hasGivenSun = false
//             };
            
//             // Save using the manager
//             GameSaveManager.Instance.SaveFlowerData(flowerData);
//         }
//     }

//     // =========================================================================
//     // SETUP INSTRUCTIONS
//     // =========================================================================
    
//     /*
//      * SETUP INSTRUCTIONS:
//      * 
//      * 1. Add GameSaveManager to your scene:
//      *    - Create an empty GameObject in your scene
//      *    - Add the GameSaveManager component to it
//      *    - Optionally add GameSaveManagerPrefab component for easier setup
//      * 
//      * 2. Configure auto-save (optional):
//      *    - In the GameSaveManager inspector:
//      *      - Set autoSaveEnabled to true if you want automatic saving
//      *      - Set autoSaveDelay to the number of seconds between auto-saves
//      * 
//      * 3. The system will automatically:
//      *    - Save zone state when pots are dropped or removed
//      *    - Load zone state when zones are initialized
//      *    - Handle all the Yandex Games save/load functionality
//      * 
//      * 4. For manual control:
//      *    - Call GameSaveManager.Instance.SaveGameState() to save everything
//      *    - Call GameSaveManager.Instance.LoadGameState() to load everything
//      *    - Call GameSaveManager.Instance.SaveZoneState(zone) to save a specific zone
//      *    - Call GameSaveManager.Instance.LoadZoneState(zone) to load a specific zone
//      * 
//      * 5. The existing code in PotZoneArea (zone_for_pot.cs) will continue to work
//      *    and will automatically use the new GameSaveManager when available.
//      * 
//      * 6. For pots and flowers:
//      *    - Call pot.SaveState() to save a pot and its flower
//      *    - Call flower.SaveState() to save a flower
//      *    - The LoadState methods are called automatically during game load
//      */
// }