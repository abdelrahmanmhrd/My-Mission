using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using SpaceMission.Data;
using SpaceMission.Engineering;
using SpaceMission.State;

namespace SpaceMission.Core
{
    /// Bootstraps the global services and owns the screen flow.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<MissionData> MissionStarted;
        public event Action<ValidationReport> Validated;
        public event Action<MissionOutcome> MissionFinished;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            GameState.DB = GameDatabase.Load();
            GameState.Settings = new GameSettings();
            GameState.Settings.Load();
            GameState.Progress = new ProgressService();
            GameState.Progress.Load();
        }

        public void StartMission(MissionData mission)
        {
            GameState.CurrentMission = mission;
            GameState.Config = new MissionConfig { mission = mission };
            GameState.LastValidation = null;
            GameState.LastEngineering = null;
            GameState.LastOutcome = null;
            GameState.LastScore = null;
            MissionStarted?.Invoke(mission);
        }

        public void ValidateConfiguration()
        {
            if (GameState.CurrentMission == null || GameState.Config == null) return;
            GameState.LastValidation = ValidationEngine.Validate(GameState.CurrentMission, GameState.Config, GameState.DB);
            Validated?.Invoke(GameState.LastValidation);
        }

        public ValidationReport ValidateNow()
        {
            ValidateConfiguration();
            return GameState.LastValidation;
        }

        public void BeginLaunch()
        {
            StartCoroutine(SimFlow());
        }

        IEnumerator SimFlow()
        {
            // Load the additive launch scene and build its contents. If anything
            // goes wrong, fall back to the deterministic verdict without visuals.
            var op = SceneManager.LoadSceneAsync("SimLaunch", LoadSceneMode.Additive);
            if (op == null)
            {
                Debug.LogWarning("[GameManager] SimLaunch scene unavailable — producing verdict directly.");
                RunMission();
                yield break;
            }
            while (!op.isDone) yield return null;
            Simulation.SimSceneBootstrap.BuildScene();
        }

        /// Deterministic mission verdict + scoring + progression.
        public void RunMission()
        {
            if (GameState.CurrentMission == null || GameState.Config == null) return;
            var report = GameState.LastValidation ?? ValidateNow();
            var engineering = report.engineering;
            GameState.LastEngineering = engineering;

            var outcome = ScoringModel.EvaluateOutcome(GameState.CurrentMission, GameState.Config, engineering);
            var score = ScoringModel.Score(GameState.CurrentMission, GameState.Config, engineering, outcome);
            GameState.LastOutcome = outcome;
            GameState.LastScore = score;

            if (outcome.success)
            {
                GameState.Progress.SubmitScore(GameState.CurrentMission.id, Mathf.RoundToInt(score.total));
                UnlockNextMission(GameState.CurrentMission.id);
            }

            MissionFinished?.Invoke(outcome);
        }

        void UnlockNextMission(string missionId)
        {
            var missions = GameState.DB.missions;
            for (var i = 0; i < missions.Count; i++)
            {
                if (missions[i].id != missionId || i + 1 >= missions.Count) continue;
                GameState.Progress.UnlockMission(missions[i + 1].id);
                return;
            }
        }
    }
}
