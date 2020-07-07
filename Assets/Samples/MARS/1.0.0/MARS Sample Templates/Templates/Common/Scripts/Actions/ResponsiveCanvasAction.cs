#if INCLUDE_MARS
using Unity.MARS;
using Unity.MARS.Data;
using Unity.MARS.Query;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Action that adjusts a canvas between world space rendering or screen rendering, based on device type
    /// </summary>
    internal class ResponsiveCanvasAction : MonoBehaviour, IMatchAcquireHandler, IMatchLossHandler, IRequiresTraits
    {
        static readonly TraitRequirement[] k_RequiredTraits = { TraitDefinitions.User, new TraitRequirement(TraitDefinitions.DisplaySpatial, false), new TraitRequirement(TraitDefinitions.DisplayFlat, false) };

#pragma warning disable 649
        [SerializeField]
        [Tooltip("The canvas to set between world or overlay rendering modes")]
        Canvas m_CanvasTarget;

        [SerializeField]
        [Tooltip("The canvas scalar mode")]
        ScaleMode m_ScaleMode;
#pragma warning restore 649
        [SerializeField]
        [Tooltip("The reference resolution for screen size mode")]
        Vector2 m_RefResolution = new Vector2(360f, 1000f);
        
        public enum ScaleMode
        {
            PhysicalSize, ScreenSize
        }

        CanvasScaler m_CanvasScaler;
        bool m_WorldMode = true;

        Vector3 m_SavedPosition;
        Vector3 m_SavedScale;
        Vector2 m_SavedAnchorMin;
        Vector2 m_SavedAnchorMax;
        Vector2 m_SavedAnchorPosition;
        Vector2 m_SavedSizeDelta;

        public void OnMatchAcquire(QueryResult queryResult)
        {
            if (m_CanvasTarget == null)
            {
                Debug.Log("Action will have no result without a canvas to adapt");
                return;
            }

            m_CanvasScaler = m_CanvasTarget.GetComponent<CanvasScaler>();

            if (m_CanvasScaler == null)
            {
                Debug.Log("Action will not work correctly without a canvas scaler");
                return;
            }

            queryResult.TryGetTrait(TraitNames.DisplaySpatial, out bool newWorldMode);
            SwitchMode(newWorldMode);
        }

        public void OnMatchLoss(QueryResult queryResult)
        {
            if (m_CanvasTarget == null || m_CanvasScaler == null)
                return;

            SwitchMode(true);
        }

        public TraitRequirement[] GetRequiredTraits()
        {
            return k_RequiredTraits;
        }

        void SwitchMode(bool worldMode)
        {
            if (worldMode == m_WorldMode)
                return;

            // If we're switching to spatial rendering, switch to in-world rendering, and adjust the pixel sizes to scale properly
            // Restore the canvas to the saved sizes, as responsive canvases are authored in world space.
            // In flat mode, we switch to overlay and set the reference pixels higher to keep text and 9-slices looking similar
            m_WorldMode = worldMode;

            var canvasTransform = m_CanvasTarget.GetComponent<RectTransform>();

            if (m_WorldMode)
            {
                m_CanvasTarget.renderMode = RenderMode.WorldSpace;
                m_CanvasTarget.scaleFactor = 1.0f;
                m_CanvasScaler.referencePixelsPerUnit = 1.0f;

                canvasTransform.position = m_SavedPosition;
                canvasTransform.localScale = m_SavedScale;
                canvasTransform.anchorMin = m_SavedAnchorMin;
                canvasTransform.anchorMax = m_SavedAnchorMax;
                canvasTransform.anchoredPosition = m_SavedAnchorPosition;
                canvasTransform.sizeDelta = m_SavedSizeDelta;
            }
            else
            {
                m_SavedPosition = canvasTransform.position;
                m_SavedScale = canvasTransform.localScale;
                m_SavedAnchorMin = canvasTransform.anchorMin;
                m_SavedAnchorMax = canvasTransform.anchorMax;
                m_SavedAnchorPosition = canvasTransform.anchoredPosition;
                m_SavedSizeDelta = canvasTransform.sizeDelta;

                m_CanvasTarget.renderMode = RenderMode.ScreenSpaceOverlay;
                if (m_ScaleMode == ScaleMode.ScreenSize)
                {
                    m_CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    m_CanvasScaler.referenceResolution = m_RefResolution;
                    m_CanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                }
                else if (m_ScaleMode == ScaleMode.PhysicalSize) 
                {
                    m_CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPhysicalSize;
                    m_CanvasScaler.referencePixelsPerUnit = 100.0f;
                }
            }
        }
    }
}
#endif
