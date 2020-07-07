#if INCLUDE_MARS
using Unity.MARS;
using Unity.MARS.Forces;
using UnityEngine;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Adjusts how much an object is moved by forces, based on whether or not is seen by the user
    /// </summary>
    internal class ForceBehavior : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        [Tooltip("The object that should be affected by being in view.")]
        ProxyAlignmentForce m_AssetForce;
#pragma warning restore 649

        [SerializeField]
        [Tooltip("The field of view to use to determine if this object is in view")]
        Vector2 m_ForceFOV = new Vector2(0.25f, 0.75f);

        [SerializeField]
        [Tooltip("Multiplier to set force motion scale to, when in view.")]
        float m_InFOVForce = 0.1f;

        ProxyAlignmentForceScaling scaleForce;
        Camera m_Camera;

        void Start()
        {
            scaleForce = m_AssetForce.scaleForces;
            m_Camera = MarsRuntimeUtils.GetActiveCamera(true);
        }

        void Update()
        {
            Vector3 screenPoint = m_Camera.WorldToViewportPoint(transform.position);
            var dist = Vector3.Distance(transform.position, m_Camera.transform.position);
            var inFov = screenPoint.z > 0f && screenPoint.x > m_ForceFOV.x && screenPoint.x < m_ForceFOV.y && screenPoint.y > m_ForceFOV.x && screenPoint.y < m_ForceFOV.y;

            if (inFov && dist < 1.5f)
            {
                ScaleForce(m_InFOVForce);
            }
            else
            {
                ScaleForce(1f);
            }
        }

        void ScaleForce(float movementScale)
        {
            scaleForce.movementScale = movementScale;
            m_AssetForce.scaleForces = scaleForce;
        }
    }
}
#endif
