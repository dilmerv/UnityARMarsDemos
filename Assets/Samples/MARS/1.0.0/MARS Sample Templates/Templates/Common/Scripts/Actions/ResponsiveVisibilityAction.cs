#if INCLUDE_MARS
using System.Collections.Generic;
using Unity.MARS;
using Unity.MARS.Data;
using Unity.MARS.Query;
using UnityEngine;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Action that activates or deactives specific objects based on device type
    /// </summary>
    internal class ResponsiveVisibilityAction : MonoBehaviour, IRequiresTraits, IMatchVisibilityHandler
    {
        static readonly TraitRequirement[] k_RequiredTraits = { TraitDefinitions.User, new TraitRequirement(TraitDefinitions.DisplaySpatial, false), new TraitRequirement(TraitDefinitions.DisplayFlat, false) };

#pragma warning disable 649
        [SerializeField]
        [Tooltip("Objects to display on flat devices, like mobile phones and tablets")]
        List<GameObject> m_FlatOnlyObjects;

        [SerializeField]
        [Tooltip("Objects to display on spatial devices such as HMDs")]
        List<GameObject> m_SpatialOnlyObjects;
#pragma warning restore 649

        bool m_Initialized = false;
        bool m_LastMode = false;

        public void FilterVisibleObjects(QueryState newState, QueryResult queryResult, List<GameObject> objectsToActivate, List<GameObject> objectsToDeactivate)
        {
            switch (newState)
            {
                case QueryState.Acquiring:
                    queryResult.TryGetTrait(TraitNames.DisplaySpatial, out bool worldMode);

                    if (m_Initialized)
                    {
                        if (m_LastMode == worldMode)
                            return;
                        else
                            objectsToActivate.AddRange(m_LastMode ? m_SpatialOnlyObjects : m_FlatOnlyObjects);
                    }
                    m_Initialized = true;
                    m_LastMode = worldMode;

                    objectsToDeactivate.AddRange(m_LastMode ? m_FlatOnlyObjects : m_SpatialOnlyObjects);
                    break;

                case QueryState.Resuming:
                case QueryState.Unavailable:
                case QueryState.Unknown:

                    if (!m_Initialized)
                        return;
                    m_Initialized = false;
                    objectsToActivate.AddRange(m_LastMode ? m_SpatialOnlyObjects : m_FlatOnlyObjects);

                    break;
            }
        }

        public TraitRequirement[] GetRequiredTraits()
        {
            return k_RequiredTraits;
        }

    }
}
#endif
