using System.Collections.Generic;
using UnityEngine;

namespace Fizyka
{
    /// <summary>
    /// Strefa-trigger (Collider.isTrigger = true): nie zatrzymuje obiektów, ale wywołuje
    /// OnTriggerEnter / OnTriggerStay / OnTriggerExit. Liczy obiekty wewnątrz i zmienia kolor.
    /// </summary>
    public class TriggerZoneReporter : MonoBehaviour
    {
        public string displayName = "Strefa";
        public Color emptyColor = new Color(0.2f, 0.9f, 0.4f, 0.25f);
        public Color occupiedColor = new Color(1f, 0.6f, 0.1f, 0.45f);

        readonly HashSet<Collider> m_Inside = new HashSet<Collider>();
        Renderer m_Renderer;
        WorldLabel m_Label;
        int m_Enter, m_Exit, m_Stay;

        void Awake()
        {
            m_Renderer = GetComponent<Renderer>();
            m_Label = GetComponent<WorldLabel>();
        }

        void OnTriggerEnter(Collider other)
        {
            m_Inside.Add(other);
            m_Enter++;
            DemoHud.Log($"<color=#b9f6ca>OnTriggerEnter</color>  {displayName} ← {other.name}");
        }

        void OnTriggerStay(Collider other)
        {
            m_Stay++;
        }

        void OnTriggerExit(Collider other)
        {
            m_Inside.Remove(other);
            m_Exit++;
            DemoHud.Log($"<color=#80d8ff>OnTriggerExit</color>   {displayName} – {other.name}");
        }

        void Update()
        {
            m_Inside.RemoveWhere(c => c == null || !c.gameObject.activeInHierarchy);
            if (m_Renderer != null)
                m_Renderer.material.color = m_Inside.Count > 0 ? occupiedColor : emptyColor;
            if (m_Label != null)
                m_Label.extra = $"w środku: {m_Inside.Count}\nEnter: {m_Enter}  Stay: {m_Stay}  Exit: {m_Exit}";
        }
    }
}
