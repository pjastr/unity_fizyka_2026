using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fizyka
{
    /// <summary>
    /// Wspólny "menedżer" każdej sceny demonstracyjnej:
    ///  • panel z tytułem i opisem (po polsku) – przełączany klawiszem C (Player/Crouch),
    ///  • pasek podpowiedzi sterowania,
    ///  • dziennik zdarzeń (np. OnCollisionEnter),
    ///  • globalne skróty: Spacja – akcja sceny, przytrzymaj E – reset sceny,
    ///    1/2 – poprzednia/następna scena, Esc – wyjście (w buildzie).
    /// Obiekty UI są tworzone przez generator scen (menu Fizyka) i widoczne w hierarchii.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class DemoHud : MonoBehaviour
    {
        public static DemoHud Instance { get; private set; }

        [Header("Treść")]
        public string title = "Tytuł sceny";
        [TextArea(5, 30)] public string description = "Opis";
        [TextArea(2, 6)] public string sceneHint = "Spacja – start eksperymentu";
        [Tooltip("Czy w podpowiedzi pokazywać sterowanie swobodną kamerą")]
        public bool freeCameraControls = true;

        [Header("Referencje UI (ustawiane przez generator)")]
        public GameObject descriptionPanel;
        public Text titleText;
        public Text descriptionText;
        public Text hintText;
        public GameObject logPanel;
        public Text logText;
        public RectTransform labelsRoot;
        public Font font;

        [Header("Dziennik")]
        public bool showLog = false;
        public int maxLogLines = 12;

        readonly List<string> m_Log = new List<string>();
        readonly StringBuilder m_Sb = new StringBuilder();

        void Awake()
        {
            Instance = this;
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Refresh();
            if (logPanel != null)
                logPanel.SetActive(showLog);
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Refresh()
        {
            if (titleText != null) titleText.text = title;
            if (descriptionText != null) descriptionText.text = description;
            if (hintText != null)
            {
                int idx = SceneManager.GetActiveScene().buildIndex;
                int count = SceneManager.sceneCountInBuildSettings;
                string sceneInfo = idx >= 0 && count > 0 ? $"Scena {idx + 1}/{count}" : "Scena";
                string cam = freeCameraControls
                    ? "<b>PPM + mysz</b> – obrót kamery   <b>WASD</b> – ruch kamery   <b>Shift</b> – szybciej   <b>Kółko</b> – przybliżenie   "
                    : "";
                hintText.text =
                    $"<b>{sceneInfo}</b>   {sceneHint}\n" +
                    "<b>LPM</b> – szturchnij obiekt   " + cam + "<b>C</b> – opis   " +
                    "<b>przytrzymaj E</b> – reset   <b>1/2</b> – poprzednia/następna scena   <b>Esc</b> – wyjście";
            }
        }

        void Update()
        {
            if (DemoInput.PressedThisFrame(DemoInput.Jump) && !SuppressMainAction)
                DemoActionBase.InvokeAll();

            if (DemoInput.PerformedThisFrame(DemoInput.Interact))
                ReloadScene();

            if (DemoInput.PressedThisFrame(DemoInput.Crouch) && descriptionPanel != null)
                descriptionPanel.SetActive(!descriptionPanel.activeSelf);

            if (DemoInput.PressedThisFrame(DemoInput.Next))
                LoadRelative(+1);
            if (DemoInput.PressedThisFrame(DemoInput.Previous))
                LoadRelative(-1);

            if (DemoInput.PressedThisFrame(DemoInput.Cancel))
            {
#if !UNITY_EDITOR
                Application.Quit();
#endif
            }
        }

        /// <summary>Scena 16 (CharacterController) używa Spacji do skoku – wtedy nie wołamy akcji sceny.</summary>
        public static bool SuppressMainAction { get; set; }

        public static void ReloadScene()
        {
            var s = SceneManager.GetActiveScene();
            if (s.buildIndex >= 0)
                SceneManager.LoadScene(s.buildIndex);
            else
                SceneManager.LoadScene(s.name);
        }

        public static void LoadRelative(int delta)
        {
            int count = SceneManager.sceneCountInBuildSettings;
            if (count <= 0)
            {
                Debug.LogWarning("[Fizyka] Brak scen w Build Settings – użyj menu Fizyka > Generuj wszystkie sceny.");
                return;
            }
            int idx = SceneManager.GetActiveScene().buildIndex;
            if (idx < 0) idx = 0;
            int next = ((idx + delta) % count + count) % count;
            SceneManager.LoadScene(next);
        }

        // ---------------- dziennik ----------------

        public static void Log(string line)
        {
            if (Instance == null)
            {
                Debug.Log("[Fizyka] " + line);
                return;
            }
            Instance.AddLog(line);
        }

        void AddLog(string line)
        {
            if (logPanel != null && !logPanel.activeSelf)
                logPanel.SetActive(true);

            m_Log.Add($"<color=#9fd3ff>{Time.time:0.00}s</color>  {line}");
            while (m_Log.Count > maxLogLines)
                m_Log.RemoveAt(0);

            if (logText != null)
            {
                m_Sb.Clear();
                m_Sb.Append("<b>Dziennik zdarzeń</b>\n");
                for (int i = 0; i < m_Log.Count; i++)
                    m_Sb.Append(m_Log[i]).Append('\n');
                logText.text = m_Sb.ToString();
            }
        }

        // ---------------- etykiety ----------------

        public Text CreateLabelText(string name)
        {
            if (labelsRoot == null)
                return null;
            var go = new GameObject("Label_" + name, typeof(RectTransform));
            go.transform.SetParent(labelsRoot, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(260, 120);
            rt.pivot = new Vector2(0.5f, 0f);
            var t = go.AddComponent<Text>();
            t.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 15;
            t.alignment = TextAnchor.LowerCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;
            t.color = Color.white;
            t.raycastTarget = false;
            var sh = go.AddComponent<Outline>();
            sh.effectColor = new Color(0, 0, 0, 0.85f);
            sh.effectDistance = new Vector2(1.5f, -1.5f);
            return t;
        }
    }
}
