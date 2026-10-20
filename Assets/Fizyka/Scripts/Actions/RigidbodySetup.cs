using UnityEngine;

namespace Fizyka
{
    /// <summary>
    /// Ustawia parametry Rigidbody, które NIE są zapisywane w scenie (nie ma ich w Inspectorze Rigidbody):
    /// maxAngularVelocity (domyślnie 7 rad/s) oraz sleepThreshold (domyślnie z Project Settings → Physics).
    /// Wartość ujemna = zostaw domyślną.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(-50)]
    public class RigidbodySetup : MonoBehaviour
    {
        [Tooltip("Rigidbody.maxAngularVelocity [rad/s]; domyślnie w Unity 7. Ujemna = nie zmieniaj.")]
        public float maxAngularVelocity = -1f;
        [Tooltip("Rigidbody.sleepThreshold – próg energii (na kg) poniżej którego ciało zasypia. Ujemna = nie zmieniaj.")]
        public float sleepThreshold = -1f;

        void Awake()
        {
            var rb = GetComponent<Rigidbody>();
            if (maxAngularVelocity >= 0f) rb.maxAngularVelocity = maxAngularVelocity;
            if (sleepThreshold >= 0f) rb.sleepThreshold = sleepThreshold;
        }
    }
}
