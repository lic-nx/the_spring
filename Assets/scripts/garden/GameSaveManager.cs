using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

/// <summary>
/// Менеджер сохранений и загрузок сада.
/// Единственный скрипт, который отвечает за сохранение и загрузку зон для горшков,
/// горшков и цветов. Зоны, горшки и цветы просто передают в него свои значения
/// (SaveZone / SavePot / SaveFlower) или получают их обратно при загрузке (LoadGameState).
/// </summary>
public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance
    {
        get
        {
            EnsureExists();
            return _instance;
        }
    }
    private static GameSaveManager _instance;

    /// <summary>true, пока идёт восстановление сада из сохранения (сохранение в этот момент заблокировано).</summary>
    public static bool IsLoading { get; private set; }

    [Header("Настройки сохранений")]
    [Tooltip("Автоматически загружать сохранение сада при входе в сцену.")]
    public bool loadOnSceneLoad = true;

    private bool _loadScheduled = false;
    private int _pendingSceneHandle = int.MinValue;
    private int _loadedSceneHandle = int.MinValue;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>Создаёт менеджер, если он ещё не существует на сцене.</summary>
    public static void EnsureExists()
    {
        if (_instance == null)
        {
            var go = new GameObject("GameSaveManager");
            _instance = go.AddComponent<GameSaveManager>();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _loadedSceneHandle = int.MinValue;
        if (loadOnSceneLoad) RequestLoad();
    }

    /// <summary>
    /// Запросить загрузку сохранения для текущей сцены.
    /// Вызывается зонами при их запуске, чтобы гарантировать загрузку даже при ленивом создании менеджера.
    /// </summary>
    public static void RequestLoad()
    {
        EnsureExists();
        _instance.ScheduleLoad(SceneManager.GetActiveScene().handle);
    }

    private void ScheduleLoad(int sceneHandle)
    {
        if (_loadScheduled && _pendingSceneHandle == sceneHandle) return;
        _loadScheduled = true;
        _pendingSceneHandle = sceneHandle;
        StartCoroutine(LoadAfterFrame(sceneHandle));
    }

    private IEnumerator LoadAfterFrame(int sceneHandle)
    {
        // Ждём кадр, чтобы все зоны успели зарегистрироваться в DropZoneManager (Start).
        yield return null;
        _loadScheduled = false;
        if (sceneHandle != _loadedSceneHandle)
        {
            _loadedSceneHandle = sceneHandle;
            LoadGameState();
        }
    }

    // ============================= СОХРАНЕНИЕ =============================

    private GardenSaveData GardenData
    {
        get
        {
            if (YG2.saves == null) return null;
            if (YG2.saves.gardenData == null)
            {
                YG2.saves.gardenData = new GardenSaveData
                {
                    currency = YG2.saves.Coins
                };
            }
            return YG2.saves.gardenData;
        }
    }

    /// <summary>Сохраняет состояние зоны (какой спрайт горшка в ней стоит).</summary>
    public void SaveZone(string zoneId, string potSpriteName)
    {
        if (IsLoading) return;
        if (YG2.saves == null) return;
        if (YG2.saves.occupiedZones == null) YG2.saves.occupiedZones = new List<ZoneSaveData>();

        YG2.saves.occupiedZones.RemoveAll(z => z.zoneId == zoneId);

        if (string.IsNullOrEmpty(potSpriteName))
        {
            YG2.SaveProgress();
            return;
        }

        YG2.saves.occupiedZones.Add(new ZoneSaveData
        {
            zoneId = zoneId,
            potSpriteName = potSpriteName
        });
        YG2.SaveProgress();
    }

    /// <summary>Освобождает зону: удаляет её запись из сохранения.</summary>
    public void FreeZone(string zoneId)
    {
        if (IsLoading) return;
        if (string.IsNullOrEmpty(zoneId)) return;
        if (YG2.saves == null || YG2.saves.occupiedZones == null) return;

        YG2.saves.occupiedZones.RemoveAll(z => z.zoneId == zoneId);
        YG2.SaveProgress();
    }

    /// <summary>Сохраняет состояние горшка (собирает значения из Pot).</summary>
    public void SavePot(Pot pot)
    {
        if (IsLoading || pot == null) return;

        var flower = pot.CurrentFlower;
        var data = new PotSaveData
        {
            potId = pot.PotId,
            spriteName = pot.PotSpriteName,
            zoneId = GetZoneIdOf(pot.CurrentZone),
            position = pot.transform.position,
            isOccupied = flower != null,
            flowerId = flower != null ? flower.FlowerId : string.Empty
        };
        SavePot(data);

        // Горшок мог изменить зону или позицию — обновляем и запись цветка,
        // чтобы при загрузке он не оказался смещён относительно горшка.
        if (flower != null)
        {
            SaveFlower(flower);
        }
    }

    /// <summary>Сохраняет переданные данные горшка.</summary>
    public void SavePot(PotSaveData data)
    {
        if (IsLoading) return;
        var garden = GardenData;
        if (garden == null || data == null) return;

        garden.pots.RemoveAll(p => p.potId == data.potId);
        garden.pots.Add(data);
        garden.currency = YG2.saves.Coins;
        YG2.SaveProgress();
    }

    /// <summary>Удаляет горшок из сохранения.</summary>
    public void RemovePot(string potId)
    {
        if (IsLoading || string.IsNullOrEmpty(potId)) return;
        var garden = GardenData;
        if (garden == null) return;

        garden.pots.RemoveAll(p => p.potId == potId);
        YG2.SaveProgress();
    }

    /// <summary>Сохраняет состояние цветка (собирает значения из Flower).</summary>
    public void SaveFlower(Flower flower)
    {
        if (IsLoading || flower == null) return;

        var data = new FlowerSaveData
        {
            flowerId = flower.FlowerId,
            prefabName = flower.PrefabName,
            spriteName = flower.SpriteName,
            position = flower.transform.position,
            currentStageIndex = flower.CurrentStageIndex,
            timeSinceLastWatering = flower.TimeSinceLastWatering,
            timeSinceLastSunGeneration = flower.TimeSinceLastSunGeneration,
            needWater = flower.NeedWater,
            needFertilize = flower.NeedFertilize,
            isFullyGrown = flower.IsFullyGrown,
            careEventCount = flower.CareEventCount,
            growthConditionsName = flower.GrowthConditionsName,
            hasGivenSun = flower.HasGivenSun
        };
        SaveFlower(data);
    }

    /// <summary>Сохраняет переданные данные цветка.</summary>
    public void SaveFlower(FlowerSaveData data)
    {
        if (IsLoading) return;
        var garden = GardenData;
        if (garden == null || data == null) return;

        garden.flowers.RemoveAll(f => f.flowerId == data.flowerId);
        garden.flowers.Add(data);
        garden.currency = YG2.saves.Coins;
        YG2.SaveProgress();
    }

    /// <summary>Удаляет цветок из сохранения.</summary>
    public void RemoveFlower(string flowerId)
    {
        if (IsLoading || string.IsNullOrEmpty(flowerId)) return;
        var garden = GardenData;
        if (garden == null) return;

        garden.flowers.RemoveAll(f => f.flowerId == flowerId);
        YG2.SaveProgress();
    }

    /// <summary>Удаляет горшок и все связанные с ним данные из сохранения.</summary>
    public void RemovePotAndContents(Pot pot)
    {
        if (pot == null) return;

        if (pot.CurrentFlower != null && GameSaveManager.Instance != null)
        {
            RemoveFlower(pot.CurrentFlower.FlowerId);
        }
        RemovePot(pot.PotId);
    }

    // ============================= ИНВЕНТАРЬ =============================

    /// <summary>Сохраняет инвентарь (список семян и их количество).</summary>
    public void SaveInventory()
    {
        if (IsLoading) return;
        if (YG2.saves == null) return;
        if (InventoryManager.Instance == null) return;

        YG2.saves.inventory = new List<InventorySlotData>();
        foreach (var pair in InventoryManager.Instance.GetAllItems())
        {
            if (pair.Key == null || pair.Value <= 0) continue;
            YG2.saves.inventory.Add(new InventorySlotData
            {
                seedId = pair.Key.name,
                quantity = pair.Value
            });
        }
        YG2.SaveProgress();
    }

    /// <summary>Восстанавливает содержимое инвентаря из сохранения.</summary>
    private void LoadInventoryFromSave()
    {
        if (InventoryManager.Instance == null || YG2.saves.inventory == null) return;

        var data = new Dictionary<SeedItem, int>();
        foreach (var slot in YG2.saves.inventory)
        {
            if (slot == null || slot.quantity <= 0 || string.IsNullOrEmpty(slot.seedId)) continue;

            var seed = ResolveSeedItem(slot.seedId);
            if (seed == null)
            {
                Debug.LogWarning($"[GameSaveManager] Семя '{slot.seedId}' не найдено в магазине. Пропущено.");
                continue;
            }

            data[seed] = slot.quantity;
        }

        InventoryManager.Instance.ApplyInventory(data);
    }

    private SeedItem ResolveSeedItem(string seedId)
    {
        if (Shop.Instance != null && Shop.Instance.availableSeedsForSale != null)
        {
            foreach (var seed in Shop.Instance.availableSeedsForSale)
            {
                if (seed != null && seed.name == seedId) return seed;
            }
        }
        return null;
    }

    // ============================= ЗАГРУЗКА =============================

    /// <summary>Восстанавливает сад из сохранения: сначала зоны и горшки, затем цветы.</summary>
    public void LoadGameState()
    {
        if (YG2.saves == null)
        {
            Debug.LogWarning("[GameSaveManager] YG2.saves ещё не готов. Загрузка пропущена.");
            return;
        }

        IsLoading = true;
        try
        {
            LoadZonesFromSave();
            LoadFlowersFromSave();
            LoadInventoryFromSave();
            Debug.Log("[GameSaveManager] Состояние сада загружено.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadZonesFromSave()
    {
        var garden = YG2.saves.gardenData;

        // Основной путь: восстанавливаем горшки по gardenData.pots — у каждой записи
        // есть zoneId и spriteName, поэтому горшок восстановится даже если запись
        // в occupiedZones отсутствует (горшок мог быть размещён редактором или иначе).
        if (garden != null && garden.pots != null && garden.pots.Count > 0)
        {
            foreach (var potData in garden.pots)
            {
                if (potData == null || string.IsNullOrEmpty(potData.zoneId)) continue;

                var zone = FindZone(potData.zoneId);
                if (zone == null) continue;

                zone.RestorePot(potData.spriteName);

                var pot = FindPotInZone(potData.zoneId);
                if (pot != null) pot.AssignId(potData.potId);
            }
            return;
        }

        // Fallback для старых сохранений, где были только occupiedZones без gardenData.
        if (DropZoneManager.Instance == null || YG2.saves.occupiedZones == null) return;

        foreach (var zoneObj in DropZoneManager.Instance.GetAllZones())
        {
            if (zoneObj == null) continue;
            var zone = zoneObj.GetComponent<PotZoneArea>();
            if (zone == null) continue;

            var data = YG2.saves.occupiedZones.FirstOrDefault(z => z.zoneId == zone.ZoneId);
            if (data == null || string.IsNullOrEmpty(data.potSpriteName)) continue;

            zone.RestorePot(data.potSpriteName);
        }
    }

    private PotZoneArea FindZone(string zoneId)
    {
        if (DropZoneManager.Instance == null) return null;
        foreach (var zoneObj in DropZoneManager.Instance.GetAllZones())
        {
            if (zoneObj == null) continue;
            var zone = zoneObj.GetComponent<PotZoneArea>();
            if (zone != null && zone.ZoneId == zoneId) return zone;
        }
        return null;
    }

    private void LoadFlowersFromSave()
    {
        var garden = YG2.saves?.gardenData;
        if (garden == null || garden.pots == null || garden.flowers == null) return;

        foreach (var potData in garden.pots)
        {
            if (!potData.isOccupied || string.IsNullOrEmpty(potData.flowerId)) continue;
            if (string.IsNullOrEmpty(potData.zoneId)) continue;

            var flowerData = garden.flowers.FirstOrDefault(f => f.flowerId == potData.flowerId);
            if (flowerData == null) continue;

            var pot = FindPotInZone(potData.zoneId);
            if (pot == null || pot.CurrentFlower != null) continue;

            RestoreFlowerIntoPot(pot, flowerData);
        }
    }

    private Pot FindPotInZone(string zoneId)
    {
        foreach (var pot in FindObjectsOfType<Pot>())
        {
            if (GetZoneIdOf(pot.CurrentZone) == zoneId) return pot;
        }
        return null;
    }

    private string GetZoneIdOf(iPotDropArea area)
    {
        var zone = area as PotZoneArea;
        return zone != null ? zone.ZoneId : string.Empty;
    }

    private void RestoreFlowerIntoPot(Pot pot, FlowerSaveData data)
    {
        var prefab = ResolveFlowerPrefab(data.prefabName);
        if (prefab == null)
        {
            Debug.LogError($"[GameSaveManager] Не удалось найти префаб цветка '{data.prefabName}'. Цветок не восстановлен.");
            return;
        }

        var flowerObj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        var flower = flowerObj.GetComponent<Flower>();
        if (flower == null)
        {
            Destroy(flowerObj);
            Debug.LogError($"[GameSaveManager] На префабе '{prefab.name}' отсутствует компонент Flower.");
            return;
        }

        if (!string.IsNullOrEmpty(data.growthConditionsName))
        {
            var conditions = ResolveGrowthConditions(data.growthConditionsName);
            if (conditions == null)
            {
                Debug.LogWarning($"[GameSaveManager] Условия роста '{data.growthConditionsName}' не найдены. Используются условия префаба.");
            }
            else
            {
                flower.Initialize(conditions);
            }
        }

        if (!string.IsNullOrEmpty(data.flowerId))
        {
            flower.AssignFlowerId(data.flowerId);
        }

        if (!pot.PlantFlower(flower))
        {
            Destroy(flowerObj);
            return;
        }

        flower.LoadFromData(data);
    }

    private GameObject ResolveFlowerPrefab(string name)
    {
        var prefabName = string.IsNullOrEmpty(name) ? "Flower" : name;

        if (Shop.Instance != null && Shop.Instance.availableSeedsForSale != null)
        {
            foreach (var seed in Shop.Instance.availableSeedsForSale)
            {
                if (seed != null && seed.flowerPrefab != null && seed.flowerPrefab.name == prefabName)
                    return seed.flowerPrefab;
            }
        }

        var resourcePrefab = Resources.Load<GameObject>(prefabName);
        if (resourcePrefab != null) return resourcePrefab;
        return Resources.Load<GameObject>("Flower");
    }

    private GrowthConditions ResolveGrowthConditions(string name)
    {
        if (Shop.Instance != null && Shop.Instance.availableSeedsForSale != null)
        {
            foreach (var seed in Shop.Instance.availableSeedsForSale)
            {
                if (seed == null || seed.growthConditionsList == null) continue;
                foreach (var condition in seed.growthConditionsList)
                {
                    if (condition != null && condition.name == name) return condition;
                }
            }
        }

        var resourceCondition = Resources.Load<GrowthConditions>(name);
        if (resourceCondition != null) return resourceCondition;
        return Resources.Load<GrowthConditions>("GrowthConditions");
    }
}