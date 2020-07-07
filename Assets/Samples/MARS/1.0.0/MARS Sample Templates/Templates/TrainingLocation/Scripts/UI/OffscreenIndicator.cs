#if INCLUDE_MARS
using System.Collections.Generic;
using Unity.MARS.Forces;
using UnityEngine;

namespace Unity.MARS.Templates.UI
{
    /// <summary>
    /// Controls visuals and forces to keep an arrow pointing to an offscreen target
    /// </summary>
    internal class OffscreenIndicator : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Objects to hide when the indicator is within the tightest view bounds")]
        List<GameObject> m_HideInView = new List<GameObject>();

#pragma warning disable 649
        [SerializeField]
        [Tooltip("The force aligning this indicator to the camera")]
        ProxyAlignmentForce m_HeadPoseForce;

        [SerializeField]
        [Tooltip("The force aligning this indicator to the target")]
        ProxyAlignmentForce m_AssetForce;

        [SerializeField]
        [Tooltip("The target being tracked")]
        Transform m_Asset;
#pragma warning restore 649

        [SerializeField]
        [Tooltip("The view range at which the indicator gets pulled towards the target")]
        Vector2 m_ForceFOV = new Vector2(-.375f, 1.375f);

        [SerializeField]
        [Tooltip("The view range at which the indicator dissappears")]
        Vector2 m_DisappearFOV = new Vector2(0f, 1f);

        bool m_Fov = false;
        bool m_FovTight = false;

        ProxyAlignmentForceScaling scaleForcesAsset;

        void Start()
        {
            scaleForcesAsset = m_AssetForce.scaleForces;
        }

        void Update()
        {
            // If we're too close to the asset, hide everything
            // If we're close but not that closet to the asset, pulling closer
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(m_Asset.position);
            var inFovTight = screenPoint.z > 0f && screenPoint.x > m_DisappearFOV.x && screenPoint.x < m_DisappearFOV.y && screenPoint.y > m_DisappearFOV.x && screenPoint.y < m_DisappearFOV.y;
            var inFov = screenPoint.z > 0f && screenPoint.x > m_ForceFOV.x && screenPoint.x < m_ForceFOV.y && screenPoint.y > m_ForceFOV.x && screenPoint.y < m_ForceFOV.y;

            if (inFov)
            {
                if (!m_Fov)
                {
                    ScaleForce(1f);
                }
            }
            else
            {
                if (m_Fov)
                {
                    ScaleForce(0f);
                }
            }
            m_Fov = inFov;

            if (inFovTight)
            {
                if (!m_FovTight)
                {
                    transform.localScale = Vector3.zero;
                    transform.position = m_HeadPoseForce.targetProxy.transform.position;

                    foreach (var toHide in m_HideInView)
                    {
                        toHide.SetActive(false);
                    }
                }
            }
            else
            {
                if (m_FovTight)
                {
                    transform.localScale = Vector3.one;
                    transform.position = m_HeadPoseForce.targetProxy.transform.position;

                    foreach (var toHide in m_HideInView)
                    {
                        toHide.SetActive(true);
                    }
                }
            }
            m_FovTight = inFovTight;
        }

        void ScaleForce(float movementScale)
        {
            scaleForcesAsset.movementScale = movementScale;
            m_AssetForce.scaleForces = scaleForcesAsset;
        }
    }
}
#endif
