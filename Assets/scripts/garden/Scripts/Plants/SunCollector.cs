using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Скрипт для обработки клика по солнышку.
/// Работает через EventSystem (IPointerClickHandler), что гарантирует работу на мобилках.
/// 
/// ТРЕБОВАНИЯ:
/// 1. На объекте должен быть CircleCollider2D (isTrigger = true)
/// 2. На камере должен быть Physics2DRaycaster
/// 3. На сцене должен быть EventSystem
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class SunCollector : MonoBehaviour, IPointerClickHandler
{
    private Flower _parentFlower;

    private void Awake()
    {
        // Ищем родительский Flower
        _parentFlower = GetComponentInParent<Flower>();
        if (_parentFlower == null)
        {
            Debug.LogError($"[SunCollector] Не найден родительский Flower у объекта '{gameObject.name}'!");
        }

        // Убеждаемся, что коллайдер — триггер
        var collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    /// <summary>
    /// Вызывается при клике на солнышко (работает и на ПК, и на мобилках).
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_parentFlower != null)
        {
            _parentFlower.CollectSun();
        }
    }
}