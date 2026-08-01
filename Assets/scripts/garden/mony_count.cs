using UnityEngine;
using TMPro;
using YG;

/// <summary>
/// Простой UI-счётчик валюты. Вешается на TMP_Text.
/// Автоматически обновляется при изменении валюты.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class CurrencyUIText : MonoBehaviour
{
    [Tooltip("Формат отображения. {0} = количество. Например: '💰 {0}'")]
    [SerializeField] private string format = "{0}";

    private TMP_Text _text;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged += UpdateText;
            UpdateText(YG2.saves.Coins);
        }
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= UpdateText;
        }
    }

    private void UpdateText(int amount)
    {
        if (_text != null)
        {
            _text.text = string.Format(format, amount);
        }
    }
}