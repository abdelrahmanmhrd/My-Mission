using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceMission.Core
{
    /// Mission progression (sequential unlock) and local best scores, persisted in PlayerPrefs.
    public class ProgressService
    {
        const string KeyUnlocked = "smd.unlocked";
        const string KeyScores = "smd.scores";

        public List<string> unlockedMissionIds = new List<string>();
        public Dictionary<string, int> bestScores = new Dictionary<string, int>();

        public event Action Changed;

        public void Load()
        {
            unlockedMissionIds.Clear();
            bestScores.Clear();

            var unlockedJson = PlayerPrefs.GetString(KeyUnlocked, "");
            if (!string.IsNullOrEmpty(unlockedJson))
            {
                var unlocked = JsonUtility.FromJson<StringList>(unlockedJson);
                if (unlocked != null && unlocked.items != null)
                    unlockedMissionIds.AddRange(unlocked.items);
            }

            var scoresJson = PlayerPrefs.GetString(KeyScores, "");
            if (!string.IsNullOrEmpty(scoresJson))
            {
                var scores = JsonUtility.FromJson<ScoreEntryList>(scoresJson);
                if (scores != null && scores.items != null)
                    foreach (var entry in scores.items)
                        bestScores[entry.missionId] = entry.score;
            }

            // The first mission is always unlocked.
            if (unlockedMissionIds.Count == 0 && !unlockedMissionIds.Contains("M0-APOLLO"))
                unlockedMissionIds.Add("M0-APOLLO");
        }

        public bool IsUnlocked(string missionId) => unlockedMissionIds.Contains(missionId);

        public void UnlockMission(string missionId)
        {
            if (IsUnlocked(missionId)) return;
            unlockedMissionIds.Add(missionId);
            Save();
            Changed?.Invoke();
        }

        public int GetBestScore(string missionId) =>
            bestScores.TryGetValue(missionId, out var s) ? s : 0;

        public void SubmitScore(string missionId, int score)
        {
            if (score <= GetBestScore(missionId)) return;
            bestScores[missionId] = score;
            Save();
            Changed?.Invoke();
        }

        void Save()
        {
            var unlockedWrapper = new StringList { items = new List<string>(unlockedMissionIds) };
            PlayerPrefs.SetString(KeyUnlocked, JsonUtility.ToJson(unlockedWrapper));

            var scoreWrapper = new ScoreEntryList { items = new List<ScoreEntry>() };
            foreach (var kv in bestScores)
                scoreWrapper.items.Add(new ScoreEntry { missionId = kv.Key, score = kv.Value });
            PlayerPrefs.SetString(KeyScores, JsonUtility.ToJson(scoreWrapper));
            PlayerPrefs.Save();
        }

        [Serializable] class StringList { public List<string> items; }
        [Serializable] class ScoreEntry { public string missionId; public int score; }
        [Serializable] class ScoreEntryList { public List<ScoreEntry> items; }
    }
}
