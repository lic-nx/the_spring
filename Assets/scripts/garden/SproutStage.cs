using System.Collections; // Подключаем пространство имён для обычных коллекций
using System.Collections.Generic; // Подключаем пространство имён для обобщённых коллекций
using UnityEngine; // Главное пространство имён Unity

// Growth stage representing a sprouted flower.
// Стадия роста, представляющая проросший цветок.
public class SproutStage : IGrowthStage // Класс SproutStage реализует интерфейс IGrowthStage
{
    private Flower _flower; // Ссылка на объект Flower, к которому относится данная стадия
    private float _timeInStage; // Таймер, измеряющий, сколько времени прошло в текущей стадии

    public SproutStage(Flower flower)
    {
        _flower = flower; // Сохраняем ссылку на цветок
        _timeInStage = 0f; // Инициализируем таймер нулём
    }

    // Update called each frame; transition to next stage when time elapsed.
    // Вызывается каждый кадр; переход к следующей стадии после истечения времени.
    public void Update()
    {
        _timeInStage += Time.deltaTime; // Прибавляем прошедшее время к таймеру
        if (_timeInStage >= _flower.Conditions.TimeToYoungShoot)
        {
            _flower.AdvanceToNextStage(); // Переключаемся на следующую стадию роста
        }
    }

    // Return next stage (placeholder – stays in same stage for now).
    // Возвращает следующую стадию (заглушка – пока остаётся в той же стадии).
    public IGrowthStage NextStage()
    {
        // TODO: implement further stages like YoungShootStage.
        // TODO: добавить реализацию дальнейших стадий, например YoungShootStage.
        return this; // Пока возвращаем текущий объект как следующую стадию
    }
}