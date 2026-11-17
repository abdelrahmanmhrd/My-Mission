using SpaceMission.Core;
using SpaceMission.Data;
using SpaceMission.Engineering;
using SpaceMission.State;

namespace SpaceMission.Core
{
    /// Central cross-scene state (single main scene + additive sim scenes).
    public static class GameState
    {
        public static GameDatabase DB;
        public static GameSettings Settings;
        public static ProgressService Progress;
        public static MissionData CurrentMission;
        public static MissionConfig Config;
        public static ValidationReport LastValidation;
        public static Engineering.EngineeringResult LastEngineering;
        public static Engineering.MissionOutcome LastOutcome;
        public static Engineering.ScoreBreakdown LastScore;
    }
}
