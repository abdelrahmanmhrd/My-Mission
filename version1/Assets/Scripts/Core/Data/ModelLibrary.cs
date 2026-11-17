using UnityEngine;

namespace SpaceMission.Data
{
    /// Maps game component IDs to their 3D model assets (under Assets/Resources/Models).
    public static class ModelLibrary
    {
        const string Root = "Models/";

        public static string EngineModelPath(string engineId)
        {
            switch (engineId)
            {
                case "ENG-F1": return Root + "Engines/F1/F1Engine";
                case "ENG-J2": return Root + "Engines/J-2/TD_J-2";
                case "ENG-MERLIN": return Root + "Engines/Merlin1D/Merlin1D";
                case "ENG-MERVAC": return Root + "Engines/Merlin1D_Vacuum/Merlin1D_Vacuum";
                case "ENG-RD180": return Root + "Engines/RD-180/RD-180";
                case "ENG-RS25": return Root + "Engines/RS-25/RS-25";
                case "ENG-AJ10": return Root + "Engines/AJ10/AJ10";
                default: return null;
            }
        }

        public static string VehicleModelPath(string vehicleId)
        {
            switch (vehicleId)
            {
                case "LV-F9": return Root + "Vehicles/Falcon9/Falcon9";
                case "LV-ATLAS541": return Root + "Vehicles/AtlasV/AtlasV";
                case "LV-SATV": return Root + "Vehicles/SaturnV/SaturnV";
                case "LV-SLS": return Root + "Vehicles/SLS/SLS";
                default: return null;
            }
        }

        public static string SpacecraftModelPath(string spacecraftId)
        {
            switch (spacecraftId)
            {
                case "SC-CSMLM": return Root + "Spacecraft/ApolloCSMLM/ApolloSoyuz";
                case "SC-ORION": return Root + "Spacecraft/Orion/OrionCapsule";
                case "SC-ORIONLANDER": return Root + "Spacecraft/Orion/OrionCapsule";
                case "SC-PERSAV": return Root + "Spacecraft/Perseverance/Perseverance";
                case "SC-ORBGEN": return Root + "Spacecraft/Maven/Maven";
                default: return null;
            }
        }

        public static string LaunchpadModelPath(string launchpadId) => Root + "Launchpad/Gantry";
        public static string PlanetModelPath(string planetId)
        {
            switch (planetId)
            {
                case "PLNT-EARTH": return Root + "Planets/Earth";
                case "PLNT-MOON": return Root + "Planets/Moon";
                default: return null;
            }
        }

        public static GameObject Load(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath)) return null;
            return Resources.Load<GameObject>(resourcePath);
        }
    }
}
