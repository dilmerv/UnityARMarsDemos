using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Contains information needed to process one step of a walkthrough
    /// </summary>
    public class WalkthroughStep : MonoBehaviour
    {
        /// Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        static List<WalkthroughTrigger> k_TriggersToRemove = new List<WalkthroughTrigger>();

        [SerializeField]
        [Tooltip("Objects to enable when this step is active ")]
        List<GameObject> m_Visuals = new List<GameObject>();

#pragma warning disable 649
        [SerializeField]
        [Tooltip("Actions to call when the step starts.")]
        UnityEvent m_OnStepBegin;

        [SerializeField]
        [Tooltip("Actions to call when the step completes.")]
        UnityEvent m_OnStepComplete;

        [SerializeField]
        [Tooltip("The purpose of this step")]
        string m_Description;
#pragma warning restore 649

        [SerializeField]
        [Tooltip("If true, this step cannot be skipped until completed at least once.")]
        bool m_BlockUntilComplete = false;

        [SerializeField]
        [Tooltip("If true, this step will automatically progress when complete - unless explicitly skipped to.")]
        bool m_AutoProgressOnComplete = true;

        bool m_Started = false;
        bool m_AutoProgressEnabled = true;
        bool m_StepInvoked = false;

        Action<bool> m_OnComplete = null;

        List<WalkthroughTrigger> m_Triggers = new List<WalkthroughTrigger>();
        List<WalkthroughTrigger> m_RemainingTriggers = new List<WalkthroughTrigger>();

        /// <summary>
        /// The purpose of this step. Appends a (Complete) if complete and normally has triggers. 
        /// </summary>
        public string Description
        {
            get
            {
                return $"{m_Description}{(Completed && m_Triggers.Count > 0 ? " (Complete)" : "") }";
            }
        }

        /// <summary>
        /// Ensures the step visuals are hidden until active and that all triggers are accounted for
        /// </summary>
        public void Initialize()
        {
            if (!m_Started)
                SetVisualsState(false);

            GetComponents(m_Triggers);
        }

        /// <summary>
        /// Returns true if this step does not currently have any triggers remaining to fire
        /// </summary>
        public bool CanProgress
        {
            get { return (!m_BlockUntilComplete || (m_RemainingTriggers.Count == 0)); }
        }

        /// <summary>
        /// Returns true if this step does not block, or has been completed at least once.
        /// </summary>
        public bool CanSkip
        {
            get { return (!m_BlockUntilComplete || Completed); }
        }

        /// <summary>
        /// True if this step's triggers have been activated at least once
        /// </summary>
        public bool Completed { get; private set; }

        /// <summary>
        /// Makes this step and its triggers the active focus of a walkthrough
        /// </summary>
        /// <param name="onComplete">Callback to fire when this step's triggers are complete</param>
        /// <param name="allowAutoProgress">If this step is allow to auto-progress during this activation</param>
        public void StartStep(Action<bool> onComplete, bool allowAutoProgress = true)
        {
            //Debug.Log($"Starting step {name}.");
            if (m_Started)
                return;

            //Debug.Log($"Did not skip step {name}.");

            // Autoprogression is enabled only if the step AND walkthrough allow it
            m_AutoProgressEnabled = allowAutoProgress && m_AutoProgressOnComplete;

            SetVisualsState(true);

            m_OnComplete = onComplete;

            m_Started = true;

            if (m_Triggers.Count == 0 && m_AutoProgressEnabled)
            {
                Debug.Log($"Step {name} automatically complete; no triggers to delay step!");
                CompleteStep();
                return;
            }

            foreach (var currentTrigger in m_Triggers)
            {
                if (currentTrigger.ResetTrigger())
                    m_RemainingTriggers.Add(currentTrigger);
            }

            if (m_RemainingTriggers.Count == 0)
            {
                Debug.Log($"Step {name} already completed.");
                CompleteStep();
                return;
            }

            if (m_RemainingTriggers.Count > 0)
                m_AutoProgressEnabled = m_AutoProgressOnComplete;
        }

        /// <summary>
        /// Ends this step being the focus of the current walkthrough
        /// </summary>
        public void CancelStep()
        {
            SetVisualsState(false);

            if (!m_Started)
                return;

            m_OnComplete = null;
            m_Started = false;

            m_RemainingTriggers.Clear();
        }

        void CompleteStep()
        {
            if (!m_Started)
                return;

            Completed = true;

            m_OnComplete?.Invoke(m_AutoProgressEnabled);
            m_OnComplete = null;
            m_Started = false;

            // We disable visuals if the the next step is being activated
            if (m_AutoProgressEnabled)
                SetVisualsState(false);

            m_RemainingTriggers.Clear();
        }

        void Update()
        {
            // If this step is running, check remaining triggers.  Any triggers that are now met get removed.
            // If there are no triggers left, then the step is complete.
            if (!m_Started)
                return;

            if (m_RemainingTriggers.Count == 0)
                return;

            k_TriggersToRemove.Clear();
            foreach (var currentTrigger in m_RemainingTriggers)
            {
                if (currentTrigger.Check())
                    k_TriggersToRemove.Add(currentTrigger);
            }

            foreach (var toRemove in k_TriggersToRemove)
            {
                m_RemainingTriggers.Remove(toRemove);
            }
            k_TriggersToRemove.Clear();

            if (m_RemainingTriggers.Count == 0)
            {
                CompleteStep();
                return;
            }
        }

        void SetVisualsState(bool enabled)
        {
            if (m_Visuals != null)
            {
                foreach (var currentVisual in m_Visuals)
                {
                    if (currentVisual != null)
                        currentVisual.SetActive(enabled);
                }
            }

            if (m_StepInvoked == enabled)
                return;

            m_StepInvoked = enabled;

            if (enabled && m_OnStepBegin != null)
                m_OnStepBegin.Invoke();

            if (!enabled && m_OnStepComplete != null)
                m_OnStepComplete.Invoke();
        }
    }
}
