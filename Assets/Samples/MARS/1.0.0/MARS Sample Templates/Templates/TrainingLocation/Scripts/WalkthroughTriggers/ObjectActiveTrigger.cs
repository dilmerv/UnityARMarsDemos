using UnityEngine;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Trigger that, when active, waits for a gameobject to be active
    /// </summary>
    internal class ObjectActiveTrigger : WalkthroughTrigger
    {
#pragma warning disable 649
        [SerializeField]
        [Tooltip("The object that, when active, allows this trigger to pass.")]
        GameObject m_ObjectToTrack;

#pragma warning restore 649
        public override bool ResetTrigger()
        {
            if (m_ObjectToTrack == null)
                return false;

            return !Check();
        }

        public override bool Check()
        {
            return m_ObjectToTrack.activeInHierarchy;
        }
    }
}
