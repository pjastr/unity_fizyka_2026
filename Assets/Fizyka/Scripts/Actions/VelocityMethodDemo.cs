using UnityEngine;

namespace Fizyka
{
    /// <summary>
    /// Porównanie sposobów nadawania ruchu ciału:
    ///  • AddForce(..., ForceMode.VelocityChange) – DODAJE zmianę prędkości (kolejne naciśnięcia sumują się),
    ///  • rb.linearVelocity = v – USTAWIA prędkość (nadpisuje wszystko, także spadanie),
    ///  • rb.MovePosition – ruch ciała kinematycznego (nie zatrzymują go ściany, ale pcha inne ciała),
    ///  • transform.position += ... – "teleportacja" z pominięciem fizyki (nie róbcie tak z Rigidbody!).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class VelocityMethodDemo : DemoActionBase
    {
        public enum Method { AddForceVelocityChange, SetLinearVelocity, MovePositionKinematic, TransformPosition }

        public Method method = Method.AddForceVelocityChange;
        public Vector3 velocity = new Vector3(0, 0, 4);
        [Tooltip("Czas ruchu dla metod MovePosition i transform.position")]
        public float moveDuration = 3f;

        Rigidbody m_Rb;
        float m_MoveLeft;

        void Awake()
        {
            m_Rb = GetComponent<Rigidbody>();
            if (method == Method.MovePositionKinematic)
                m_Rb.isKinematic = true;
        }

        public override void OnMainAction()
        {
            m_Rb.WakeUp();
            switch (method)
            {
                case Method.AddForceVelocityChange:
                    m_Rb.AddForce(velocity, ForceMode.VelocityChange);
                    DemoHud.Log($"A: AddForce({velocity.z:0} m/s, VelocityChange) → prędkość się SUMUJE");
                    break;
                case Method.SetLinearVelocity:
                    m_Rb.linearVelocity = velocity;
                    DemoHud.Log($"B: linearVelocity = {velocity.z:0} m/s → prędkość jest NADPISANA");
                    break;
                case Method.MovePositionKinematic:
                case Method.TransformPosition:
                    m_MoveLeft = moveDuration;
                    break;
            }
        }

        void FixedUpdate()
        {
            if (method == Method.MovePositionKinematic && m_MoveLeft > 0f)
            {
                m_Rb.MovePosition(m_Rb.position + velocity * Time.fixedDeltaTime);
                m_MoveLeft -= Time.fixedDeltaTime;
            }
        }

        void Update()
        {
            if (method == Method.TransformPosition && m_MoveLeft > 0f)
            {
                // ŹLE (celowo): omijamy silnik fizyki – obiekt "teleportuje się" co klatkę.
                transform.position += velocity * Time.deltaTime;
                m_MoveLeft -= Time.deltaTime;
            }
        }
    }
}
