using UnityEngine;
using UnityEngine.UI;
using YG;

public class soundEffectController : MonoBehaviour
{
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private button_pushsound audioButton; // ← Новая ссылка
    private bool isEnabled = false;

    void Start()
    {
        isEnabled = YG2.saves.EffectMusicEnabled;

        if (musicToggle != null)
        {
            musicToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            musicToggle.isOn = !isEnabled;
            musicToggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }

    private void OnToggleValueChanged(bool isOn)
    {
        isEnabled = isOn;
        YG2.saves.EffectMusicEnabled = !isOn;
        YG2.SaveProgress();

        // ✅ Вызов через экземпляр с проверкой на null
        audioButton?.ButtonPush();

        Debug.Log($"Звук сохранён: {!isOn}");
    }
}