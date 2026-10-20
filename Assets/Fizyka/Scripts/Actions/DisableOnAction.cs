using UnityEngine;

namespace Fizyka
{
    /// <summary>
    /// "Zapadnia" / blokada: po naciśnięciu Spacji wyłącza ten obiekt (collider i renderer znikają),
    /// więc ciała, które na nim leżały, zaczynają spadać/zsuwać się.
    /// Leżące na nim obiekty mogą już "spać" – budzimy wszystko w pobliżu.
    /// </summary>
    public class DisableOnAction : DemoActionBase
    {
        public float wakeRadius = 6f;

        public override void OnMainAction()
        {
            var col = GetComponent<Collider>();
            Bounds b = col != null ? col.bounds : new Bounds(transform.position, Vector3.one);
            b.Expand(wakeRadius);
            foreach (var c in Physics.OverlapBox(b.center, b.extents))
            {
                if (c.attachedRigidbody != null)
                    c.attachedRigidbody.WakeUp();
            }
            gameObject.SetActive(false);
        }
    }
}
