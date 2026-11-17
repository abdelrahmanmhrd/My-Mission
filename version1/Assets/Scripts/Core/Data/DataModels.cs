using System;
using System.Collections.Generic;

namespace SpaceMission.Data
{
    [Serializable]
    public class EngineData
    {
        public string id;
        public string name;
        public string description;
        public string fuelId;
        public float thrustSlKN;
        public float thrustVacKN;
        public float ispVacS;
        public float dryMassKg;
        public string stageRole;          // "booster" | "upper" | "spacecraft"
        public bool restartable;
        public float costUSD;
        public string[] compatibleVehicles;
        public string source;
    }

    [Serializable]
    public class FuelData
    {
        public string id;
        public string name;
        public string description;
        public float ispMultiplier;       // applied to engine ispVac
        public float tankMassFactor;      // tank structure mass per FU of fuel
        public float costPerFU;           // USD per tonne (gameplay approximation)
        public string source;
    }

    [Serializable]
    public class VehicleStage
    {
        public string stageName;
        public int boosterSlots;
        public int upperSlots;
        public int fixedSolidBoosters;
        public float fixedBoosterThrustKN;   // total thrust of the fixed solid boosters at liftoff
        public float fixedBoosterMassKg;     // total mass of the fixed solid boosters
        public float dryMassKg;              // stage structure mass (approximation: engines excluded, picked engines' mass folded in)
        public float maxFuelFU;              // max fuel capacity in tonnes
        public bool restartable;             // stage requires a restartable engine when true (upper stages that burn twice)
    }

    [Serializable]
    public class VehicleData
    {
        public string id;
        public string name;
        public string description;
        public bool crewRated;
        public float costUSD;
        public float payloadLEOT;
        public float payloadTLIT;
        public float payloadTMIT;
        public VehicleStage[] stages;
        public string source;
    }

    [Serializable]
    public class SpacecraftData
    {
        public string id;
        public string name;
        public string description;
        public int crewCapacity;
        public float massT;
        public bool crewed;
        public float propellantT;         // onboard propellant carried (informational/mass context)
        public string engineId;           // fixed main engine that comes with the spacecraft
        public float engineIspS;          // effective isp of the spacecraft main engine
        public float arrivalDvMS;         // real arrival-burn capability (precomputed from mission performance)
        public float returnDvMS;          // real return-burn capability (precomputed from mission performance)
        public float powerGenerationW;    // electrical power available for instruments
        public float commCapacityGbPerDay;// downlink capacity for instrument data
        public float costUSD;
        public string source;
    }

    [Serializable]
    public class LaunchpadData
    {
        public string id;
        public string name;
        public string description;
        public string[] compatibleVehicles;
        public bool crewInfrastructure;
        public float costUSD;
        public string source;
    }

    [Serializable]
    public class InstrumentData
    {
        public string id;
        public string name;
        public string description;
        public float massKg;
        public float powerW;
        public float dataGbPerDay;
        public float scienceValue;
        public float costUSD;
        public string[] missionTags;
        public string role;               // Mast | Analyzer | Weather | Radar | Oxygen | Surface | Sample | EVA
        public string effect;             // none | cameraControl | crewOxygen
        public string source;
    }

    [Serializable]
    public class InstrumentRequirement
    {
        public string type;               // "required" (ids must all be present) | "anyOfRole" (count instruments with the role)
        public string[] ids;
        public string role;
        public int count;
        public string severity;           // "block" | "critical" | "soft"
        public string consequence;
    }

    [Serializable]
    public class DeltaVPhase
    {
        public string phase;              // "departure" | "arrival" | "return"
        public float dvMS;                // required delta-v in m/s
        public string scope;              // "vehicle" | "spacecraft"
    }

    [Serializable]
    public class MissionData
    {
        public string id;
        public string name;
        public string briefTitle;
        public string briefBody;
        public string missionType;        // "crewed" | "automated"
        public int crew;
        public string requiredSpacecraftId;
        public string destinationPlanetId;
        public float distanceKm;
        public bool crewedPadRequired;
        public bool crewRatedVehicleRequired;
        public string capacityMetric;     // "TLI" | "TMI" — payload capacity the configuration must satisfy
        public InstrumentRequirement[] instrumentRequirements;
        public float minScienceValue;
        public float budgetUSD;
        public DeltaVPhase[] deltaV;
        public bool isTutorial;
        public string successCondition;
    }

    [Serializable]
    public class PlanetData
    {
        public string id;
        public string name;
        public float massKg;
        public float radiusKm;
        public float surfaceGravityMS2;
        public string atmosphere;
        public float distanceFromEarthKm;
        public string source;
    }
}
