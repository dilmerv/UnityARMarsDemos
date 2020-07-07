#if INCLUDE_MARS
using Unity.MARS;
using Unity.MARS.Data;
using Unity.MARS.Query;
using UnityEngine;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Action that sets a rect transform to mirror one of two targets, based on device type
    /// </summary>
    internal class CopyRectTransformAction : MonoBehaviour, IMatchAcquireHandler, IMatchLossHandler, IRequiresTraits
    {
        static readonly TraitRequirement[] k_RequiredTraits = { TraitDefinitions.User, new TraitRequirement(TraitDefinitions.DisplaySpatial, false), new TraitRequirement(TraitDefinitions.DisplayFlat, false) };

#pragma warning disable 649
        [SerializeField]
        [Tooltip("The transform to modify")]
        RectTransform m_RectTransform;

        [SerializeField]
        [Tooltip("The transform to mirror if the device type is flat")]
        RectTransform m_FlatTarget;

        [SerializeField]
        [Tooltip("The transform to mirror if the device type is spatial")]
        RectTransform m_SpatialTarget;
#pragma warning restore 649

        // The original transform values to restore on match loss
        Vector2 m_OriginalAnchorMin;
        Vector2 m_OriginalAnchorMax;
        Vector2 m_OriginalSizeDelta;
        Vector2 m_OriginalPivot;

        public void OnMatchAcquire(QueryResult queryResult)
        {
            // Ensure some kind of mirror can possibly occur, then backup the original canvas values
            if (m_RectTransform == null || (m_FlatTarget == null && m_SpatialTarget == null))
            {
                Debug.Log("Action will have no result without a transform to manipulate and at least one target");
                return;
            }
            m_OriginalAnchorMin = m_RectTransform.anchorMin;
            m_OriginalAnchorMax = m_RectTransform.anchorMax;
            m_OriginalSizeDelta = m_RectTransform.sizeDelta;
            m_OriginalPivot = m_RectTransform.pivot;

            // Check if the device is spatial, and assume it is flat otherwise
            // Then adjust the original transform to match one of the mirror targets
            queryResult.TryGetTrait(TraitNames.DisplaySpatial, out bool worldMode);

            if (worldMode)
            {
                if (m_SpatialTarget != null)
                {
                    m_RectTransform.anchorMin = m_SpatialTarget.anchorMin;
                    m_RectTransform.anchorMax = m_SpatialTarget.anchorMax;
                    m_RectTransform.sizeDelta = m_SpatialTarget.sizeDelta;
                    m_RectTransform.pivot = m_SpatialTarget.pivot;
                    m_RectTransform.position = m_SpatialTarget.position;
                }
            }
            else
            {
                if (m_FlatTarget != null)
                {
                    m_RectTransform.anchorMin = m_FlatTarget.anchorMin;
                    m_RectTransform.anchorMax = m_FlatTarget.anchorMax;
                    m_RectTransform.sizeDelta = m_FlatTarget.sizeDelta;
                    m_RectTransform.pivot = m_FlatTarget.pivot;
                    m_RectTransform.position = m_FlatTarget.position;
                }
            }
        }

        public void OnMatchLoss(QueryResult queryResult)
        {
            // Restore the original transform settings
            m_RectTransform.anchorMin = m_OriginalAnchorMin;
            m_RectTransform.anchorMax = m_OriginalAnchorMax;
            m_RectTransform.sizeDelta = m_OriginalSizeDelta;
            m_RectTransform.pivot = m_OriginalPivot;
        }

        public TraitRequirement[] GetRequiredTraits()
        {
            return k_RequiredTraits;
        }
    }
}
#endif
