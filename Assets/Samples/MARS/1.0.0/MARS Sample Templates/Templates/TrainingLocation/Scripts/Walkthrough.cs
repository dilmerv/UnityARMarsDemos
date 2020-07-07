using System;
using UnityEngine;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Defines a walkthrough - a series of steps gated by triggers
    /// </summary>
    public class Walkthrough : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        [Tooltip("The name of this walkthrough - for reference by UI")]
        string m_WalkthroughName;

        [SerializeField]
        [Tooltip("All of the steps this walkthrough requires, in order")]
        WalkthroughStep[] m_Steps;
#pragma warning restore 649

        /// <summary>
        /// The name of the walkthrough experience
        /// </summary>
        public string WalkthroughName { get { return m_WalkthroughName; } }

        /// <summary>
        /// All of the steps this walkthrough requires, in order
        /// </summary>
        public WalkthroughStep[] steps { get { return m_Steps; } }

        /// <summary>
        /// The currently active step of the walkthrough
        /// </summary>
        public int CurrentStep { get; private set; } = 0;

        /// <summary>
        /// Event that is raised whenever the state of the walkthrough has changed.
        /// </summary>
        public Action WalkthroughChangedCallback;

        /// <summary>
        /// Shifts to another step of the walkthrough
        /// </summary>
        /// <param name="stepIndex">The step to make active</param>
        /// <param name="autoProgressIfComplete">If true, allows for skipping to the subsequent step if the current one is already complete.</param>
        public void SkipToStep(int stepIndex, bool autoProgressIfComplete = false)
        {
            // Ignore invalid indices and no-ops
            if (stepIndex < 0 || stepIndex >= m_Steps.Length || stepIndex == CurrentStep)
                return;

            // If any steps between our current step and the next are incomplete and block progression, we do not allow skipping to occur. This prevents
            // problems like skipping to a step where relocalization has not yet occurred.
            if (stepIndex > CurrentStep)
            {
                for (var testStepIndex = CurrentStep; testStepIndex < stepIndex; testStepIndex++)
                {
                    var testStep = m_Steps[testStepIndex];
                    if (!testStep.CanSkip)
                    {
                        Debug.LogWarning($"Can't skip past incomplete step {testStep.name}");
                        WalkthroughChangedCallback?.Invoke();
                        return;
                    }
                }
            }

            // If a valid step is already being displayed, set it back to inactive now
            if (CurrentStep >= 0 && CurrentStep < m_Steps.Length)
                m_Steps[CurrentStep].CancelStep();

            CurrentStep = stepIndex;
            m_Steps[CurrentStep].StartStep(OnStepComplete, autoProgressIfComplete);

            WalkthroughChangedCallback?.Invoke();
        }

        void Awake()
        {
            // We ensure each walkthrough step is ready to work (as we can't ensure components are waking in a determined order), then start the first step
            foreach (var step in m_Steps)
            {
                step.Initialize();
            }
            if (m_Steps != null && m_Steps.Length > 0)
                m_Steps[CurrentStep].StartStep(OnStepComplete);

            WalkthroughChangedCallback?.Invoke();
        }

        void OnStepComplete(bool autoProgress)
        {
            // We still call the changed callback even if we are not auto-progressing, as some UI may want to update labels or controls
            if (!autoProgress)
            {
                WalkthroughChangedCallback?.Invoke();
                return;
            }

            // If we are auto-progressing, increment the step index and start the process again
            CurrentStep++;

            if (m_Steps == null || CurrentStep >= m_Steps.Length)
                return;

            m_Steps[CurrentStep].StartStep(OnStepComplete);

            WalkthroughChangedCallback?.Invoke();
        }
    }
}
