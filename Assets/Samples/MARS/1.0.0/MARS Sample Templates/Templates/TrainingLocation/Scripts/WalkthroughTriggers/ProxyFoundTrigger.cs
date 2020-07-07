#if INCLUDE_MARS
using Unity.MARS;
using UnityEngine;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Trigger that, when active, waits for a proxy to be tracking within MARS
    /// </summary>
    internal class ProxyFoundTrigger : WalkthroughTrigger
    {
#pragma warning disable 649
        //[SerializeField]
        [Tooltip("The proxy that, when matched, allows this trigger to pass.")]
        public Proxy ProxyToFind;
#pragma warning restore 649
        public override bool ResetTrigger()
        {
            if (ProxyToFind == null)
                return false;

            return !Check();
        }

        public override bool Check()
        {
            return ProxyToFind.queryState.HasFlag(QueryState.Tracking);
        }
    }
}
#endif
