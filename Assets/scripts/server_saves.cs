using System.Collections.Generic;
using UnityEngine;

namespace YG
{
    [System.Serializable]
    public class ZoneSaveData
    {
        public string zoneId;
        public string potSpriteName;
    }

    [System.Serializable]
    public class PotSaveData
    {
        public string potId;
        public string spriteName;
        public string zoneId;
        public Vector3 position;
        public bool isOccupied;
        public string flowerId;
    }

    [System.Serializable]
    public class FlowerSaveData
    {
        public string flowerId;
        public string prefabName;
        public string spriteName;
        public Vector3 position;
        public int currentStageIndex;
        public float timeSinceLastWatering;
        public float timeSinceLastSunGeneration;
        public bool needWater;
        public bool needFertilize;
        public bool isFullyGrown;
        public int careEventCount;
        public string growthConditionsName;
        public bool hasGivenSun;
    }

    [System.Serializable]
    public class GardenSaveData
    {
        public List<PotSaveData> pots = new List<PotSaveData>();
        public List<FlowerSaveData> flowers = new List<FlowerSaveData>();
        public int currency;
        public System.DateTime lastSaveTime;
    }

    public partial class SavesYG
    {
        public int UnlockedLevel = 1;
        public int CompletedLevel = 0;
        public int MusicEnabled = 1;
        public int ReachedIndex = 0;
        public bool EffectMusicEnabled = true;
        public int Coins = 500;

        public List<ZoneSaveData> occupiedZones = new List<ZoneSaveData>();
        public GardenSaveData gardenData;
    }
}