using System.Collections;
using UnityEngine;
using SpaceMission.Core;

namespace SpaceMission.Simulation
{
    /// Launch + ascent phase (GDD 10.2 phases 1–2): ignition, flame, smoke, liftoff,
    /// atmospheric climb. Duration compressed to ~35 s. Fuel burn follows the
    /// engineering model; an underfueled config flame-outs partway and falls.
    public class LaunchSimulation : MonoBehaviour
    {
        public GameObject rocketRoot;
        public ParticleSystem flameParticles;
        public ParticleSystem smokeParticles;

        MissionSimulationDirector director;
        float ascentElapsed;
        float ascentDuration;
        bool launched;
        bool failureDone;

        public float AltitudeKm { get; private set; }
        public float VelocityMS { get; private set; }
        public float FuelFraction { get; private set; }
        public float ProgressFraction => ascentDuration > 0f ? Mathf.Clamp01(ascentElapsed / ascentDuration) : 0f;

        public void Begin(MissionSimulationDirector directorRef, float duration)
        {
            director = directorRef;
            ascentDuration = duration;
            director.SetPhase(SimPhase.LaunchPad);
            StartCoroutine(RunSequence());
        }

        IEnumerator RunSequence()
        {
            yield return new WaitForSeconds(2.5f);
            director.SetPhase(SimPhase.Ascent);
            Ignite();
            launched = true;

            var flameOut = director != null && director.failureAtAltitudeFraction < 0.999f;

            while (launched && ascentElapsed < ascentDuration)
            {
                var dt = Time.deltaTime;
                ascentElapsed += dt;
                var t = ProgressFraction;

                var eased = EaseAscent(t);
                AltitudeKm = eased * 400f;
                VelocityMS = Mathf.Lerp(0f, 7800f, eased);
                FuelFraction = Mathf.Clamp01(1f - t * (flameOut ? director.failureAtAltitudeFraction : 1f));

                if (flameOut && t >= director.failureAtAltitudeFraction && !failureDone)
                {
                    failureDone = true;
                    CutEngines();
                    yield return StartCoroutine(FallAndFail());
                    yield break;
                }
                yield return null;
            }

            CutEngines();
            director.OnAscentEnded(true);
        }

        static float EaseAscent(float t) => t * t * (3f - 2f * t) * 0.85f + t * 0.15f;

        void Ignite()
        {
            if (flameParticles != null) flameParticles.Play();
            if (smokeParticles != null) smokeParticles.Play();
        }

        void CutEngines()
        {
            if (flameParticles != null) flameParticles.Stop();
            if (smokeParticles != null) smokeParticles.Stop();
        }

        IEnumerator FallAndFail()
        {
            VelocityMS = 0f;
            var fallTime = 0f;
            var start = rocketRoot != null ? rocketRoot.transform.position : Vector3.zero;
            while (fallTime < 2.5f)
            {
                fallTime += Time.deltaTime;
                AltitudeKm = Mathf.Max(0f, AltitudeKm - fallTime * 9f);
                if (rocketRoot != null)
                {
                    rocketRoot.transform.position = start + Vector3.down * (fallTime * fallTime * 12f);
                    rocketRoot.transform.Rotate(Vector3.forward, 120f * Time.deltaTime);
                }
                yield return null;
            }
            director.OnAscentEnded(false);
        }
    }
}
