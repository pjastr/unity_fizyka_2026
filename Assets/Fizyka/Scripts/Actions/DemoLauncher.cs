using UnityEngine;

namespace Fizyka
{
    /// <summary>
    /// Wyrzutnia: po naciśnięciu Spacji tworzy kulkę (Sphere + Rigidbody) i nadaje jej prędkość
    /// wzdłuż osi Z (forward) wyrzutni. Opcjonalnie sprawdza, czy kulka "przeleciała" przez ścianę
    /// (scena Collision Detection).
    /// </summary>
    public class DemoLauncher : DemoActionBase
    {
        [Header("Pocisk")]
        public float speed = 10f;
        public float radius = 0.25f;
        public float mass = 1f;
        public Material material;
        public PhysicsMaterial physicsMaterial;
        public CollisionDetectionMode collisionDetection = CollisionDetectionMode.Discrete;
        public RigidbodyInterpolation interpolation = RigidbodyInterpolation.None;
        public bool useGravity = true;
        public float lifeTime = 20f;
        public string projectileName = "Kulka";
        public bool showLabel = false;

        [Header("Wykrywanie przenikania (opcjonalne)")]
        [Tooltip("Jeśli ustawione – po strzale sprawdzamy, czy kulka znalazła się za tym obiektem (wzdłuż kierunku strzału).")]
        public Transform passCheckWall;
        public float passCheckDelay = 0.6f;
        public string passCheckName = "";

        GameObject m_Last;
        float m_CheckAt = -1f;

        public override void OnMainAction() => Fire();

        public GameObject Fire()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = projectileName;
            go.transform.SetPositionAndRotation(transform.position, transform.rotation);
            go.transform.localScale = Vector3.one * (radius * 2f);
            if (material != null)
                go.GetComponent<Renderer>().sharedMaterial = material;
            var col = go.GetComponent<SphereCollider>();
            if (physicsMaterial != null)
                col.sharedMaterial = physicsMaterial;

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.useGravity = useGravity;
            rb.collisionDetectionMode = collisionDetection;
            rb.interpolation = interpolation;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;
            rb.linearVelocity = transform.forward * speed;

            if (showLabel)
            {
                var l = go.AddComponent<WorldLabel>();
                l.header = projectileName;
                l.info = WorldLabel.Info.Speed;
                l.worldOffset = new Vector3(0, radius + 0.4f, 0);
            }

            if (lifeTime > 0f)
                Destroy(go, lifeTime);

            m_Last = go;
            if (passCheckWall != null)
                m_CheckAt = Time.time + passCheckDelay;
            return go;
        }

        void Update()
        {
            if (m_CheckAt < 0f || Time.time < m_CheckAt)
                return;
            m_CheckAt = -1f;
            if (m_Last == null || passCheckWall == null)
                return;

            Vector3 dir = transform.forward;
            float wallDist = Vector3.Dot(passCheckWall.position - transform.position, dir);
            float ballDist = Vector3.Dot(m_Last.transform.position - transform.position, dir);
            bool passed = ballDist > wallDist;
            string label = string.IsNullOrEmpty(passCheckName) ? name : passCheckName;
            DemoHud.Log(passed
                ? $"<b>{label}</b>: <color=#ff6b6b>kulka PRZENIKNĘŁA przez ścianę!</color>"
                : $"<b>{label}</b>: <color=#7dff8a>kolizja wykryta – kulka zatrzymana/odbita.</color>");

            var wl = GetComponent<WorldLabel>();
            if (wl != null)
                wl.extra = passed ? "<color=#ff6b6b>PRZENIKNĘŁA</color>" : "<color=#7dff8a>odbita</color>";
        }
    }
}
