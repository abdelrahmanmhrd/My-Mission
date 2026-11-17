using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceMission.Data
{
    /// Loads all JSON datasets from Assets/Resources/Data and exposes lookup dictionaries.
    public class GameDatabase
    {
        public List<EngineData> engines = new List<EngineData>();
        public List<FuelData> fuels = new List<FuelData>();
        public List<VehicleData> vehicles = new List<VehicleData>();
        public List<SpacecraftData> spacecraft = new List<SpacecraftData>();
        public List<LaunchpadData> launchpads = new List<LaunchpadData>();
        public List<InstrumentData> instruments = new List<InstrumentData>();
        public List<MissionData> missions = new List<MissionData>();
        public List<PlanetData> planets = new List<PlanetData>();

        Dictionary<string, EngineData> engineMap;
        Dictionary<string, FuelData> fuelMap;
        Dictionary<string, VehicleData> vehicleMap;
        Dictionary<string, SpacecraftData> spacecraftMap;
        Dictionary<string, LaunchpadData> launchpadMap;
        Dictionary<string, InstrumentData> instrumentMap;
        Dictionary<string, MissionData> missionMap;
        Dictionary<string, PlanetData> planetMap;

        public static GameDatabase Load()
        {
            var db = new GameDatabase();
            db.engines = LoadList<EngineData>("engines");
            db.fuels = LoadList<FuelData>("fuels");
            db.vehicles = LoadList<VehicleData>("vehicles");
            db.spacecraft = LoadList<SpacecraftData>("spacecraft");
            db.launchpads = LoadList<LaunchpadData>("launchpads");
            db.instruments = LoadList<InstrumentData>("instruments");
            db.missions = LoadList<MissionData>("missions");
            db.planets = LoadList<PlanetData>("planets");
            db.BuildMaps();
            return db;
        }

        static List<T> LoadList<T>(string fileName)
        {
            var result = new List<T>();
            var asset = Resources.Load<TextAsset>($"Data/{fileName}");
            if (asset == null)
            {
                Debug.LogError($"[GameDatabase] Missing data file: Data/{fileName}.json");
                return result;
            }
            var wrapper = JsonUtility.FromJson<Wrapper<T>>(asset.text);
            if (wrapper != null && wrapper.items != null)
                result.AddRange(wrapper.items);
            return result;
        }

        [Serializable]
        class Wrapper<T>
        {
            public List<T> items;
        }

        void BuildMaps()
        {
            engineMap = ToMap(engines, e => e.id);
            fuelMap = ToMap(fuels, f => f.id);
            vehicleMap = ToMap(vehicles, v => v.id);
            spacecraftMap = ToMap(spacecraft, s => s.id);
            launchpadMap = ToMap(launchpads, p => p.id);
            instrumentMap = ToMap(instruments, i => i.id);
            missionMap = ToMap(missions, m => m.id);
            planetMap = ToMap(planets, p => p.id);
        }

        static Dictionary<string, T> ToMap<T>(List<T> list, Func<T, string> key)
        {
            var map = new Dictionary<string, T>();
            if (list == null) return map;
            foreach (var item in list)
                map[key(item)] = item;
            return map;
        }

        public EngineData GetEngine(string id) => id != null && engineMap.TryGetValue(id, out var v) ? v : null;
        public FuelData GetFuel(string id) => id != null && fuelMap.TryGetValue(id, out var v) ? v : null;
        public VehicleData GetVehicle(string id) => id != null && vehicleMap.TryGetValue(id, out var v) ? v : null;
        public SpacecraftData GetSpacecraft(string id) => id != null && spacecraftMap.TryGetValue(id, out var v) ? v : null;
        public LaunchpadData GetLaunchpad(string id) => id != null && launchpadMap.TryGetValue(id, out var v) ? v : null;
        public InstrumentData GetInstrument(string id) => id != null && instrumentMap.TryGetValue(id, out var v) ? v : null;
        public MissionData GetMission(string id) => id != null && missionMap.TryGetValue(id, out var v) ? v : null;
        public PlanetData GetPlanet(string id) => id != null && planetMap.TryGetValue(id, out var v) ? v : null;
    }
}
