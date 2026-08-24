// using UnityEngine;
// using System.Collections.Generic;
// using System.Linq;
// using YG;

// /// <summary>
// /// Universal game save/load manager for garden system.
// /// Handles saving and loading of zones, pots, and flowers.
// /// </summary>
// public class GameSaveManager : MonoBehaviour
// {
//     public static GameSaveManager Instance { get; private set; }

//     [Header("Save Settings")]
//     [Tooltip("Whether to auto-save when changes occur")]
//     public bool autoSaveEnabled = true;

//     [Tooltip("Delay between auto-saves in seconds")]
//     public float autoSaveDelay = 2f;

//     private float _lastSaveTime;

//     private void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     private void Update()
//     {
//         if (autoSaveEnabled && Time.time - _lastSaveTime >= autoSaveDelay)
//         {
//             SaveGameState();
//             _lastSaveTime = Time.time;
//         }
//     }

//     #region Save Data Structures

//     [System.Serializable]
//     public class ZoneData
//     {
//         public string zoneId;
//         public bool isEmpty;
//         public string potId;
//         public Vector3 position;
//     }

//     [System.Serializable]
//     public class PotData
//     {
//         public string potId;
//         public string spriteName;
//         public string zoneId;
//         public Vector3 position;
//         public bool isOccupied;
//         public string flowerId;
//     }

//     [System.Serializable]
//     public class FlowerData
//     {
//         public string flowerId;
//         public string prefabName;
//         public string spriteName;
//         public Vector3 position;
//         public int currentStageIndex;
//         public float timeSinceLastWatering;
//         public float timeSinceLastSunGeneration;
//         public bool needWater;
//         public bool needFertilize;
//         public bool isFullyGrown;
//         public int careEventCount;
//         public string growthConditionsName;
//         public bool hasGivenSun;
//     }

//     [System.Serializable]
//     public class GardenSaveData
//     {
//         public List<ZoneData> zones = new List<ZoneData>();
//         public List<PotData> pots = new List<PotData>();
//         public List<FlowerData> flowers = new List<FlowerData>();
//         public int currency;
//         public System.DateTime lastSaveTime;
//     }

//     #endregion

//     #region Save Methods

//     /// <summary>
//     /// Save the current state of a zone
//     /// </summary>
//     public void SaveZoneState(iPotDropArea zone, Pot potComponent = null, string spriteName = null)
//     {
//         if (YG2.saves == null)
//         {
//             Debug.LogError("[GameSaveManager] YG2.saves is null!");
//             return;
//         }

//         var zoneData = CreateZoneData(zone, potComponent, spriteName);
//         SaveZoneData(zoneData);
//     }

//     /// <summary>
//     /// Save a pot's state
//     /// </summary>
//     public void SavePotState(Pot pot)
//     {
//         if (YG2.saves == null)
//         {
//             Debug.LogError("[GameSaveManager] YG2.saves is null!");
//             return;
//         }

//         var potData = CreatePotData(pot);
//         SavePotData(potData);

//         if (pot.CurrentFlower != null)
//         {
//             SaveFlowerState(pot.CurrentFlower);
//         }
//     }

//     /// <summary>
//     /// Save a flower's state
//     /// </summary>
//     public void SaveFlowerState(Flower flower)
//     {
//         if (YG2.saves == null)
//         {
//             Debug.LogError("[GameSaveManager] YG2.saves is null!");
//             return;
//         }

//         var flowerData = CreateFlowerData(flower);
//         SaveFlowerData(flowerData);
//     }

//     /// <summary>
//     /// Save complete game state
//     /// </summary>
//     public void SaveGameState()
//     {
//         if (YG2.saves == null)
//         {
//             Debug.LogError("[GameSaveManager] YG2.saves is null!");
//             return;
//         }

//         Debug.Log("[GameSaveManager] Starting full game state save...");

//         var gardenData = new GardenSaveData
//         {
//             lastSaveTime = System.DateTime.Now,
//             currency = CurrencyManager.Instance != null ? CurrencyManager.Instance.CurrentCurrency : 0
//         };

