#if INCLUDE_MARS
using Unity.MARS;
using Unity.MARS.Data;
using Unity.MARS.Query;
using UnityEngine;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Action that reparents a canvas to new RectTransform, based on device type
    /// </summary>
    internal class ReparentCanvasAction : MonoBehaviour, IMatchAcquireHandler, IMatchLossHandler, IRequiresTraits
    {
        static readonly TraitRequirement[] k_RequiredTraits = { TraitDefinitions.User, new TraitRequirement(TraitDefinitions.DisplaySpatial, false), new TraitRequirement(TraitDefinitions.DisplayFlat, false) };

#pragma warning disable 649
        [SerializeField]
        [Tooltip("The canvas to reparent")]
        Canvas m_CanvasToDraw;

        [SerializeField]
        [Tooltip("If set, will reparent the canvas to this target in flat mode")]
        RectTransform m_FlatTarget;

        [SerializeField]
        [Tooltip("If set, will reparent the canvas to this target in spatial mode")]
        RectTransform m_SpatialTarget;
#pragma warning restore 649

        Transform m_CanvasOriginalParent = null;

        public void OnMatchAcquire(QueryResult queryResult)
        {
            // Ensure some kind of reparenting can possibly occur, then backup the original parent
            if (m_CanvasToDraw == null || (m_FlatTarget == null && m_SpatialTarget == null))
            {
                Debug.Log("Action will have no result without a canvas to draw and at least one target");
                return;
            }

            if (m_CanvasOriginalParent == null)
                m_CanvasOriginalParent = m_CanvasToDraw.transform.parent;


            queryResult.TryGetTrait(TraitNames.DisplaySpatial, out bool worldMode);

            var destination = worldMode ? m_SpatialTarget : m_FlatTarget;

            var rectTransform = m_CanvasToDraw.GetComponent<RectTransform>();
            rectTransform.SetParent(destination, false);
        }

        public void OnMatchLoss(QueryResult queryResult)
        {
            if (m_CanvasToDraw == null)
                return;

            var rectTransform = m_CanvasToDraw.GetComponent<RectTransform>();
            rectTransform.SetParent(m_CanvasOriginalParent, false);
        }

        public TraitRequirement[] GetRequiredTraits()
        {
            return k_RequiredTraits;
        }
    }
}
#endif
