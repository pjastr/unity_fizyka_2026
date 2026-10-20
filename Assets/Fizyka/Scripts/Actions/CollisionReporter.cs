using UnityEngine;

namespace Fizyka
{
    /// <summary>
    /// Pokazuje działanie zdarzeń OnCollisionEnter / OnCollisionStay / OnCollisionExit.
    /// Uwaga: zdarzenia pojawiają się tylko, gdy co najmniej jeden z obiektów ma
    /// dynamiczny (nie-kinematyczny) Rigidbody, a żaden collider nie jest triggerem.
    /// </summary>
    public class CollisionReporter : MonoBehaviour
    {
        public string displayName = "";
        public Color flashColor = new Color(1f, 0.25f, 0.2f);
        public Color stayColor = new Color(1f, 0.75f, 0.2f);
        public bool logStay = false;

        Renderer m_Renderer;
        Color m_BaseColor;
        float m_Flash;
        int m_Enter, m_Exit, m_StayFrames, m_Touching;
        float m_LastImpulse;
        WorldLabel m_Label;

        string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;

        void Awake()
        {
            m_Renderer = GetComponent<Renderer>();
            if (m_Renderer != null)
                m_BaseColor = m_Renderer.material.color; // kopia materiału tylko dla tego obiektu
            m_Label = GetComponent<WorldLabel>();
        }

        void OnCollisionEnter(Collision c)
        {
            m_Enter++;
            m_Touching++;
            m_Flash = 0.35f;
            m_LastImpulse = c.impulse.magnitude;
            DemoHud.Log($"<color=#ff8a80>OnCollisionEnter</color>  {DisplayName} ← {c.gameObject.name}  " +
                        $"(prędkość względna {c.relativeVelocity.magnitude:0.0} m/s, impuls {m_LastImpulse:0.00} N·s, punktów styku: {c.contactCount})");
        }

        void OnCollisionStay(Collision c)
        {
            m_StayFrames++;
            if (logStay && m_StayFrames % 50 == 0)
                DemoHud.Log($"<color=#ffd180>OnCollisionStay</color>  {DisplayName} – {c.gameObject.name} (wywołań: {m_StayFrames})");
        }

        void OnCollisionExit(Collision c)
        {
            m_Exit++;
            m_Touching = Mathf.Max(0, m_Touching - 1);
            DemoHud.Log($"<color=#80d8ff>OnCollisionExit</color>   {DisplayName} – {c.gameObject.name}");
        }

        void Update()
        {
            if (m_Renderer != null)
            {
                Color target = m_Touching > 0 ? stayColor : m_BaseColor;
                m_Renderer.material.color = m_Flash > 0f ? flashColor : target;
                m_Flash -= Time.deltaTime;
            }
            if (m_Label != null)
                m_Label.extra = $"Enter: {m_Enter}   Stay: {m_StayFrames}   Exit: {m_Exit}\nostatni impuls: {m_LastImpulse:0.00} N·s";
        }
    }
}
