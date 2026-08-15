using UnityEngine;
using System.Collections.Generic;
using YG;

public class LeftDropArea : MonoBehaviour, iPotDropArea
{
    public bool isEmpty = true;

    [Header("Настройки сохранения")]
    [Tooltip("Уникальный идентификатор зоны. Если оставить пустым, будет автоматически использовано имя объекта в иерархии.")]
    [SerializeField] private string zoneId;

    private void Awake()
    {
        if (string.IsNullOrEmpty(zoneId))
        {
            zoneId = gameObject.name;
        }
    }

    private void Start()
    {
        if (DropZoneManager.Instance != null)
        {
            DropZoneManager.Instance.RegisterZone(this.gameObject);
        }
        
        this.gameObject.SetActive(false); 
        
        LoadZoneState();
    }

    private void LoadZoneState()
    {
        if (YG2.saves == null || YG2.saves.occupiedZones == null) return;

        foreach (var zoneData in YG2.saves.occupiedZones)
        {
            if (zoneData.zoneId == this.zoneId && !string.IsNullOrEmpty(zoneData.potSpriteName))
            {
                RestorePot(zoneData.potSpriteName);
                break;
            }
        }
    }

    private void RestorePot(string spriteName)
    {
        if (Shop.Instance == null || Shop.Instance.potDragDropPrefab == null)
        {
            Debug.LogError("[LeftDropArea] Не удалось найти Shop.Instance или potDragDropPrefab для восстановления горшка!");
            return;
        }

        GameObject potObj = Instantiate(Shop.Instance.potDragDropPrefab, this.transform);
        potObj.transform.localPosition = Vector3.zero;
        potObj.transform.localRotation = Quaternion.identity;

        SpriteRenderer potSpriteRenderer = potObj.GetComponent<SpriteRenderer>();
        if (potSpriteRenderer != null)
        {
            Sprite savedSprite = Shop.Instance.GetPotSpriteByName(spriteName);
            if (savedSprite != null)
            {
                potSpriteRenderer.sprite = savedSprite;
            }
            else
            {
                Debug.LogWarning($"[LeftDropArea] Спрайт '{spriteName}' не найден в списке магазина!");
            }
        }

        isEmpty = false;
        
        var potComponent = potObj.GetComponent<Pot>();
        if (potComponent != null)
        {
            potComponent.SetCurrentZone(this);
            potComponent.AlignToZone(this.transform);
        }
        
        Debug.Log($"[LeftDropArea] Зона '{zoneId}' восстановлена с горшком '{spriteName}'.");
    }

    public bool OnPotDrop(GameObject pot)
    {
        if (!isEmpty)
        {
            Debug.Log("Зона уже занята! Горшок не установлен.");
            return false;
        }

        var potComponent = pot.GetComponent<Pot>();
        if (potComponent != null)
        {
            isEmpty = false;
            potComponent.AlignToZone(this.transform);
            potComponent.SetCurrentZone(this);
            
            SaveZoneState(potComponent);
            
            Debug.Log("Горшок успешно установлен в левую зону.");
            return true;
        }

        Transform zoneAttach = transform.childCount > 0 ? transform.GetChild(0) : transform;
        Transform potAttach = pot.transform.childCount > 0 ? pot.transform.GetChild(0) : pot.transform;
        Vector3 originalOffset = potAttach.position - pot.transform.position;
        pot.transform.position = zoneAttach.position - originalOffset;
        potAttach.position = zoneAttach.position;
        isEmpty = false;
        
        potComponent = pot.GetComponent<Pot>();
        if (potComponent != null)
        {
            potComponent.SetCurrentZone(this);
            SaveZoneState(potComponent);
        }
        else
        {
            SaveZoneState(pot.name);
        }
        
        return true;
    }

    private void SaveZoneState(Pot potComponent)
    {
        string spriteName = "Unknown";
        SpriteRenderer sr = potComponent.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            spriteName = sr.sprite.name;
        }
        SaveZoneState(spriteName);
    }

    private void SaveZoneState(string spriteName)
    {
        // ДОБАВЛЕНА ПРОВЕРКА НА NULL
        if (YG2.saves == null)
        {
            Debug.LogWarning("[LeftDropArea] YG2.saves is null, cannot save zone state.");
            return;
        }
        
        if (YG2.saves.occupiedZones == null)
        {
            YG2.saves.occupiedZones = new List<ZoneSaveData>();
        }

        YG2.saves.occupiedZones.RemoveAll(z => z.zoneId == this.zoneId);

        YG2.saves.occupiedZones.Add(new ZoneSaveData
        {
            zoneId = this.zoneId,
            // zoneEmpty = this.isEmpty;
            potSpriteName = spriteName
        });

        YG2.SaveProgress();
    }

    public void FreeZone()
    {
        isEmpty = true;
        
        // ДОБАВЛЕНА ПРОВЕРКА НА NULL
        if (YG2.saves != null && YG2.saves.occupiedZones != null)
        {
            YG2.saves.occupiedZones.RemoveAll(z => z.zoneId == this.zoneId);
            YG2.SaveProgress();
        }
        
        Debug.Log("Зона освобождена.");
    }
}