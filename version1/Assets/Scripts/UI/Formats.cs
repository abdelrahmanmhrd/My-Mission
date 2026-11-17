using SpaceMission.Core;
using UnityEngine;

namespace SpaceMission.UI
{
    /// Display formatting — REAL engineering units everywhere (user decision 2026-11-15).
    /// Money is always USD. Thrust kN, mass kg/t, fuel tonnes, distance km.
    public static class Formats
    {
        public static string Thrust(float kn) => $"{kn:0} kN";

        public static string Mass(float kg) =>
            kg >= 1000f ? $"{kg / 1000f:0.0#} t" : $"{kg:0} kg";

        public static string Fuel(float tonnes) => $"{tonnes:0.#} t";

        public static string Money(float usd) => $"${usd / 1000000f:0.0}M";

        public static string Distance(float km) =>
            km >= 1e6f ? $"{km / 1e6f:0.0} million km" : $"{km:0} km";

        public static string DeltaV(float mS) => $"{mS:0} m/s";

        public static string Payload(float kg) =>
            kg >= 1000f ? $"{kg / 1000f:0.00} t" : $"{kg:0} kg";

        public static string Power(float watts) => watts >= 1000f ? $"{watts / 1000f:0.0} kW" : $"{watts:0} W";

        public static string Data(float gbPerDay) => $"{gbPerDay:0.0} Gb/day";
    }
}
