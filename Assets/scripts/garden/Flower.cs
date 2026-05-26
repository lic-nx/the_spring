// ------------------------------------------------------------
// Flower.cs – класс, описывающий растущий цветок в садовом режиме игры
// ------------------------------------------------------------
// Этот скрипт представляет класс цветка, управляя его ростом, таймерами ухода
// и взаимодействием с условиями роста (GrowthConditions). Внутри кода присутствуют
// комментарии на русском языке, поясняющие каждую часть реализации.
// ------------------------------------------------------------
using System.Collections; // Базовые коллекции C# (не используются напрямую, но оставлены для совместимости)
using System.Collections.Generic; // Обобщённые коллекции (может потребоваться в будущих расширениях)
using UnityEngine; // Основные типы UnityEngine, необходимые для MonoBehaviour
using UnityEngine.SceneManagement; // Позволяет управлять сценами (в данном скрипте пока не используется, но подключено для возможностей расширения)
using UnityEngine.Rendering.PostProcessing; // Пакет пост‑обработки, может потребоваться для визуальных эффектов цветка

public class Flower : MonoBehaviour
{
    // Сериализуемые поля, видимые в инспекторе Unity, позволяют задавать параметры в редакторе
    [SerializeField] private GrowthConditions _conditions; // Ссылка на объект условий роста, назначается через инспектор
    // Спрайты, представляющие каждую стадию роста (0 – семя, 1 – всход, 2 – молодой побег, 3 – цветение)
    [SerializeField] private Sprite[] stageSprites; // Массив спрайтов для разных стадий роста
    // Опциональное поле для отладки текущей стадии (может быть раскомментировано при необходимости)
    // [SerializeField] private SeedStage _seedStage;

    public IGrowthStage CurrentStage { get; private set; } // Текущая стадия роста, доступна только для чтения извне
    public GrowthConditions Conditions => _conditions; // Публичный геттер для условий роста

    // Счётчики времени, измеряющие, как долго цветок не получал ухода
    private float _timeSinceLastWatering; // Время с последнего полива
    private float _timeSinceLastFertilizing; // Время с последнего удобрения
    private float _timeSinceLastSunlight; // Время с последнего доступа к свету

    /// <summary>
    /// Инициализирует цветок заданными условиями роста и устанавливает начальную стадию – семя.
    /// </summary>
    public void Initialize(GrowthConditions conditions)
    {
        _conditions = conditions; // Сохраняем переданные условия роста
        CurrentStage = new SeedStage(this); // Устанавливаем первую стадию – семя
        // Сбрасываем таймеры ухода, т.к. цветок только появился
        _timeSinceLastWatering = 0f;
        _timeSinceLastFertilizing = 0f;
        _timeSinceLastSunlight = 0f;
    }

    private void Awake()
    {
        // Гарантируем наличие ссылки на объект условий роста; если её нет, пытаемся загрузить ресурс по умолчанию
        if (_conditions == null)
        {
            _conditions = Resources.Load<GrowthConditions>("DefaultGrowthConditions");
            if (_conditions == null)
            {
                // Если ресурс не найден, создаём объект условий роста программно и полагаемся на его валидацию
                _conditions = ScriptableObject.CreateInstance<GrowthConditions>();
                // Предполагается, что метод OnValidate внутри GrowthConditions уже проверил корректность значений
            }
        }
        // Если стадия роста ещё не установлена (например, при загрузке сцены), создаём стартовую стадию
        if (CurrentStage == null)
        {
            CurrentStage = new SeedStage(this);
        }
    }

    private void Update()
    {
        // Обновляем таймеры ухода каждую каденцию
        _timeSinceLastWatering += Time.deltaTime;
        _timeSinceLastFertilizing += Time.deltaTime;
        _timeSinceLastSunlight += Time.deltaTime;

        // Проверяем, не превысили ли таймеры допустимые интервалы, и вызываем соответствующую логику
        CheckGrowthConditions();
        // Делегируем обновление текущей стадии её собственному методу
        CurrentStage.Update();
    }

    // Методы, вызываемые из внешних систем (например, UI) для сброса соответствующих таймеров
    public void Water() => _timeSinceLastWatering = 0f; // Сбрасываем таймер полива
    public void Fertilize() => _timeSinceLastFertilizing = 0f; // Сбрасываем таймер удобрения
    public void ProvideSunlight() => _timeSinceLastSunlight = 0f; // Сбрасываем таймер освещения

    private void CheckGrowthConditions()
    {
        // Если цветок находится на стадии семени и какой‑либо таймер превысил допустимый порог, выводим сообщение в консоль
        if (CurrentStage is SeedStage && _timeSinceLastWatering >= Conditions.TimeBetweenWatering)
        {
            Debug.Log("Seed needs watering"); // Требуется полив
        }
        if (CurrentStage is SeedStage && _timeSinceLastFertilizing >= Conditions.TimeBetweenFertilizing)
        {
            Debug.Log("Seed needs fertilizing"); // Требуется удобрение
        }
        if (CurrentStage is SeedStage && _timeSinceLastSunlight >= Conditions.TimeBetweenSunlight)
        {
            Debug.Log("Seed needs sunlight"); // Требуется свет
        }
    }

    // Переводит цветок к следующей стадии роста, используя метод NextStage текущей стадии
    public void AdvanceToNextStage() => CurrentStage = CurrentStage.NextStage();
}