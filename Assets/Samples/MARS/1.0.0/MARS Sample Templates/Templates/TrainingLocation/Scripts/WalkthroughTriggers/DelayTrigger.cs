using UnityEngine;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Trigger that, when active, waits for a specified period of time
    /// </summary>
    internal class DelayTrigger : WalkthroughTrigger
    {
#pragma warning disable 649
        [SerializeField]
        [Tooltip("How long to wait until this step can complete")]
        float m_Delay = 3.0f;

        float m_TimeSinceCheck = 0.0f;

#pragma warning restore 649
        public override bool ResetTrigger()
        {
            m_TimeSinceCheck = Time.time;
            return !Check();
        }

        public override bool Check()
        {
            return (Time.time - m_TimeSinceCheck) > m_Delay;
        }
    }
}
