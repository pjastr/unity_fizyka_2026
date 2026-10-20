using UnityEngine;

namespace Fizyka
{
    /// <summary>
    /// Ciało czeka "zamrożone" jako kinematyczne; po Spacji przełączamy isKinematic = false
    /// i od tej chwili działa na nie pełna symulacja (grawitacja, kolizje).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ReleaseOnAction : DemoActionBase
    {
        public override void OnMainAction()
        {
            var rb = GetComponent<Rigidbody>();
            if (!rb.isKinematic)
                return;
            rb.isKinematic = false;
            rb.WakeUp();
        }
    }
}
