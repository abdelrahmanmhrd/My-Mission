using UnityEngine;
using UnityEngine.InputSystem;
using SpaceMission.Data;

namespace SpaceMission.Simulation
{
    /// Sim camera (GDD 10.4): player can orbit/zoom while the rocket flies.
    /// Without the Zoom Camera Mast (cameraControl instrument) the camera is
    /// locked to a fixed position — the mission-specific consequence rule.
    public class SimCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 25f;
        public float minDistance = 8f;
        public float maxDistance = 160f;
        public float rotateSpeed = 2.4f;
        public float zoomSpeed = 12f;

        float yaw;
        float pitch = 12f;
        bool cameraLocked;

        public void Setup(Transform followTarget)
        {
            target = followTarget;
            var cfg = SpaceMission.Core.GameState.Config;
            var db = SpaceMission.Core.GameState.DB;
            cameraLocked = true;
            if (cfg != null && db != null)
                foreach (var id in cfg.instrumentIds)
                {
                    var inst = db.GetInstrument(id);
                    if (inst != null && inst.effect == "cameraControl") cameraLocked = false;
                }
            var angles = transform.eulerAngles;
            yaw = angles.y;
        }

        void LateUpdate()
        {
            if (target == null) return;

            if (!cameraLocked)
            {
                if (Mouse.current != null && Mouse.current.rightButton.isPressed)
                {
                    var delta = Mouse.current.delta.ReadValue() * 0.1f;
                    yaw += delta.x * rotateSpeed;
                    pitch = Mathf.Clamp(pitch - delta.y * rotateSpeed, -60f, 80f);
                }
                if (Mouse.current != null)
                {
                    var scroll = Mouse.current.scroll.ReadValue().y;
                    if (Mathf.Abs(scroll) > 0.01f)
                        distance = Mathf.Clamp(distance - scroll * zoomSpeed * 0.05f, minDistance, maxDistance);
                }
            }

            var rot = Quaternion.Euler(pitch, yaw, 0f);
            var pos = target.position + rot * new Vector3(0f, 0f, -distance);
            transform.position = Vector3.Lerp(transform.position, pos, cameraLocked ? 0.06f : 0.25f);
            transform.LookAt(target.position);
        }

        public void ResetFollow()
        {
            yaw = 0f;
            pitch = 12f;
            distance = 25f;
        }
    }
}
