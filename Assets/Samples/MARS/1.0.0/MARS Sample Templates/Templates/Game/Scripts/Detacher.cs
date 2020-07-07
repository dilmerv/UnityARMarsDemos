#if INCLUDE_MARS
using Unity.MARS;
using Unity.MARS.Query;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Helper script for AR objects that should spawn on a proxy but then move freely around the world
    /// It does this by removing it from the transform hierarchy
    /// </summary>
    internal class Detacher : MonoBehaviour, IMatchAcquireHandler
    {
        /// <summary>
        /// What should do to the root object upon detachment?
        /// </summary>
        enum RootAction
        {
            /// <summary> Leave the root object alone </summary>
            None = 0,
            /// <summary> Disable the root object </summary>
            Disable,
            /// <summary> Destroy the root object </summary>
            Destroy
        }

#pragma warning disable 649
        [SerializeField]
        [Tooltip("The object to remove from the transform hierarchy.")]
        Transform m_ToDetach;

        [SerializeField]
        [Tooltip("The root object at the top of the transform hierarchy to modify.")]
        GameObject m_OldRoot;

#pragma warning restore 649

        [SerializeField]
        [Tooltip("What operation to perform on the root obejct after the target is detached.")]
        RootAction m_RootAction = RootAction.Disable;

        bool m_Detached = false;

        public void OnMatchAcquire(QueryResult queryResult)
        {
            Detach();
        }

        void Detach()
        {
            if (m_ToDetach == null)
                return;

            Transform toDetachParent = null;
#if UNITY_EDITOR
            var simContentRoot = EditorOnlyDelegates.GetSimulatedObjectsRoot?.Invoke();
            if (simContentRoot != null && m_ToDetach.IsChildOf(simContentRoot.transform))
                toDetachParent = simContentRoot.transform;
#endif

            // Detach the target
            m_ToDetach.parent = toDetachParent;

            m_Detached = true;
        }

        void Update()
        {
            if (!m_Detached)
                return;

            enabled = false;

            // If the root exists, optionally destroy or disable it
            if (m_OldRoot != null)
            {
                switch (m_RootAction)
                {
                    case RootAction.Disable:
                        m_OldRoot.SetActive(false);
                        break;
                    case RootAction.Destroy:
                        UnityObjectUtils.Destroy(m_OldRoot);
                        break;
                }
            }
        }
    }
}
#endif