//         SaveAllZones(gardenData);
//         SaveAllPots(gardenData);
//         SaveAllFlowers(gardenData);

//         SaveToYandex(gardenData);
//         Debug.Log("[GameSaveManager] Full game state saved successfully.");
//     }

//     private void SaveAllZones(GardenSaveData gardenData)
//     {
//         if (DropZoneManager.Instance == null) return;

//         foreach (var zoneObj in DropZoneManager.Instance.GetAllZones())
//         {
//             var zone = zoneObj.GetComponent<iPotDropArea>();
//             if (zone != null)
//             {
//                 var zoneData = CreateZoneData(zone);
//                 gardenData.zones.Add(zoneData);
//             }
//         }
//     }

//     private void SaveAllPots(GardenSaveData gardenData)
//     {
//         var allPots = FindObjectsOfType<Pot>();
//         foreach (var pot in allPots)
//         {
//             var potData = CreatePotData(pot);
//             gardenData.pots.Add(potData);
//         }
//     }

//     private void SaveAllFlowers(GardenSaveData gardenData)
//     {
//         var allFlowers = FindObjectsOfType<Flower>();
//         foreach (var flower in allFlowers)
//         {
//             var flowerData = CreateFlowerData(flower);
//             gardenData.flowers.Add(flowerData);
//         }
//     }

//     private ZoneData CreateZoneData(iPotDropArea zone, Pot potComponent = null, string spriteName = null)
//     {
//         var leftDropArea = zone as PotZoneArea;
//         if (leftDropArea != null)
//         {
//             return new ZoneData
//             {
//                 zoneId = leftDropArea.zoneId,
//                 isEmpty = leftDropArea.isEmpty,
//                 potId = potComponent != null ? potComponent.gameObject.name : string.Empty,
//                 position = leftDropArea.transform.position
//             };
//         }

//         return new ZoneData
//         {
//             zoneId = zone.GetType().Name + "_" + zone.GetHashCode(),
//             isEmpty = true,
//             position = zone.GetComponent<Transform>().position
//         };
//     }

//     private PotData CreatePotData(Pot pot)
//     {
//         var spriteRenderer = pot.GetComponent<SpriteRenderer>();
//         string spriteName = spriteRenderer != null && spriteRenderer.sprite != null 
//             ? spriteRenderer.sprite.name 
//             : "Unknown";

//         return new PotData
//         {
//             potId = pot.gameObject.name,
//             spriteName = spriteName,
//             zoneId = pot.CurrentZone != null ? GetZoneId(pot.CurrentZone) : string.Empty,
//             position = pot.transform.position,
//             isOccupied = pot.CurrentFlower != null,
//             flowerId = pot.CurrentFlower != null ? pot.CurrentFlower.gameObject.name : string.Empty
//         };
//     }

//     private FlowerData CreateFlowerData(Flower flower)
//     {
//         var spriteRenderer = flower.GetComponent<SpriteRenderer>();
//         string spriteName = spriteRenderer != null && spriteRenderer.sprite != null 
//             ? spriteRenderer.sprite.name 
//             : "Unknown";

//         string prefabName = "Unknown";
//         if (flower.Conditions != null)
//         {
//             prefabName = flower.Conditions.name;
//         }

//         return new FlowerData
//         {
//             flowerId = flower.gameObject.name,
//             prefabName = prefabName,
//             spriteName = spriteName,
//             position = flower.transform.position,
//             currentStageIndex = GetPrivateField<int>(flower, "_currentStageIndex"),
//             timeSinceLastWatering = GetPrivateField<float>(flower, "_timeSinceLastWatering"),
//             timeSinceLastSunGeneration = GetPrivateField<float>(flower, "_timeSinceLastSunGeneration"),
//             needWater = GetPrivateField<bool>(flower, "_needWater"),
//             needFertilize = GetPrivateField<bool>(flower, "_needFertilize"),
//             isFullyGrown = GetPrivateField<bool>(flower, "_isFullyGrown"),
//             careEventCount = GetPrivateField<int>(flower, "_careEventCount"),
//             growthConditionsName = flower.Conditions != null ? flower.Conditions.name : "Default",
//             hasGivenSun = false
//         };
//     }

