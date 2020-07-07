#if INCLUDE_MARS
using Unity.MARS;
using Unity.MARS.Data;
using Unity.MARS.Forces;
using Unity.MARS.Query;
using UnityEngine;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Action that sets a transform to mirror one of two targets, based on device type
    /// </summary>
    internal class ResponsiveLocalizationAction : MonoBehaviour, IMatchAcquireHandler, IMatchLossHandler, IRequiresTraits, IUsesMarsSceneEvaluation
    {
        static readonly TraitRequirement[] k_RequiredTraits = { TraitDefinitions.User, new TraitRequirement(TraitDefinitions.DisplaySpatial, false), new TraitRequirement(TraitDefinitions.DisplayFlat, false) };

#pragma warning disable 649
        [SerializeField]
        [Tooltip("The localization assets to reparent")]
        GameObject m_Asset;
        
        [SerializeField]
        [Tooltip("The proxy to mirror if the device type is flat")]
        Proxy m_FlatProxy;

        [SerializeField]
        [Tooltip("The proxy to mirror if the device type is spatial")]
        Proxy m_SpatialProxy;
        
        [SerializeField]
        [Tooltip("The proxy to trigger for the first walkthrough step")]
        ProxyFoundTrigger m_ProxyFound;
        
        [SerializeField]
        [Tooltip("The transform to offset if the device type is flat")]
        Transform m_FlatOffset;

        [SerializeField]
        [Tooltip("The transform to offset if the device type is spatial")]
        Transform m_SpatialOffset;
#pragma warning restore 649
        
        Transform m_AssetOriginalParent = null;

        // The original transform values to restore on match loss
        Vector3 m_OriginalLocalPosition;
        Quaternion m_OriginalLocalRotation;
        Vector3 m_OriginalLocalScale;

        public void OnMatchAcquire(QueryResult queryResult)
        {
            // Ensure some kind of mirror can possibly occur, then backup the original canvas values
            if (m_Asset == null || (m_FlatProxy == null && m_SpatialProxy == null))
            {
                Debug.Log("Action will have no result without a transform to manipulate and at least one target");
                return;
            }
            
            if (m_AssetOriginalParent == null)
                m_AssetOriginalParent = m_Asset.transform.parent;
            
            m_OriginalLocalPosition = m_Asset.transform.localPosition;
            m_OriginalLocalRotation = m_Asset.transform.localRotation;
            m_OriginalLocalScale = m_Asset.transform.localScale;

            // Check if the device is spatial, and assume it is flat otherwise
            // Then adjust the original transform to match one of the mirror targets
            queryResult.TryGetTrait(TraitNames.DisplaySpatial, out bool worldMode);
            
            var destination = worldMode ? m_SpatialProxy.transform : m_FlatProxy.transform;

            var assetTransform = m_Asset.GetComponent<Transform>();
            assetTransform.SetParent(destination, false);

            if (worldMode)
            {
                if (m_SpatialProxy != null)
                {
                    m_ProxyFound.ProxyToFind = m_SpatialProxy;
                    m_Asset.transform.localPosition = m_SpatialOffset.localPosition;
                    m_Asset.transform.localRotation = m_SpatialOffset.localRotation;
                    m_Asset.transform.localScale = m_SpatialOffset.localScale;
                }
            }
            else
            {
                if (m_FlatProxy != null)
                {
                    m_ProxyFound.ProxyToFind = m_FlatProxy;
                    m_Asset.transform.localPosition = m_FlatOffset.localPosition;
                    m_Asset.transform.localRotation = m_FlatOffset.localRotation;
                    m_Asset.transform.localScale = m_FlatOffset.localScale;
                }
            }
        }

        public void OnMatchLoss(QueryResult queryResult)
        {
            if (m_Asset == null)
                return;

            var assetTransform = m_Asset.GetComponent<Transform>();
            assetTransform.SetParent(m_AssetOriginalParent, false);
            
            m_Asset.transform.localPosition = m_OriginalLocalPosition;
            m_Asset.transform.localRotation = m_OriginalLocalRotation;
            m_Asset.transform.localScale = m_OriginalLocalScale;
        }

        public TraitRequirement[] GetRequiredTraits()
        {
            return k_RequiredTraits;
        }
    }
}
#endif
