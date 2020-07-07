#if INCLUDE_MARS
using System.Collections.Generic;
using Unity.MARS;
using Unity.MARS.Query;
using UnityEngine;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Activates or deactivates objects based on the tracking state of the given MARS Object
    /// Useful for activating backup UI in case world elements are not available
    /// </summary>
    [MonoBehaviourComponentMenu(typeof(DependencyAction), "Action/Activate Dependency")]
    internal class DependencyAction : MonoBehaviour, IMatchAcquireHandler, IMatchTimeoutHandler, IMatchLossHandler
    {
        [SerializeField]
        [Tooltip("Which external objects that can only be active once this object is found")]
        List<GameObject> m_Dependents = new List<GameObject>();

        [SerializeField]
        [Tooltip("Which externals objects that should be active only when this object is not found")]
        List<GameObject> m_AntiDependents = new List<GameObject>();

        void Awake()
        {
            ToggleDependents(false);
        }

        public void OnMatchAcquire(QueryResult queryResult)
        {
            ToggleDependents(true);
        }


        public void OnMatchLoss(QueryResult queryResult)
        {
            ToggleDependents(false);
        }

        public void OnMatchTimeout(QueryArgs queryArgs)
        {
            ToggleDependents(false);
        }

        void ToggleDependents(bool state)
        {
            foreach (var dependent in m_Dependents)
            {
                if (dependent != null)
                    dependent.SetActive(state);
            }

            foreach (var antiDependent in m_AntiDependents)
            {
                if (antiDependent != null)
                    antiDependent.SetActive(!state);
            }
        }
    }
}
#endif
