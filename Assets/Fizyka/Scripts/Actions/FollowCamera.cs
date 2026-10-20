using UnityEngine;

namespace Fizyka
{
    /// <summary>
    /// Kamera trzecioosobowa: podąża za celem; PPM + mysz – obrót wokół postaci, kółko – odległość.
    /// </summary>
    public class FollowCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 7f;
        public float minDistance = 2f;
        public float maxDistance = 20f;
        public float height = 1.2f;
        public float lookSensitivity = 0.15f;
        public float yaw = 0f;
        public float pitch = 20f;

        void LateUpdate()
        {
            if (target == null)
                return;

            if (DemoInput.IsPressed(DemoInput.RightClick))
            {
                Vector2 look = DemoInput.ReadVector2(DemoInput.Look);
                yaw += look.x * lookSensitivity;
                pitch = Mathf.Clamp(pitch - look.y * lookSensitivity, -10f, 80f);
            }

            Vector2 scroll = DemoInput.ReadVector2(DemoInput.ScrollWheel);
            if (Mathf.Abs(scroll.y) > 0.01f)
                distance = Mathf.Clamp(distance - Mathf.Sign(scroll.y) * 1f, minDistance, maxDistance);

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + Vector3.up * height;
            transform.SetPositionAndRotation(pivot - rot * Vector3.forward * distance, rot);
        }
    }
}
