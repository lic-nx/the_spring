using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SeedItem", menuName = "Garden/Seed Item")]
public class SeedItem : ScriptableObject
{
    public string name = "Ромашка";
    public GameObject flowerPrefab;
    public List<GrowthConditions> growthConditionsList = new List<GrowthConditions>();
    public List<int> weights = new List<int>();
    public Sprite seedSprite;
    public int price = 10;

    // Этот метод вызывается редактором Unity при изменении полей в Inspector
    private void OnValidate()
    {
        SyncLists();
    }

    // Логика синхронизации
    private void SyncLists()
    {
        if (growthConditionsList == null) growthConditionsList = new List<GrowthConditions>();
        if (weights == null) weights = new List<int>();

        // Если условий больше, чем весов -> добавляем веса со значением по умолчанию (например, 10)
        while (weights.Count < growthConditionsList.Count)
        {
            weights.Add(10); 
        }

        // Если весов больше, чем условий -> удаляем лишние веса с конца
        while (weights.Count > growthConditionsList.Count)
        {
            weights.RemoveAt(weights.Count - 1);
        }
    }

    // Дополнительная кнопка в контекстном меню (правый клик по скрипту в Inspector) 
    // для принудительной синхронизации, если что-то пошло не так
    [ContextMenu("Синхронизировать списки")]
    private void ForceSync()
    {
        SyncLists();
        Debug.Log("Списки синхронизированы. Количество элементов: " + weights.Count);
    }
}