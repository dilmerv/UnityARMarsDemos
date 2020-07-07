using UnityEngine;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Trigger that, when active, waits for one object to 'view' another object within a cone
    /// </summary>
    internal class GazeTrigger : WalkthroughTrigger
    {
#pragma warning disable 649
        [SerializeField]
        [Tooltip("The object that is doing the viewing.")]
        Transform m_Eye;

        [SerializeField]
        [Tooltip("The object that is being viewed.")]
        Transform m_Target;
#pragma warning restore 649

        [SerializeField]
        [Tooltip("The cone the viewed object must be within.")]
        float m_FOV = 60.0f;

        public override bool ResetTrigger()
        {
            if (m_Eye == null || m_Target == null)
                return false;

            
            return !Check();
        }

        public override bool Check()
        {
            // Get the angle between eye and target
            var targetToEye = (m_Target.position - m_Eye.position).normalized;
            return Vector3.Angle(m_Eye.forward, targetToEye) < m_FOV;
        }
    }
}
