using UnityEngine;

namespace Fizyka
{
    /// <summary>
    /// Porusza kinematycznym Rigidbody (isKinematic = true) za pomocą MovePosition / MoveRotation
    /// w FixedUpdate. Kinematyczne ciało ignoruje siły i kolizje, ale samo PCHA ciała dynamiczne.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class KinematicMover : MonoBehaviour
    {
        public enum Motion { PingPong, Rotate }

        public Motion motion = Motion.PingPong;
        [Tooltip("PingPong: przesunięcie od pozycji startowej (świat)")]
        public Vector3 offset = new Vector3(6, 0, 0);
        [Tooltip("PingPong: czas jednego przejścia w sekundach")]
        public float period = 3f;
        [Tooltip("Rotate: prędkość obrotu w stopniach na sekundę (oś Y)")]
        public float degreesPerSecond = 60f;

        Rigidbody m_Rb;
        Vector3 m_Start;
        Quaternion m_StartRot;
        float m_T;

        void Awake()
        {
            m_Rb = GetComponent<Rigidbody>();
            m_Rb.isKinematic = true;
            m_Start = m_Rb.position;
            m_StartRot = m_Rb.rotation;
        }

        void FixedUpdate()
        {
            m_T += Time.fixedDeltaTime;
            if (motion == Motion.PingPong)
            {
                float k = Mathf.PingPong(m_T / Mathf.Max(0.01f, period), 1f);
                k = Mathf.SmoothStep(0f, 1f, k);
                m_Rb.MovePosition(m_Start + offset * k);
            }
            else
            {
                m_Rb.MoveRotation(m_StartRot * Quaternion.Euler(0f, degreesPerSecond * m_T, 0f));
            }
        }
    }
}
