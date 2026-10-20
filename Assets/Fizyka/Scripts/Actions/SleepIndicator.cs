using UnityEngine;

namespace Fizyka
{
    /// <summary>
    /// Koloruje obiekt zależnie od stanu Rigidbody.IsSleeping():
    /// niebieski = uśpiony (fizyka go nie liczy), pomarańczowy = aktywny.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SleepIndicator : MonoBehaviour
    {
        public Color sleepingColor = new Color(0.3f, 0.5f, 1f);
        public Color awakeColor = new Color(1f, 0.6f, 0.15f);
        public bool logChanges = true;

        Rigidbody m_Rb;
        Renderer m_Renderer;
        bool m_WasSleeping;

        void Awake()
        {
            m_Rb = GetComponent<Rigidbody>();
            m_Renderer = GetComponent<Renderer>();
        }

        void Update()
        {
            bool sleeping = m_Rb.IsSleeping();
            if (m_Renderer != null)
                m_Renderer.material.color = sleeping ? sleepingColor : awakeColor;
            if (logChanges && sleeping != m_WasSleeping && Time.timeSinceLevelLoad > 0.1f)
                DemoHud.Log(sleeping ? $"{name}: <color=#7fb2ff>zasnął</color> (sleepThreshold = {m_Rb.sleepThreshold:0.###})"
                                     : $"{name}: <color=#ffb347>obudził się</color>");
            m_WasSleeping = sleeping;
        }
    }
}
