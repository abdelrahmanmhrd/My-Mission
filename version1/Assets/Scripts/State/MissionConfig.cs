using System;
using System.Collections.Generic;
using UnityEngine;
using SpaceMission.Data;

namespace SpaceMission.State
{
    /// One launch-vehicle stage as configured by the player.
    [Serializable]
    public class StageSelection
    {
        public string stageName;
        public List<string> engineIds = new List<string>();
        public string fuelId = "";
        public float fuelAmountFU;
    }

    /// The shared mission configuration state. Every selection page reads from and
    /// writes to this object; all screens stay in sync through Changed.
    public class MissionConfig
    {
        public MissionData mission;

        public string vehicleId = "";
        public string spacecraftId = "";
        public string launchpadId = "";
        public List<StageSelection> stages = new List<StageSelection>();
        public List<string> instrumentIds = new List<string>();

        public event Action Changed;

        public VehicleData Vehicle(GameDatabase db) => db.GetVehicle(vehicleId);
        public SpacecraftData Spacecraft(GameDatabase db) => db.GetSpacecraft(spacecraftId);
        public LaunchpadData Launchpad(GameDatabase db) => db.GetLaunchpad(launchpadId);

        public void NotifyChanged() => Changed?.Invoke();

        /// Rebuilds the stage selection list whenever the vehicle changes, preserving
        /// existing selections where stage shapes match.
        public void SetVehicle(string id, GameDatabase db)
        {
            if (vehicleId == id) return;
            vehicleId = id;
            stages.Clear();

            var vehicle = db.GetVehicle(id);
            if (vehicle != null)
                foreach (var stage in vehicle.stages)
                    stages.Add(new StageSelection { stageName = stage.stageName });

            NotifyChanged();
        }

        public void SetSpacecraft(string id) { if (spacecraftId == id) return; spacecraftId = id; NotifyChanged(); }
        public void SetLaunchpad(string id) { if (launchpadId == id) return; launchpadId = id; NotifyChanged(); }

        public void SetStageEngines(int stageIndex, List<string> engineIds)
        {
            if (stageIndex < 0 || stageIndex >= stages.Count) return;
            stages[stageIndex].engineIds = new List<string>(engineIds);
            NotifyChanged();
        }

        public void SetStageFuel(int stageIndex, string fuelId)
        {
            if (stageIndex < 0 || stageIndex >= stages.Count) return;
            stages[stageIndex].fuelId = fuelId;
            NotifyChanged();
        }

        public void SetStageFuelAmount(int stageIndex, float amountFU)
        {
            if (stageIndex < 0 || stageIndex >= stages.Count) return;
            stages[stageIndex].fuelAmountFU = Mathf.Max(0f, amountFU);
            NotifyChanged();
        }

        public void ToggleInstrument(string instrumentId)
        {
            if (instrumentIds.Contains(instrumentId))
                instrumentIds.Remove(instrumentId);
            else
                instrumentIds.Add(instrumentId);
            NotifyChanged();
        }

        public void Clear()
        {
            vehicleId = "";
            spacecraftId = "";
            launchpadId = "";
            stages.Clear();
            instrumentIds.Clear();
            NotifyChanged();
        }

        /// True when every category has at least the required selections made
        /// (before any feasibility validation).
        public bool HasAllRequiredSelections(GameDatabase db)
        {
            if (db == null || mission == null) return false;
            if (Vehicle(db) == null || Spacecraft(db) == null || Launchpad(db) == null) return false;

            foreach (var stage in stages)
            {
                if (stage.engineIds.Count == 0) return false;
                if (string.IsNullOrEmpty(stage.fuelId)) return false;
                if (stage.fuelAmountFU <= 0f) return false;
            }
            return true;
        }
    }
}
