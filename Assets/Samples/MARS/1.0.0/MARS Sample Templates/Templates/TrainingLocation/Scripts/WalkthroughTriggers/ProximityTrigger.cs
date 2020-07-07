using UnityEngine;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Trigger that, when active, waits for two objects to be within a specified distance
    /// </summary>
    internal class ProximityTrigger : WalkthroughTrigger
    {
#pragma warning disable 649
        [SerializeField]
        [Tooltip("The first of the paired objects")]
        Transform m_Source;

        [SerializeField]
        [Tooltip("The second of the paired objects")]
        Transform m_Target;
#pragma warning restore 649
        [SerializeField]
        [Tooltip("The maximum distance between these objects to for the trigger to pass")]
        float m_Distance = 1.0f;

        public override bool ResetTrigger()
        {
            if (m_Source == null || m_Target == null)
                return false;

            return !Check();
        }

        public override bool Check()
        {
            // Get the distance between source and target
            return (m_Target.position - m_Source.position).sqrMagnitude < (m_Distance * m_Distance);
        }
    }
}
