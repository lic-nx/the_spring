using UnityEngine;
using UnityEngine.UI;
using YG;

public class soundEffectController : MonoBehaviour
{
    [SerializeField] private Toggle musicToggle;
    private bool isEnabled = false;

    void Start()
    {
        // Читаем сохранение
        isEnabled = YG2.saves.EffectMusicEnabled;

        if (musicToggle != null)
        {
            // ✅ Безопасная установка значения без триггера события
            musicToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            musicToggle.isOn = !isEnabled;
            musicToggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }

    private void OnToggleValueChanged(bool isOn)
    {
        isEnabled = isOn;

        // ✅ Просто сохраняем то, что пришло от Toggle
        YG2.saves.EffectMusicEnabled = !isOn;
        YG2.SaveProgress();

        Debug.Log($"Звук сохранён: {!isOn}");
    }
}