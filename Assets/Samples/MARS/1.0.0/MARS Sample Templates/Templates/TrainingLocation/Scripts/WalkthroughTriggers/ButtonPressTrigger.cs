using UnityEngine;
using UnityEngine.UI;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Trigger that, when active, waits for a UI button to be pressed
    /// </summary>
    internal class ButtonPressTrigger : WalkthroughTrigger
    {
#pragma warning disable 649
        [SerializeField]
        [Tooltip("The UI button that when pressed, will allow this trigger to pass.")]
        Button m_ButtonToPress;
#pragma warning restore 649
        bool m_Triggered = false;

        public override bool ResetTrigger()
        {
            m_Triggered = false;
            if (m_ButtonToPress == null)
                return false;

            m_ButtonToPress.onClick.RemoveListener(ButtonPressHandler);
            m_ButtonToPress.onClick.AddListener(ButtonPressHandler);
            return true;
        }

        public override bool Check()
        {
            return m_Triggered;
        }

        void ButtonPressHandler()
        {
            m_Triggered = true;
        }
    }
}
