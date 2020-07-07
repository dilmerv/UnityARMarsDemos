using UnityEngine;

namespace Unity.MARS.Templates.Game
{
    /// <summary>
    /// Simple animation/game logic script for the crystal collectable
    /// </summary>
    internal class CrystalController : MonoBehaviour
    {
        const float k_CollectionDuration = 0.75f;
        const float k_RotationSpeed = 0.5f;

        [SerializeField]
        [Tooltip("The mesh renderer associated with this crystal, used for overriding shader properties")]
        Renderer m_Renderer = null;

        [SerializeField]
        [Tooltip("The particle effect to play when the crystal is collected")]
        GameObject m_CollectEffect = null;

        float m_CurrentRotation = 0.0f;

        bool m_Collected = false;
        float m_CollectionTimer = 0.0f;
        Vector3 m_StartScale = Vector3.one;
        Vector3 m_CollectedScale = Vector3.one;

        MaterialPropertyBlock m_ColorPropertyBlock;

        private void Start()
        {
            m_ColorPropertyBlock = new MaterialPropertyBlock();
        }

        void Update()
        {
            // If not collected, just rotate slowly
            if (!m_Collected)
            {
                m_CurrentRotation += Time.deltaTime * k_RotationSpeed * 180.0f;
                transform.localRotation = Quaternion.Euler(0.0f, m_CurrentRotation, 0.0f);
            }
            else
            {
                // Once collect, spin faster and faster, while shrinking to a beam of light
                m_CollectionTimer += Time.deltaTime;
                var collectionPercent = m_CollectionTimer / k_CollectionDuration;

                // Speed up
                m_CurrentRotation += Time.deltaTime * k_RotationSpeed * 180.0f * Mathf.Lerp(1.0f, 16.0f, collectionPercent);

                // Scale up
                transform.localScale = Vector3.Lerp(m_StartScale, m_CollectedScale, collectionPercent);

                // Color up
                m_ColorPropertyBlock.SetColor("_EmissionColor", Color.Lerp(new Color(.64f,.64f,.64f,1.0f), new Color(4.0f, 4.0f, 4.0f), collectionPercent));
                m_Renderer.SetPropertyBlock(m_ColorPropertyBlock);
                transform.localRotation = Quaternion.Euler(0.0f, m_CurrentRotation, 0.0f);

                if (collectionPercent >= 1.0f)
                {
                    Destroy(gameObject);
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (m_Collected)
                return;

            if (other.GetComponentInParent<DirectARCharacterController>())
            {
                // Collect!
                m_Collected = true;
                m_StartScale = transform.localScale;
                m_CollectedScale = m_StartScale;
                m_CollectedScale.x = 0;
                m_CollectedScale.z = 0;
                m_CollectedScale.y *= 1.75f;

                if (m_CollectEffect != null)
                    m_CollectEffect.SetActive(true);
            }
        }
    }
}
