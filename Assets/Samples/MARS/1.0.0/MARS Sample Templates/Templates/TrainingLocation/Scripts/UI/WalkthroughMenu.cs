#if INCLUDE_MARS
using System.Collections.Generic;
using Unity.MARS;
using Unity.MARS.Forces;
using Unity.MARS.Query;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.MARS.Templates.UI
{
    /// <summary>
    /// Updates a series of UI objects with data based on a given walkthrough, and UI objects
    /// that can control progression of a walkthrough. 
    /// </summary>
    internal class WalkthroughMenu : UIBehaviour, IMatchAcquireHandler
    {
#pragma warning disable 649
        // Main Walkthrough data
        [SerializeField]
        [Tooltip("The walkthrough this menu should display and control.")]
        Walkthrough m_Walkthrough;

        // Window controls
        [SerializeField]
        [Tooltip("Button that sets the walkthrough menu into minimized mode.")]
        Button m_MinimizeButton;

        [SerializeField]
        [Tooltip("Button that restore the walkthrough menu to full size.")]
        Button m_MaximizeButton;

        [SerializeField]
        [Tooltip("Button (HMD Only) that stops the menu from following the user.")]
        Button m_PinButton;

        [SerializeField]
        [Tooltip("The UI that is shown when the walkthrough menu is full size")]
        GameObject m_MainPanel;

        [SerializeField]
        [Tooltip("The UI that is shown when the walkthrough menu is minimized")]
        GameObject m_MinimizedPanel;

        // Navigation controls
        [SerializeField]
        [Tooltip("The UI holds all of the quick navigation controls")]
        RectTransform m_NavPanel;

        [SerializeField]
        [Tooltip("Button that skips back a step in the walkthrough")]
        Button m_PreviousStepButton;

        [SerializeField]
        [Tooltip("Button that progresses a step in the walkthrough")]
        Button m_NextStepButton;

        [SerializeField]
        [Tooltip("Button that brings up the quick navigation menu")]
        Button m_QuicknavButton;

        // Navigation Generation
        [SerializeField]
        [Tooltip("Transform that directly holds all the quick navigation buttons")]
        RectTransform m_NavButtonHolder;

        [SerializeField]
        [Tooltip("Transform that is duplicated to create navigation buttons")]
        RectTransform m_NavButtonPrototype;

        [SerializeField]
        [Tooltip("How far apart vertically (as a function of the button height) to position quick nav buttons")]
        float m_ButtonSpacePercent = .25f;

        // Dynamic walkthrough data
        [SerializeField]
        [Tooltip("Text element that displays the walkthrough title")]
        TMPro.TextMeshProUGUI m_WalkthroughLabel;

        [SerializeField]
        [Tooltip("Text element that displays the step title")]
        TMPro.TextMeshProUGUI m_StepLabel;

        [SerializeField]
        [Tooltip("Text element that displays walkthrough progression")]
        TMPro.TextMeshProUGUI m_StepCountLabel;

        // Flat specific handling
        [SerializeField]
        [Tooltip("The back button for the nav panel in flat mode")]
        Button m_NavPanelBackButton;
        
        [SerializeField]
        [Tooltip("Scroll view to apply rect mask in flat mode")]
        GameObject m_ScrollView;
#pragma warning restore 649

        bool m_WorldMode = false;
        bool m_Pinned = false;

        List<RectTransform> m_NavButtons = new List<RectTransform>();

        protected override void Awake()
        {
            // Hook up all button listeners and walkthrough event callbacks for UI updating
            m_PreviousStepButton.onClick.AddListener(OnPreviousStep);
            m_NextStepButton.onClick.AddListener(OnNextStep);
            m_MinimizeButton.onClick.AddListener(OnMinimizeMainPanel);
            m_MaximizeButton.onClick.AddListener(OnMaximizeMainPanel);
            m_PinButton.onClick.AddListener(OnPin);
            m_QuicknavButton.onClick.AddListener(OnOpenNavPanel);
            m_NavPanelBackButton.onClick.AddListener(OnBackNavPanel);
            m_NavPanelBackButton.gameObject.SetActive(false);
            m_NavPanel.gameObject.SetActive(false);
            m_WalkthroughLabel.SetText(m_Walkthrough.WalkthroughName);

            m_Walkthrough.WalkthroughChangedCallback += UpdateStepUI;

            UpdateStepUI();
            SetWorldMode(m_WorldMode);
            m_NavButtons.Clear();

            // Go through all walkthrough steps and create quick nav buttons for each
            var stepIndex = 0;
            var buttonCount = 0;
            var buttonParent = m_NavButtonPrototype.transform.parent;
            var walkthroughSteps = m_Walkthrough.steps;
            
            foreach (var currentStep in walkthroughSteps)
            {
                // Duplicate the prototype
                var button = Instantiate(m_NavButtonPrototype, buttonParent);
                m_NavButtons.Add(button);

                // Hook up the on-click event to navigation
                var navToIndex = stepIndex;
                var buttonText = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.SetText(walkthroughSteps[stepIndex].Description);
                var toggleComp = button.GetComponent<Toggle>();
                toggleComp.onValueChanged.AddListener((bool toggleState) =>
                {
                    if (toggleState)
                    {
                        m_Walkthrough.SkipToStep(navToIndex);
                    }
                });

                buttonCount++;
                stepIndex++;
            }
            // Hide the prototype since we only want to use the instances
            m_NavButtonPrototype.gameObject.SetActive(false);
            ResizeMenuItems();
        }

        /// <summary>
        /// Enables spatial-mode scripts
        /// </summary>
        /// <param name="worldMode">True if the menu is being displayed in spatial mode, false if as overlay</param>
        public void SetWorldMode(bool worldMode)
        {
            var forcesMainPanel = m_MainPanel.GetComponents<ProxyAlignmentForce>();
            foreach (var force in forcesMainPanel)
            {
                force.enabled = worldMode;
            }
            
            var forcesNavPanel = m_NavPanel.gameObject.GetComponents<ProxyAlignmentForce>();
            foreach (var force in forcesNavPanel)
            {
                force.enabled = worldMode;
            }
            
            var spawnMain = m_MainPanel.GetComponent<SpawnPosition>();
            spawnMain.enabled = worldMode;
            
            var forceMain = m_MainPanel.GetComponent<ForceBehavior>();
            forceMain.enabled = worldMode;
            
            var spawnNav = m_NavPanel.GetComponent<SpawnPosition>();
            spawnNav.enabled = worldMode;
            
            var forceNav = m_NavPanel.GetComponent<ForceBehavior>();
            forceNav.enabled = worldMode;
            
            var spawnMin = m_MinimizedPanel.GetComponent<SpawnPosition>();
            spawnMin.enabled = worldMode;
            
            var forceMin = m_MinimizedPanel.GetComponent<ForceBehavior>();
            forceMin.enabled = worldMode;

            var mask = m_ScrollView.GetComponent<RectMask2D>();
            mask.enabled = !worldMode;

            m_WorldMode = worldMode;
        }


        void ResizeMenuItems()
        {
            if (m_NavButtons.Count == 0)
                return;

            var buttonHeight = m_NavButtons[0].rect.height;
            var buttonSpace = buttonHeight * (1.0f + m_ButtonSpacePercent);
            var totalButtonSpace = buttonSpace * m_NavButtons.Count;

            // Make sure the menu content area can hold all the buttons
            m_NavButtonHolder.sizeDelta = m_NavButtonHolder.sizeDelta + new Vector2(0.0f, totalButtonSpace - m_NavButtonHolder.rect.height);

            var startingPosition = m_NavButtons[0].anchoredPosition;
            startingPosition.y = -(totalButtonSpace * 0.5f) + (buttonSpace * 0.5f);

            // Position each button
            for (var buttonIndex = 0; buttonIndex < m_NavButtons.Count; buttonIndex++)
            {
                var button = m_NavButtons[buttonIndex];
                button.anchoredPosition = startingPosition + new Vector2(0.0f, buttonSpace * buttonIndex);
            }

            UpdateQuickNavUI();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            ResizeMenuItems();
        }

        void OnNextStep()
        {
            m_Walkthrough.SkipToStep(m_Walkthrough.CurrentStep + 1, true);
        }

        void OnPreviousStep()
        {
            m_Walkthrough.SkipToStep(m_Walkthrough.CurrentStep - 1);
        }

        void UpdateStepUI()
        {
            // Conditionally activate the next and previous buttons if there are steps to move to and progression is allowed
            var currentStep = m_Walkthrough.CurrentStep;
            var stepList = m_Walkthrough.steps;

            currentStep = Mathf.Clamp(currentStep, 0, stepList.Length - 1);
            var stepData = stepList[currentStep];

            var nextStepAvailable = currentStep < (stepList.Length - 1) && stepData.CanProgress;
            var prevStepAvailable = currentStep > 0;

            m_NextStepButton.gameObject.SetActive(nextStepAvailable);
            m_PreviousStepButton.gameObject.SetActive(prevStepAvailable);

            m_StepLabel.SetText(stepData.Description);
            m_StepCountLabel.SetText($"Step {currentStep} / {stepList.Length - 1}");

            UpdateQuickNavUI();
        }

        void UpdateQuickNavUI()
        {
            // Highlight the currently activte step
            var currentStep = m_Walkthrough.CurrentStep;
            if (currentStep < m_NavButtons.Count)
            {
                var toggle = m_NavButtons[currentStep].GetComponent<Toggle>();
                toggle.isOn = true;
            }
            // Go through quick nav and activate completion tags on buttons
            var stepList = m_Walkthrough.steps;
            for (var stepCounter = 0; stepCounter < m_NavButtons.Count; stepCounter++)
            {
                var stepData = stepList[stepCounter];
                var buttonPrototype = m_NavButtons[stepCounter].GetComponent<NavButtonPrototype>();
                buttonPrototype.SetComplete(stepData.Completed);
            }
        }
        void OnMinimizeMainPanel()
        {
            if (m_NavPanel.gameObject.activeSelf)
            {
                m_NavPanel.gameObject.SetActive(false);
            }
            m_MainPanel.SetActive(false);
            m_MinimizedPanel.SetActive(true);
        }

        void OnMaximizeMainPanel()
        {
            m_MainPanel.SetActive(true);
            m_MinimizedPanel.SetActive(false);
        }

        void OnPin()
        {
            m_Pinned = !m_Pinned;
        }

        void OnOpenNavPanel()
        {
            m_NavPanel.gameObject.SetActive(!m_NavPanel.gameObject.activeSelf);
            if (!m_WorldMode)
            {
                //hide main panel in flat mode
                m_MainPanel.gameObject.SetActive(!m_NavPanel.gameObject.activeSelf);
                m_QuicknavButton.gameObject.SetActive(false);
                m_NavPanelBackButton.gameObject.SetActive(true);
            }
        }

        void OnBackNavPanel()
        {
            m_MainPanel.SetActive(true);
            m_NavPanel.gameObject.SetActive(false);
            m_QuicknavButton.gameObject.SetActive(true);
            m_NavPanelBackButton.gameObject.SetActive(false);
        }

        public void OnMatchAcquire(QueryResult queryResult)
        {
            queryResult.TryGetTrait(TraitNames.DisplaySpatial, out bool worldMode);
            SetWorldMode(worldMode);
        }
    }
}
#endif
