using System.Collections.Generic;
using UnityEngine;

namespace Fizyka
{
    /// <summary>
    /// Bazowa klasa dla wszystkiego, co reaguje na "akcję główną" sceny (Spacja = Player/Jump).
    /// Komponenty rejestrują się same – DemoHud wywołuje OnMainAction() na wszystkich aktywnych.
    /// </summary>
    public abstract class DemoActionBase : MonoBehaviour
    {
        static readonly List<DemoActionBase> s_All = new List<DemoActionBase>();

        protected virtual void OnEnable()
        {
            if (!s_All.Contains(this))
                s_All.Add(this);
        }

        protected virtual void OnDisable()
        {
            s_All.Remove(this);
        }

        /// <summary>Wywoływane po naciśnięciu Spacji (akcja Player/Jump).</summary>
        public abstract void OnMainAction();

        public static void InvokeAll()
        {
            // kopia – akcje mogą włączać/wyłączać inne komponenty
            var copy = s_All.ToArray();
            foreach (var a in copy)
            {
                if (a != null && a.isActiveAndEnabled)
                    a.OnMainAction();
            }
        }

        public static int Count => s_All.Count;
    }
}
