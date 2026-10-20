using UnityEngine;

namespace Fizyka
{
    /// <summary>
    /// Trigger "komin z wiatrem": w OnTriggerStay (wywoływanym co krok fizyki) dodaje
    /// przyspieszenie do góry (ForceMode.Acceleration – niezależnie od masy).
    /// Gdy przyspieszenie &gt; g, obiekt unosi się; po wyjściu ze strefy znów spada.
    /// </summary>
    public class UpdraftZone : MonoBehaviour
    {
        public Vector3 acceleration = new Vector3(0, 14f, 0);

        void OnTriggerStay(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
                rb.AddForce(acceleration, ForceMode.Acceleration);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody != null)
                DemoHud.Log($"<color=#b3e5fc>OnTriggerEnter</color>  Komin ← {other.name} – zaczyna działać siła w OnTriggerStay");
        }
    }
}
