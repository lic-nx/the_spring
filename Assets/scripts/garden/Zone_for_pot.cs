using UnityEngine;
using System.Collections.Generic;
using YG;

public class PotZoneArea : MonoBehaviour, iPotDropArea
{
    public bool isEmpty = true;

    [Header("Настройки сохранения")]
    [Tooltip("Уникальный идентификатор зоны. Если оставить пустым, будет автоматически использовано имя объекта в иерархии.")]
    [SerializeField] private string zoneId;

    // Вспомогательное свойство для красивых и понятных логов
    private string LogPrefix => $"[PotZoneArea | {zoneId}]";

    private void Awake()
    {
        if (string.IsNullOrEmpty(zoneId))
        {
            zoneId = gameObject.name;
            Debug.Log($"{LogPrefix} zoneId был пуст. Автоматически назначено имя объекта: '{zoneId}'.");
        }
        else
        {
            Debug.Log($"{LogPrefix} Инициализация с заданным zoneId: '{zoneId}'.");
        }
    }

    private void Start()
    {
        if (DropZoneManager.Instance != null)
        {
            DropZoneManager.Instance.RegisterZone(this.gameObject);
            Debug.Log($"{LogPrefix} Зона успешно зарегистрирована в DropZoneManager.");
        }
        else
        {
            Debug.LogWarning($"{LogPrefix} DropZoneManager.Instance не найден! Регистрация пропущена.");
        }
        
        this.gameObject.SetActive(false); 
        Debug.Log($"{LogPrefix} Объект зоны деактивирован (SetActive(false)).");
        
        Debug.Log($"{LogPrefix} Начало загрузки состояния зоны из сохранения...");
        // LoadZoneState();
    }

    //  private void LoadZoneState()
    //  {
    //      if (GameSaveManager.Instance != null)
    //      {
    //          Debug.Log($"{LogPrefix} Используем GameSaveManager для загрузки состояния зоны...");
    //          GameSaveManager.Instance.LoadZoneState(this);
    //          return;
    //      }

    //      if (YG2.saves == null) 
    //      {
    //          Debug.LogWarning($"{LogPrefix} YG2.saves равен null. Загрузка невозможна (первый запуск?).");
    //          return;
    //      }
    //      if (YG2.saves.occupiedZones == null) 
    //      {
    //          Debug.Log($"{LogPrefix} Список occupiedZones пуст или null. Зона считается пустой.");
    //          return;
    //      }
 
    //      Debug.Log($"{LogPrefix} Поиск данных зоны в сохранении. Всего записей в сейве: {YG2.saves.occupiedZones.Count}");
 
    //      bool found = false;
    //      foreach (var zoneData in YG2.saves.occupiedZones)
    //      {
    //          if (zoneData.zoneId == this.zoneId)
    //          {
    //              found = true;
    //              if (!string.IsNullOrEmpty(zoneData.potSpriteName))
    //              {
    //                  Debug.Log($"{LogPrefix} Найдено сохранение! Спрайт горшка: '{zoneData.potSpriteName}'. Запуск восстановления...");
    //                  RestorePot(zoneData.potSpriteName);
    //              }
    //              else
    //              {
    //                  Debug.Log($"{LogPrefix} Запись для зоны найдена, но potSpriteName пуст. Зона останется пустой.");
    //              }
    //              break;
    //          }
    //      }
 
    //      if (!found)
    //      {
    //          Debug.Log($"{LogPrefix} Записи для этой зоны в сохранении не найдено. Зона пуста.");
    //      }
    //  }

