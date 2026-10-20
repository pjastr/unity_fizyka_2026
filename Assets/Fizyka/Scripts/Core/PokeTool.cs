using UnityEngine;

namespace Fizyka
{
    /// <summary>
    /// LPM (Player/Attack) – "szturchnięcie": promień (Raycast) z kamery przez kursor;
    /// jeśli trafi w dynamiczny Rigidbody, przykłada impuls w punkcie trafienia
    /// (AddForceAtPosition, ForceMode.Impulse). Ten sam impuls daje różne efekty dla różnych mas!
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class PokeTool : MonoBehaviour
    {
        [Tooltip("Impuls w N·s przykładany przy kliknięciu")]
        public float impulse = 4f;
        public float maxDistance = 200f;
        public bool logPokes = true;

        Camera m_Cam;

        void Awake() => m_Cam = GetComponent<Camera>();

        void Update()
        {
            if (!DemoInput.PressedThisFrame(DemoInput.Attack))
                return;

            Ray ray = m_Cam.ScreenPointToRay(DemoInput.PointerPosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, ~0, QueryTriggerInteraction.Ignore))
                return;

            Rigidbody rb = hit.rigidbody;
            if (rb == null)
            {
                if (logPokes) DemoHud.Log($"LPM: <b>{hit.collider.name}</b> nie ma Rigidbody (obiekt statyczny) – nic się nie dzieje.");
                return;
            }
            if (rb.isKinematic)
            {
                if (logPokes) DemoHud.Log($"LPM: <b>{rb.name}</b> jest kinematyczny – siły go nie przesuwają.");
                return;
            }

            rb.WakeUp();
            rb.AddForceAtPosition(ray.direction * impulse, hit.point, ForceMode.Impulse);
            if (logPokes) DemoHud.Log($"LPM: impuls {impulse:0.#} N·s → <b>{rb.name}</b> (m = {rb.mass:0.##} kg, Δv ≈ {impulse / rb.mass:0.##} m/s)");
        }
    }
}