//     private string GetZoneId(iPotDropArea zone)
//     {
//         var leftDropArea = zone as PotZoneArea;
//         if (leftDropArea != null)
//         {
//             return leftDropArea.zoneId;
//         }
//         return zone.GetType().Name + "_" + zone.GetHashCode();
//     }

//     private T GetPrivateField<T>(object obj, string fieldName)
//     {
//         var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//         if (field != null)
//         {
//             return (T)field.GetValue(obj);
//         }
//         return default(T);
//     }

//     private void SaveZoneData(ZoneData zoneData)
//     {
//         if (YG2.saves.occupiedZones == null)
//         {
//             YG2.saves.occupiedZones = new List<ZoneSaveData>();
//         }

//         YG2.saves.occupiedZones.RemoveAll(z => z.zoneId == zoneData.zoneId);
        
//         YG2.saves.occupiedZones.Add(new ZoneSaveData
//         {
//             zoneId = zoneData.zoneId,
//             potSpriteName = zoneData.isEmpty ? string.Empty : zoneData.potId
//         });

//         YG2.SaveProgress();
//     }

//     private void SavePotData(PotData potData)
//     {
//         SaveToYandex();
//     }

//     private void SaveFlowerData(FlowerData flowerData)
//     {
//         SaveToYandex();
//     }

//     private void SaveToYandex(GardenSaveData gardenData = null)
//     {
//         if (gardenData != null)
//         {
//             YG2.saves.gardenData = gardenData;
//         }
//         YG2.SaveProgress();
//     }

//     #endregion

//     #region Load Methods

//     /// <summary>
//     /// Load zone state from save
//     /// </summary>
//     public void LoadZoneState(iPotDropArea zone)
//     {
//         if (YG2.saves == null || YG2.saves.occupiedZones == null)
//         {
//             Debug.LogWarning("[GameSaveManager] No save data available.");
//             return;
//         }

//         var leftDropArea = zone as PotZoneArea;
//         if (leftDropArea == null) return;

//         var zoneData = YG2.saves.occupiedZones.FirstOrDefault(z => z.zoneId == leftDropArea.zoneId);
//         if (zoneData != null && !string.IsNullOrEmpty(zoneData.potSpriteName))
//         {
//             leftDropArea.RestorePot(zoneData.potSpriteName);
//         }
//     }

//     /// <summary>
//     /// Load complete game state
//     /// </summary>
//     public void LoadGameState()
//     {
//         Debug.Log("[GameSaveManager] Loading game state...");

//         if (YG2.saves == null)
//         {
//             Debug.LogWarning("[GameSaveManager] No save data available.");
//             return;
//         }

//         LoadAllZones();
//         LoadAllPots();
//         LoadAllFlowers();

//         Debug.Log("[GameSaveManager] Game state loaded successfully.");
//     }

//     private void LoadAllZones()
//     {
//         if (DropZoneManager.Instance == null || YG2.saves.occupiedZones == null) return;

//         foreach (var zoneObj in DropZoneManager.Instance.GetAllZones())
//         {
//             var zone = zoneObj.GetComponent<iPotDropArea>();
//             if (zone != null)
//             {
//                 LoadZoneState(zone);
//             }
//         }
//     }

//     private void LoadAllPots()
//     {
//         if (YG2.saves.gardenData == null || YG2.saves.gardenData.pots == null) return;

//         foreach (var potData in YG2.saves.gardenData.pots)
//         {
//             var potObj = GameObject.Find(potData.potId);
//             if (potObj != null)
//             {
//                 var pot = potObj.GetComponent<Pot>();
//                 if (pot != null)
//                 {
//                     LoadPotState(pot, potData);
//                 }
//             }
//         }
//     }

//     private void LoadAllFlowers()
//     {
//         if (YG2.saves.gardenData == null || YG2.saves.gardenData.flowers == null) return;