    private void RestorePot(string spriteName)
    {
        Debug.Log($"{LogPrefix} RestorePot: Начало создания горшка со спрайтом '{spriteName}'.");

        if (Shop.Instance == null || Shop.Instance.potDragDropPrefab == null)
        {
            Debug.LogError($"{LogPrefix} ОШИБКА: Не удалось найти Shop.Instance или potDragDropPrefab для восстановления горшка!");
            return;
        }

        // Возвращен parent (this.transform), чтобы localPosition работал корректно
        GameObject potObj = Instantiate(Shop.Instance.potDragDropPrefab); 
        potObj.transform.localPosition = Vector3.zero;
        potObj.transform.localRotation = Quaternion.identity;
        Debug.Log($"{LogPrefix} RestorePot: Префаб инстанцирован и привязан к зоне. Локальная позиция сброшена.");

        SpriteRenderer potSpriteRenderer = potObj.GetComponent<SpriteRenderer>();
        if (potSpriteRenderer != null)
        {
            Sprite savedSprite = Shop.Instance.GetPotSpriteByName(spriteName);
            if (savedSprite != null)
            {
                potSpriteRenderer.sprite = savedSprite;
                Debug.Log($"{LogPrefix} RestorePot: Спрайт '{spriteName}' успешно назначен.");
            }
            else
            {
                Debug.LogWarning($"{LogPrefix} RestorePot: Спрайт '{spriteName}' не найден в магазине! Оставлен дефолтный.");
            }
        }
        else
        {
            Debug.LogWarning($"{LogPrefix} RestorePot: На префабе отсутствует компонент SpriteRenderer!");
        }

        isEmpty = false;
        Debug.Log($"{LogPrefix} RestorePot: Статус зоны изменен на isEmpty = false.");
        
        var potComponent = potObj.GetComponent<Pot>();
        if (potComponent != null)
        {
            potComponent.SetCurrentZone(this);
            potComponent.AlignToZone(this.transform);
            Debug.Log($"{LogPrefix} RestorePot: Логическая связь с компонентом Pot установлена.");
        }
        else
        {
            Debug.LogWarning($"{LogPrefix} RestorePot: На префабе отсутствует компонент Pot! Связь не установлена.");
        }
        
        Debug.Log($"{LogPrefix} RestorePot: ГОТОВО. Зона успешно восстановлена.");
    }

    public bool OnPotDrop(GameObject pot)
    {
        Debug.Log($"{LogPrefix} OnPotDrop: Получен горшок '{pot.name}'. Текущий статус isEmpty: {isEmpty}.");
        
        if (!isEmpty)
        {
            Debug.LogWarning($"{LogPrefix} OnPotDrop: ОТКЛОНЕНО. Зона уже занята!");
            return false;
        }
        
        var potComponent = pot.GetComponent<Pot>();
        if (potComponent != null)
        {
            var oldZone = potComponent.CurrentZone;
            if (oldZone != null && oldZone != this)
            {
                oldZone.FreeZone();
            }
            
            Debug.Log($"{LogPrefix} OnPotDrop: Компонент Pot найден. Выполняется привязка...");
            isEmpty = false;
            potComponent.AlignToZone(this.transform);
            potComponent.SetCurrentZone(this);
            
            // // SaveZoneState(potComponent);
            
            Debug.Log($"{LogPrefix} OnPotDrop: Горшок успешно установлен и сохранен.");
            return true;
        }
        
        Debug.Log($"{LogPrefix} OnPotDrop: Компонент Pot НЕ найден. Используется фоллбэк-логика позиционирования.");
        Transform zoneAttach = transform.childCount > 0 ? transform.GetChild(0) : transform;
        Transform potAttach = pot.transform.childCount > 0 ? pot.transform.GetChild(0) : pot.transform;
        Vector3 originalOffset = potAttach.position - pot.transform.position;
        pot.transform.position = zoneAttach.position - originalOffset;
        potAttach.position = zoneAttach.position;
        isEmpty = false;
        Debug.Log($"{LogPrefix} OnPotDrop: Фоллбэк-позиционирование завершено. isEmpty = false.");
        
        // В оригинальном коде здесь повторная проверка, оставляем как есть
        potComponent = pot.GetComponent<Pot>();
        if (potComponent != null)
        {
            var oldZone = potComponent.CurrentZone;
            if (oldZone != null && oldZone != this)
            {
                oldZone.FreeZone();
            }
            potComponent.SetCurrentZone(this);
            // // SaveZoneState(potComponent);
        }
        else
        {
            Debug.Log($"{LogPrefix} OnPotDrop: Сохранение через имя объекта (Fallback): '{pot.name}'.");
            // SaveZoneState(pot.name);
        }
        
        return true;
    }

    //  private void // SaveZoneState(Pot potComponent)
    //  {
    //      if (GameSaveManager.Instance != null)
    //      {
    //          Debug.Log($"{LogPrefix} Используем GameSaveManager для сохранения состояния зоны...");
    //          GameSaveManager.Instance.// SaveZoneState(this, potComponent);
    //          return;
    //      }

