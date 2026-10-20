using UnityEngine;

namespace Fizyka
{
    /// <summary>
    /// Po naciśnięciu Spacji przykłada do Rigidbody siłę (AddForce) albo moment siły (AddTorque)
    /// w wybranym trybie ForceMode.
    ///  • ForceMode.Force / Acceleration – działają "w czasie": przykładamy je co krok fizyki
    ///    (FixedUpdate) przez <see cref="duration"/> sekund.
    ///  • ForceMode.Impulse / VelocityChange – działają natychmiast: przykładamy je raz.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ApplyOnAction : DemoActionBase
    {
        public enum Kind { Force, Torque }

        public Kind kind = Kind.Force;
        public ForceMode mode = ForceMode.Impulse;
        [Tooltip("Wektor siły [N], przyspieszenia [m/s²], impulsu [N·s] lub zmiany prędkości [m/s] – zależnie od trybu (dla momentu: N·m, rad/s² ...)")]
        public Vector3 vector = new Vector3(0, 0, 10);
        [Tooltip("Czas działania dla trybów ciągłych (Force/Acceleration). 0 = tylko jeden krok fizyki.")]
        public float duration = 1f;
        [Tooltip("Wektor w układzie lokalnym obiektu (true) czy świata (false)")]
        public bool localSpace = false;
        [Tooltip("Zastosuj automatycznie zaraz po starcie sceny")]
        public bool applyOnStart = false;
        [Tooltip("Reaguj na Spację (akcja Player/Jump)")]
        public bool triggerOnMainAction = true;
        public bool logToHud = false;

        Rigidbody m_Rb;
        float m_TimeLeft;
        bool m_Running;

        void Awake() => m_Rb = GetComponent<Rigidbody>();

        void Start()
        {
            if (applyOnStart)
                Trigger();
        }

        bool IsContinuous => mode == ForceMode.Force || mode == ForceMode.Acceleration;

        Vector3 WorldVector => localSpace ? transform.TransformDirection(vector) : vector;

        public override void OnMainAction()
        {
            if (triggerOnMainAction)
                Trigger();
        }

        public void Trigger()
        {
            if (m_Rb == null || m_Rb.isKinematic)
                return;
            m_Rb.WakeUp();

            if (IsContinuous)
            {
                m_TimeLeft = Mathf.Max(duration, Time.fixedDeltaTime);
                m_Running = true;
                if (logToHud)
                    DemoHud.Log($"{name}: {(kind == Kind.Force ? "AddForce" : "AddTorque")}({vector}, ForceMode.{mode}) przez {duration:0.##} s");
            }
            else
            {
                Apply();
                if (logToHud)
                    DemoHud.Log($"{name}: {(kind == Kind.Force ? "AddForce" : "AddTorque")}({vector}, ForceMode.{mode}) – jednorazowo");
            }
        }

        void FixedUpdate()
        {
            if (!m_Running)
                return;
            Apply();
            m_TimeLeft -= Time.fixedDeltaTime;
            if (m_TimeLeft <= 1e-5f)
                m_Running = false;
        }

        void Apply()
        {
            if (kind == Kind.Force)
                m_Rb.AddForce(WorldVector, mode);
            else
                m_Rb.AddTorque(WorldVector, mode);
        }
    }
}