//         foreach (var flowerData in YG2.saves.gardenData.flowers)
//         {
//             var flowerObj = GameObject.Find(flowerData.flowerId);
//             if (flowerObj != null)
//             {
//                 var flower = flowerObj.GetComponent<Flower>();
//                 if (flower != null)
//                 {
//                     LoadFlowerState(flower, flowerData);
//                 }
//             }
//             else
//             {
//                 RestoreFlowerFromData(flowerData);
//             }
//         }
//     }

//     private void LoadPotState(Pot pot, PotData potData)
//     {
//         if (!string.IsNullOrEmpty(potData.spriteName))
//         {
//             var sprite = Shop.Instance?.GetPotSpriteByName(potData.spriteName);
//             if (sprite != null)
//             {
//                 var spriteRenderer = pot.GetComponent<SpriteRenderer>();
//                 if (spriteRenderer != null)
//                 {
//                     spriteRenderer.sprite = sprite;
//                 }
//             }
//         }

//         pot.transform.position = potData.position;
//     }

//     private void LoadFlowerState(Flower flower, FlowerData flowerData)
//     {
//         SetPrivateField(flower, "_currentStageIndex", flowerData.currentStageIndex);
//         SetPrivateField(flower, "_timeSinceLastWatering", flowerData.timeSinceLastWatering);
//         SetPrivateField(flower, "_timeSinceLastSunGeneration", flowerData.timeSinceLastSunGeneration);
//         SetPrivateField(flower, "_needWater", flowerData.needWater);
//         SetPrivateField(flower, "_needFertilize", flowerData.needFertilize);
//         SetPrivateField(flower, "_isFullyGrown", flowerData.isFullyGrown);
//         SetPrivateField(flower, "_careEventCount", flowerData.careEventCount);

//         flower.transform.position = flowerData.position;

//         if (!string.IsNullOrEmpty(flowerData.spriteName))
//         {
//             var sprite = Resources.Load<Sprite>(flowerData.spriteName);
//             if (sprite != null)
//             {
//                 var spriteRenderer = flower.GetComponent<SpriteRenderer>();
//                 if (spriteRenderer != null)
//                 {
//                     spriteRenderer.sprite = sprite;
//                 }
//             }
//         }
//     }

//     private void RestoreFlowerFromData(FlowerData flowerData)
//     {
//         var prefab = Resources.Load<GameObject>(flowerData.prefabName);
//         if (prefab != null)
//         {
//             var flowerObj = Instantiate(prefab, flowerData.position, Quaternion.identity);
//             var flower = flowerObj.GetComponent<Flower>();
            
//             if (flower != null)
//             {
//                 LoadFlowerState(flower, flowerData);
                
//                 if (!string.IsNullOrEmpty(flowerData.growthConditionsName))
//                 {
//                     var conditions = Resources.Load<GrowthConditions>(flowerData.growthConditionsName);
//                     if (conditions != null)
//                     {
//                         flower.Initialize(conditions);
//                     }
//                 }
//             }
//         }
//     }

//     private void SetPrivateField(object obj, string fieldName, object value)
//     {
//         var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//         if (field != null)
//         {
//             field.SetValue(obj, value);
//         }
//     }

//     #endregion

//     #region Utility Methods

//     /// <summary>
//     /// Clear all save data for a specific zone
//     /// </summary>
//     public void ClearZoneSaveData(string zoneId)
//     {
//         if (YG2.saves != null && YG2.saves.occupiedZones != null)
//         {
//             YG2.saves.occupiedZones.RemoveAll(z => z.zoneId == zoneId);
//             YG2.SaveProgress();
//         }
//     }

//     /// <summary>
//     /// Clear all save data
//     /// </summary>
//     public void ClearAllSaveData()
//     {
//         if (YG2.saves != null)
//         {
//             YG2.saves.occupiedZones = new List<ZoneSaveData>();
//             YG2.saves.gardenData = null;
//             YG2.SaveProgress();
//         }
//     }

//     /// <summary>
//     /// Check if a zone has saved data
//     /// </summary>
//     public bool HasZoneSaveData(string zoneId)
//     {
//         if (YG2.saves == null || YG2.saves.occupiedZones == null) return false;
//         return YG2.saves.occupiedZones.Any(z => z.zoneId == zoneId);
//     }

//     #endregion
// }
