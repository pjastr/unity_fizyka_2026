using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Fizyka
{
    /// <summary>
    /// Etykieta "przyklejona" do obiektu w świecie (rysowana na Canvasie HUD).
    /// Może pokazywać na żywo wybrane parametry Rigidbody.
    /// </summary>
    public class WorldLabel : MonoBehaviour
    {
        [Flags]
        public enum Info
        {
            None = 0,
            Mass = 1,
            Speed = 2,
            AngularSpeed = 4,
            Sleeping = 8,
            Height = 16,
            LinearDamping = 32,
            AngularDamping = 64,
            Inertia = 128,
            Velocity = 256,
            Kinematic = 512,
            VelocityZ = 1024,
        }

        [TextArea(1, 4)] public string header = "";
        public Info info = Info.None;
        public Vector3 worldOffset = new Vector3(0, 1f, 0);
        public Color color = Color.white;
        public int fontSize = 15;

        /// <summary>Dodatkowy tekst ustawiany przez inne skrypty (np. liczniki zdarzeń).</summary>
        [NonSerialized] public string extra = "";

        Rigidbody m_Rb;
        Text m_Text;
        Camera m_Cam;
        RectTransform m_Canvas;
        readonly StringBuilder m_Sb = new StringBuilder();

        void Start()
        {
            m_Rb = GetComponent<Rigidbody>();
            if (DemoHud.Instance != null)
            {
                m_Text = DemoHud.Instance.CreateLabelText(name);
                if (m_Text != null)
                {
                    m_Text.color = color;
                    m_Text.fontSize = fontSize;
                    m_Canvas = DemoHud.Instance.labelsRoot.root as RectTransform;
                }
            }
        }

        void OnDisable()
        {
            if (m_Text != null) m_Text.enabled = false;
        }

        void OnEnable()
        {
            if (m_Text != null) m_Text.enabled = true;
        }

        void OnDestroy()
        {
            if (m_Text != null) Destroy(m_Text.gameObject);
        }

        void LateUpdate()
        {
            if (m_Text == null)
                return;
            if (m_Cam == null)
                m_Cam = Camera.main;
            if (m_Cam == null)
                return;

            Vector3 basePos = m_Rb != null ? m_Rb.worldCenterOfMass : transform.position;
            Vector3 sp = m_Cam.WorldToScreenPoint(basePos + worldOffset);
            if (sp.z <= 0.1f)
            {
                m_Text.enabled = false;
                return;
            }
            m_Text.enabled = true;

            // przeliczenie z pikseli ekranu na jednostki Canvasa (CanvasScaler)
            float scale = m_Canvas != null ? m_Canvas.lossyScale.x : 1f;
            if (scale <= 0f) scale = 1f;
            var rt = (RectTransform)m_Text.transform;
            rt.position = new Vector3(sp.x, sp.y, 0f);
            rt.localScale = Vector3.one;

            m_Text.text = Build();
        }

        string Build()
        {
            m_Sb.Clear();
            if (!string.IsNullOrEmpty(header))
                m_Sb.Append("<b>").Append(header).Append("</b>");

            if (m_Rb != null)
            {
                if ((info & Info.Mass) != 0) Line().Append($"m = {m_Rb.mass:0.##} kg");
                if ((info & Info.LinearDamping) != 0) Line().Append($"Linear Damping = {m_Rb.linearDamping:0.###}");
                if ((info & Info.AngularDamping) != 0) Line().Append($"Angular Damping = {m_Rb.angularDamping:0.###}");
                if ((info & Info.Speed) != 0) Line().Append($"|v| = {m_Rb.linearVelocity.magnitude:0.00} m/s");
                if ((info & Info.Velocity) != 0)
                {
                    var v = m_Rb.linearVelocity;
                    Line().Append($"v = ({v.x:0.0}; {v.y:0.0}; {v.z:0.0})");
                }
                if ((info & Info.VelocityZ) != 0) Line().Append($"v = {m_Rb.linearVelocity.z:0.00} m/s");
                if ((info & Info.AngularSpeed) != 0) Line().Append($"ω = {m_Rb.angularVelocity.magnitude:0.00} rad/s");
                if ((info & Info.Inertia) != 0) Line().Append($"I(y) = {m_Rb.inertiaTensor.y:0.###} kg·m²");
                if ((info & Info.Height) != 0) Line().Append($"h = {transform.position.y:0.00} m");
                if ((info & Info.Kinematic) != 0) Line().Append(m_Rb.isKinematic ? "isKinematic = true" : "isKinematic = false");
                if ((info & Info.Sleeping) != 0)
                    Line().Append(m_Rb.IsSleeping() ? "<color=#7fb2ff>ŚPI (IsSleeping)</color>" : "<color=#ffb347>aktywny</color>");
            }
            else if ((info & Info.Height) != 0)
            {
                Line().Append($"h = {transform.position.y:0.00} m");
            }

            if (!string.IsNullOrEmpty(extra))
                Line().Append(extra);
            return m_Sb.ToString();
        }

        StringBuilder Line()
        {
            if (m_Sb.Length > 0) m_Sb.Append('\n');
            return m_Sb;
        }
    }
}
