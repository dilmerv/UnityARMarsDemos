using UnityEngine;

namespace Unity.MARS.Templates.UI
{
    /// <summary>
    /// Helper class that mirrors the starting pose of an object to a target transform
    /// </summary>
    internal class SpawnPosition : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        [Tooltip("The transform to mirror, upon enabling this component.")]
        Transform m_MirrorTarget;
#pragma warning restore 649

        void OnEnable()
        {
            transform.position = m_MirrorTarget.transform.position;
            var facePosition = Camera.main.transform.position;
            var targetRotation = Quaternion.LookRotation(facePosition - transform.position, Vector3.up);
            targetRotation *= Quaternion.Euler(0f, 180f, 0f);
            var targetEuler = targetRotation.eulerAngles;
            var currentEuler = transform.rotation.eulerAngles;
            targetRotation = Quaternion.Euler
            (
                currentEuler.x,
                targetEuler.y,
                currentEuler.z
            );
            transform.rotation = targetRotation;
        }
    }
}