    //      string spriteName = "Unknown";
    //      SpriteRenderer sr = potComponent.GetComponent<SpriteRenderer>();
    //      if (sr != null && sr.sprite != null)
    //      {
    //          spriteName = sr.sprite.name;
    //          Debug.Log($"{LogPrefix} // SaveZoneState(Pot): Извлечено имя спрайта: '{spriteName}'.");
    //      }
    //      else
    //      {
    //          Debug.LogWarning($"{LogPrefix} // SaveZoneState(Pot): Не удалось найти SpriteRenderer или Sprite! Будет сохранено как '{spriteName}'.");
    //      }
    //      // SaveZoneState(spriteName);
    //  }

    //  private void // SaveZoneState(string spriteName)
    //  {
    //      Debug.Log($"{LogPrefix} // SaveZoneState(string): --- НАЧАЛО ПРОЦЕССА СОХРАНЕНИЯ --- Спрайт: '{spriteName}'");
 
    //      if (YG2.saves == null)
    //      {
    //          Debug.LogError($"{LogPrefix} // SaveZoneState: КРИТИЧЕСКАЯ ОШИБКА! YG2.saves is null. Сохранение прервано.");
    //          return;
    //      }
         
    //      if (YG2.saves.occupiedZones == null)
    //      {
    //          Debug.Log($"{LogPrefix} // SaveZoneState: Список occupiedZones был null. Инициализация нового списка...");
    //          YG2.saves.occupiedZones = new List<ZoneSaveData>();
    //      }
 
    //      int removedCount = YG2.saves.occupiedZones.RemoveAll(z => z.zoneId == this.zoneId);
    //      if (removedCount > 0)
    //      {
    //          Debug.Log($"{LogPrefix} // SaveZoneState: Удалено старых записей для этой зоны: {removedCount}.");
    //      }
    //      else
    //      {
    //          Debug.Log($"{LogPrefix} // SaveZoneState: Старых записей для этой зоны не найдено (создается новая).");
    //      }
 
    //      YG2.saves.occupiedZones.Add(new ZoneSaveData
    //      {
    //          zoneId = this.zoneId,
    //          potSpriteName = spriteName
    //      });
    //      Debug.Log($"{LogPrefix} // SaveZoneState: Новая запись добавлена. Текущее количество зон в сейве: {YG2.saves.occupiedZones.Count}.");
 
    //      Debug.Log($"{LogPrefix} // SaveZoneState: Вызов YG2.SaveProgress()...");
    //      YG2.SaveProgress();
    //      Debug.Log($"{LogPrefix} // SaveZoneState: --- СОХРАНЕНИЕ УСПЕШНО ЗАВЕРШЕНО ---");
    //  }

     public void FreeZone()
     {
         Debug.Log($"{LogPrefix} FreeZone: Начало очистки зоны. Текущий isEmpty: {isEmpty}.");
         isEmpty = true;
         Debug.Log($"{LogPrefix} FreeZone: Статус изменен на isEmpty = true.");
         
        //  if (GameSaveManager.Instance != null)
        //  {
        //      Debug.Log($"{LogPrefix} FreeZone: Используем GameSaveManager для очистки сохранения...");
        //      GameSaveManager.Instance.ClearZoneSaveData(this.zoneId);
        //  }
        //  else if (YG2.saves != null && YG2.saves.occupiedZones != null)
        //  {
        //      int removedCount = YG2.saves.occupiedZones.RemoveAll(z => z.zoneId == this.zoneId);
        //      Debug.Log($"{LogPrefix} FreeZone: Удалено записей из сейва: {removedCount}.");
             
        //      Debug.Log($"{LogPrefix} FreeZone: Вызов YG2.SaveProgress() для фиксации очистки...");
        //      YG2.SaveProgress();
        //      Debug.Log($"{LogPrefix} FreeZone: Очистка успешно сохранена в Яндексе!");
        //  }
        //  else
        //  {
        //      Debug.LogWarning($"{LogPrefix} FreeZone: YG2.saves или occupiedZones равны null. Очистка из сейва пропущена.");
        //  }
         
         Debug.Log($"{LogPrefix} FreeZone: Процесс завершен. Зона полностью свободна.");
     }
}