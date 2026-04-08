using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    private string currentLang;
    public System.Action OnLanguageChanged;

    private Dictionary<string, string> ru = new Dictionary<string, string>()
    {
        {"play", "Играть"},
        {"settings", "Настройки" },
        {"level", "Уровень {0}" },
        {"levels", "Уровни"},
        {"help", "Помоги"},
        {"flower", "ЦВЕТКУ"},
        {"reach", "добраться до"},
        {"sun", "СОЛНЦА"},
        {"t1", "Нажимай на треснувшую почву, пока она не сломается"},
        {"t2_1", "Зеленый листик можно двигать"},
        {"t2_2", "Зажми его и потяни"},
        {"t3_1", "Гусеница поедает растения на своем пути"},
        {"t3_2", "Постарайся с ней не сталкиваться"},
    };

    private Dictionary<string, string> en = new Dictionary<string, string>()
    {
        {"play", "Play"},
        {"settings", "Settings" },
        {"level", "Level {0}" },
        {"levels", "Levels"},
        {"help", "Help the FLOWER" },
        {"flower", ""},
        {"reach", "reach the SUN"},
        {"sun", ""},
        {"t1", "Tap the cracked soil until it breaks"},
        {"t2_1", "The green leaf can be moved"},
        {"t2_2", "Press and drag it"},
        {"t3_1", "The caterpillar eats plants in its path"},
        {"t3_2", "Try to avoid it"},
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (PlayerPrefs.HasKey("lang"))
        {
            currentLang = PlayerPrefs.GetString("lang");
        }
        else
        {
            DetectLanguage();
        }
    }


    void DetectLanguage()
    {
        if (Application.systemLanguage == SystemLanguage.Russian)
            currentLang = "ru";
        else
            currentLang = "en";
    }

    public string GetText(string key)
    {
        if (currentLang == "ru" && ru.ContainsKey(key))
            return ru[key];

        if (currentLang == "en" && en.ContainsKey(key))
            return en[key];

        return key;
    }

    public void SetLanguage(string lang)
    {
        currentLang = lang;
        PlayerPrefs.SetString("lang", currentLang);
        OnLanguageChanged?.Invoke();
    }

    public string GetText(string key, params object[] args)
    {
        string value = GetText(key);
        return string.Format(value, args);
    }

    

}
