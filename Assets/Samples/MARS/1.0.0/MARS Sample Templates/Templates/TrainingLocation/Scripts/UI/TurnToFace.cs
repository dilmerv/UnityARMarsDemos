using UnityEngine;

namespace Unity.MARS.Templates.UI
{
    /// <summary>
    /// Makes the object this is attached to face a target with a slight delay
    /// </summary>
    internal class TurnToFace : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        [Tooltip("The object to face.")]
        Transform m_FaceTarget;

        [SerializeField]
        [Tooltip("Whether or not to allow turning on the x-axis")]
        bool m_IgnoreX;

        [SerializeField]
        [Tooltip("Whether or not to allow turning on the y-axis")]
        bool m_IgnoreY;

        [SerializeField]
        [Tooltip("Whether or not to allow turning on the z-axis")]
        bool m_IgnoreZ;
#pragma warning restore 649

        [SerializeField]
        [Tooltip("How quickly to turn to ensure facing the target")]
        float m_TurnToFaceSpeed = 5f;

        [SerializeField]
        [Tooltip("How much to adjust the default facing orientation")]
        Vector3 m_RotationOffset = Vector3.zero;


        void Awake()
        {
            // Default to main camera
            if (m_FaceTarget == null)
                if (Camera.main != null)
                    m_FaceTarget = Camera.main.transform;
        }

        void Update()
        {
            if (m_FaceTarget != null)
            {
                var facePosition = m_FaceTarget.position;
                var targetRotation = Quaternion.LookRotation(facePosition - transform.position, Vector3.up);
                targetRotation *= Quaternion.Euler(m_RotationOffset);
                if (m_IgnoreX || m_IgnoreY || m_IgnoreZ)
                {
                    var targetEuler = targetRotation.eulerAngles;
                    var currentEuler = transform.rotation.eulerAngles;
                    targetRotation = Quaternion.Euler
                    (
                        m_IgnoreX ? currentEuler.x : targetEuler.x,
                        m_IgnoreY ? currentEuler.y : targetEuler.y,
                        m_IgnoreZ ? currentEuler.z : targetEuler.z
                        );
                }

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_TurnToFaceSpeed * Time.deltaTime);
            }
        }
    }
}
