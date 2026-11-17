using System;
using UnityEngine;

namespace SpaceMission.Core
{
    public enum UnitsMode { GameUnits, RealUnits }

    /// Player-facing settings, persisted in PlayerPrefs.
    public class GameSettings
    {
        const string KeyUnits = "smd.units";
        const string KeyMusic = "smd.music";
        const string KeySfx = "smd.sfx";

        public UnitsMode unitsMode = UnitsMode.GameUnits;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;

        public event Action Changed;

        public void Load()
        {
            unitsMode = (UnitsMode)PlayerPrefs.GetInt(KeyUnits, (int)UnitsMode.GameUnits);
            musicVolume = PlayerPrefs.GetFloat(KeyMusic, 0.8f);
            sfxVolume = PlayerPrefs.GetFloat(KeySfx, 1f);
        }

        public void SetUnitsMode(UnitsMode mode)
        {
            unitsMode = mode;
            PlayerPrefs.SetInt(KeyUnits, (int)mode);
            NotifyChanged();
        }

        public void SetMusicVolume(float v)
        {
            musicVolume = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat(KeyMusic, musicVolume);
            NotifyChanged();
        }

        public void SetSfxVolume(float v)
        {
            sfxVolume = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat(KeySfx, sfxVolume);
            NotifyChanged();
        }

        void NotifyChanged() => Changed?.Invoke();
    }
}
