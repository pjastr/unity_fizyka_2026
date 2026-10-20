using UnityEngine;

namespace Fizyka
{
    /// <summary>
    /// Swobodna kamera sterowana akcjami z domyślnego Input Action:
    /// PPM (UI/RightClick) + mysz (Player/Look) – obrót, WASD (Player/Move) – ruch,
    /// Shift (Player/Sprint) – szybciej, kółko (UI/ScrollWheel) – przód/tył.
    /// </summary>
    public class FreeCamera : MonoBehaviour
    {
        public float moveSpeed = 8f;
        public float sprintMultiplier = 3f;
        public float lookSensitivity = 0.15f;
        public float scrollStep = 1.5f;

        float m_Yaw;
        float m_Pitch;

        void Start()
        {
            var e = transform.eulerAngles;
            m_Yaw = e.y;
            m_Pitch = e.x > 180f ? e.x - 360f : e.x;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;

            if (DemoInput.IsPressed(DemoInput.RightClick))
            {
                Vector2 look = DemoInput.ReadVector2(DemoInput.Look);
                m_Yaw += look.x * lookSensitivity;
                m_Pitch -= look.y * lookSensitivity;
                m_Pitch = Mathf.Clamp(m_Pitch, -89f, 89f);
                transform.rotation = Quaternion.Euler(m_Pitch, m_Yaw, 0f);
            }

            Vector2 move = DemoInput.ReadVector2(DemoInput.Move);
            float speed = moveSpeed * (DemoInput.IsPressed(DemoInput.Sprint) ? sprintMultiplier : 1f);
            Vector3 fwd = transform.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 1e-4f) fwd = transform.up;
            fwd.Normalize();
            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();
            transform.position += (fwd * move.y + right * move.x) * speed * dt;

            Vector2 scroll = DemoInput.ReadVector2(DemoInput.ScrollWheel);
            if (Mathf.Abs(scroll.y) > 0.01f)
                transform.position += transform.forward * Mathf.Sign(scroll.y) * scrollStep *
                                      (DemoInput.IsPressed(DemoInput.Sprint) ? sprintMultiplier : 1f);
        }
    }
}
