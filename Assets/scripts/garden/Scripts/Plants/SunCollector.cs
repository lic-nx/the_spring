using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Скрипт для обработки клика по солнышку.
/// Работает через EventSystem (IPointerClickHandler), что гарантирует работу на мобилках.
/// 
/// ТРЕБОВАНИЯ:
/// 1. На объекте должен быть Collider2D (желательно isTrigger = true для Physics2DRaycaster)
/// 2. На камере должен быть Physics2DRaycaster
/// 3. На сцене должен быть EventSystem
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SunCollector : MonoBehaviour, IPointerClickHandler
{
    private Flower _parentFlower;

    private void Awake()
    {
        // Ищем родительский Flower
        _parentFlower = GetComponentInParent<Flower>();
        if (_parentFlower == null)
        {
            _parentFlower = GetComponent<Flower>();
        }
        if (_parentFlower == null)
        {
            Debug.LogError($"[SunCollector] Не найден родительский Flower у объекта '{gameObject.name}'!");
        }

        // Для Physics2DRaycaster надёжнее, когда коллайдер настроен как триггер.
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    private void OnEnable()
    {
        if (_parentFlower == null)
        {
            _parentFlower = GetComponentInParent<Flower>();
            if (_parentFlower == null)
            {
                _parentFlower = GetComponent<Flower>();
            }
        }
    }

    private void OnMouseDown()
    {
        TryCollect();
    }

    /// <summary>
    /// Вызывается при клике на солнышко (работает и на ПК, и на мобилках).
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        TryCollect();
    }

    private void TryCollect()
    {
        if (_parentFlower == null)
        {
            _parentFlower = GetComponentInParent<Flower>();
            if (_parentFlower == null)
            {
                _parentFlower = GetComponent<Flower>();
            }
        }

        if (_parentFlower != null)
        {
            _parentFlower.CollectSun();
        }
    }
}
