using UnityEngine;

namespace Unity.MARS.Templates.UI
{
    /// <summary>
    /// Holds references to important parts of a navigation button prefab, for use when cloning the object
    /// </summary>
    internal class NavButtonPrototype : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        [Tooltip("The object that is toggled when a step associated with a nav button is complete.")]
        GameObject m_CompleteMarker;
#pragma warning restore 649

        /// <summary>
        /// Toggles the associated completion marker object
        /// </summary>
        /// <param name="complete">Whether or not the complete marker should be active</param>
        public void SetComplete(bool complete)
        {
            m_CompleteMarker.SetActive(complete);
        }
    }
}
