using System;
using System.Collections.Generic;
using UnityEngine;
using YG;

public class LocalizationManager : MonoBehaviour
{
    private static LocalizationManager _instance;

    public static LocalizationManager Instance => _instance;

    private string currentLang;
    public Action OnLanguageChanged;

    private static bool isQuitting = false;

    private Dictionary<string, string> ru = new Dictionary<string, string>()
    {
        {"play", "Играть"},
        {"settings", "Настройки" },
        {"level", "Уровень {0}" },
        {"levels", "Уровни"},
        {"help", "Помоги <color=#EE5B71>ЦВЕТКУ</color> добраться до <color=#FFE177>СОЛНЦА</color>"},
        {"t1", "Нажимай на треснувшую почву, пока она не сломается"},
        {"t2_1", "Зелёный листик можно двигать"},
        {"t2_2", "Зажми его и потяни"},
        {"t3_1", "<color=#84EE54>ГУСЕНИЦА</color> поедает растения на своём пути"},
        {"t3_2", "Постарайся с ней не сталкиваться"},
        {"loose", "Уровень {0} \nне пройден"},
        {"win", "Уровень {0} \nпройден!"},
        {"thanks", "Спасибо \nза прохождение \nвсех уровней!"},
        {"build", "Мы уже \nстроим новые"},
    };

    private Dictionary<string, string> en = new Dictionary<string, string>()
    {
        {"play", "Play"},
        {"settings", "Settings" },
        {"level", "Level {0}" },
        {"levels", "Levels"},
        {"help", "Help the <color=#EE5B71>FLOWER</color> reach the <color=#FFE177>SUN</color>" },
        {"t1", "Tap the cracked soil until it breaks"},
        {"t2_1", "The green leaf can be moved"},
        {"t2_2", "Press and drag it"},
        {"t3_1", "The <color=#A4EC75>CATERPILLAR</color> eats plants in its path"},
        {"t3_2", "Try to avoid it"},
        {"loose", "Level {0} \nfailed"},
        {"win", "Level {0} \ncompleted!"},
        {"thanks", "Thank you \nfor completing \nall the levels!"},
        {"build", "We’re already \nbuilding new ones"},
    };

    void Awake()
    {
        if (isQuitting)
            return;

        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitLanguage();
    }
    void Start()
    {
        Debug.Log(YG2.isSDKEnabled);
    }


    void OnEnable()
    {
        if (_instance != null)
            YG2.onSwitchLang += SetLanguageFromYG;
    }

    void OnDisable()
    {
        if (_instance != null)
            YG2.onSwitchLang -= SetLanguageFromYG;
    }

    private void SetLanguageFromYG(string lang)
    {
        SetLanguage(lang);
    }

    private void InitLanguage()
    {
        string lang = "en";

        if (_instance != null && !string.IsNullOrEmpty(YG2.lang))
        {
            lang = YG2.lang;
        }
        else if (PlayerPrefs.HasKey("lang"))
        {
            lang = PlayerPrefs.GetString("lang");
        }
        else
        {
            lang = Application.systemLanguage == SystemLanguage.Russian ? "ru" : "en";
        }

        SetLanguage(lang, false);
    }

    public void SetLanguage(string lang, bool save = true)
    {
        currentLang = lang;

        if (save)
            PlayerPrefs.SetString("lang", currentLang);

        OnLanguageChanged?.Invoke();
    }

    public string GetText(string key)
    {
        if (currentLang == "ru" && ru.ContainsKey(key))
            return ru[key];

        if (currentLang == "en" && en.ContainsKey(key))
            return en[key];

        return key;
    }

    public string GetText(string key, params object[] args)
    {
        string value = GetText(key);
        return string.Format(value, args);
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
    }
}
