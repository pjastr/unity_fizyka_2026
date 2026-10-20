using UnityEngine;

namespace Fizyka
{
    /// <summary>
    /// Scena Interpolate: przełącza krok fizyki (Time.fixedDeltaTime) między wartością
    /// "przesadnie dużą" (np. 0.1 s = 10 kroków/s) a domyślną (0.02 s = 50 kroków/s).
    /// Przy rzadkich krokach fizyki różnica None / Interpolate / Extrapolate jest wyraźnie widoczna.
    /// Po opuszczeniu sceny przywraca poprzednią wartość.
    /// </summary>
    public class FixedStepToggle : DemoActionBase
    {
        public float slowStep = 0.1f;
        public float normalStep = 0.02f;
        public bool startSlow = true;

        float m_Original;
        bool m_Slow;

        void Awake()
        {
            m_Original = Time.fixedDeltaTime;
        }

        void Start()
        {
            Set(startSlow);
        }

        void OnDestroy()
        {
            Time.fixedDeltaTime = m_Original > 0f ? m_Original : normalStep;
        }

        public override void OnMainAction() => Set(!m_Slow);

        void Set(bool slow)
        {
            m_Slow = slow;
            Time.fixedDeltaTime = slow ? slowStep : normalStep;
            DemoHud.Log($"Time.fixedDeltaTime = <b>{Time.fixedDeltaTime:0.###} s</b> ({1f / Time.fixedDeltaTime:0} kroków fizyki na sekundę)");
            var l = GetComponent<WorldLabel>();
            if (l != null)
                l.extra = $"fixedDeltaTime = {Time.fixedDeltaTime:0.###} s";
        }
    }
}
