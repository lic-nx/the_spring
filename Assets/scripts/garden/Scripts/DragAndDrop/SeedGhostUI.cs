using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-компонент для "призрака" семени при перетаскивании из инвентаря.
/// Это простой Image на Canvas, который следует за курсором/пальцем.
/// </summary>
public class SeedGhostUI : MonoBehaviour
{
    [Header("UI Элементы")]
    public Image iconImage;
    
    /// <summary>
    /// Настройка призрака: устанавливает спрайт семени.
    /// </summary>
    public void Setup(Sprite sprite)
    {
        if (iconImage != null && sprite != null)
        {
            iconImage.sprite = sprite;
        }
        else
        {
            Debug.LogWarning("[SeedGhostUI] Не удалось установить спрайт. Проверьте iconImage или sprite.");
        }
    }
}