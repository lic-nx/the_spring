using System.Collections.Generic;
using UnityEngine;

namespace YG
{
    [System.Serializable]
    public class ZoneSaveData
    {
        public string zoneId;
        public bool zoneEmpty;
        public string potSpriteName;
    }

    public partial class SavesYG
    {
        // Твои существующие поля
        public int UnlockedLevel = 1; // Открытый уровень
        public int CompletedLevel = 0; // Пройденный уровень
        public int MusicEnabled = 1; // Вкл/выкл музыки
        public int ReachedIndex = 0; // Достигнутый индекс
        public bool EffectMusicEnabled = true;
        public int Coins = 500; // игровая валюта 

        // Новое: сохранение статусов зон и названий спрайтов горшков
        public List<ZoneSaveData> occupiedZones = new List<ZoneSaveData>();
    }
}