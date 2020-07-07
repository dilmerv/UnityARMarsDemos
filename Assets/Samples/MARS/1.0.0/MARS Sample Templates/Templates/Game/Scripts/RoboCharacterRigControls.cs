using UnityEngine;

namespace Unity.MARS.Templates.Game
{
    /// <summary>
    /// Programmatic rig controls for the Unit-E robot character.
    /// This is a legacy script - it is now recommended to use the
    /// Unity Animation Rigging Package for this kind of skeletal controls
    /// </summary>
    internal class RoboCharacterRigControls : MonoBehaviour
    {
        const float k_RotationToSpeed = Mathf.Rad2Deg;
        const float k_HappyUpperOffset = 0.02414652f;   // The maximum height of the upper eyelid when the robot is happy
        const float k_AngryUpperOffset = 0.01064652f;   // The maximum height of the upper eyelid when the robot is angry
        const float k_HappyLowerOffset = -0.03181273f;  // The minimum height of the lower eyelid when the robot is happy
        const float k_AngryLowerOffset = -0.02431273f;  // The minimum height of the lower eyelid when the robot is angry
        const float k_ShoulderOutOffset = 0.07819504f;  // How much the shoulders extend from the base when released
        const float k_FingerDropDistance = 0.02f;       // How much to extend fingers when the hand is open
        const float k_FingerAngle = 30.0f;              // How much to curl fingers back when the hand is open
        const float k_FingerSpreadAngle = 10.0f;        // How much to spread the fingers out when the hand is open

#pragma warning disable 649
        [SerializeField]
        Transform m_PelvisControl;
        [SerializeField]
        Transform m_PelvisCenter;
        [SerializeField]
        Transform m_TorsoControl;

        [SerializeField]
        Transform m_LeftWheel;
        [SerializeField]
        Transform m_RightWheel;

        [SerializeField]
        Transform m_EyeLids;
        [SerializeField]
        Transform m_EyelidUpper;
        [SerializeField]
        Transform m_EyelidLower;

        [SerializeField]
        Transform m_LeftShoulder;
        [SerializeField]
        Transform m_RightShoulder;
        [SerializeField]
        Transform m_LeftHand;
        [SerializeField]
        Transform m_RightHand;

        [SerializeField]
        Transform m_LeftFinger1;
        [SerializeField]
        Transform m_LeftFinger2;
        [SerializeField]
        Transform m_LeftThumb;

        [SerializeField]
        Transform m_RightFinger1;
        [SerializeField]
        Transform m_RightFinger2;
        [SerializeField]
        Transform m_RightThumb;

        [SerializeField]
        float m_Speed = 0.0f;
        [SerializeField]
        float m_Tilt = 0.0f;

        [SerializeField]
        float m_EyeTilt = 0.0f;
        [SerializeField]
        float m_EyeOpen = 0.0f;
        [SerializeField]
        float m_Mood = 0.5f;

        [SerializeField]
        float m_ArmInOut = 0.0f;
        [SerializeField]
        float m_ShoulderUpDown = 0.0f;
        [SerializeField]
        float m_ArmsForwardBack = 0.0f;
        [SerializeField]
        float m_HandsForwardBack = 0.0f;
        [SerializeField]
        float m_HandsCloseOpen = 0.0f;
        [SerializeField]
        float m_ArmUpDown = 0.0f;
#pragma warning restore 649

        float m_CurrentWheelAngle = 0.0f;

        Vector3 m_UpperEyelidClosed = Vector3.zero;
        Vector3 m_LowerEyelidClosed = Vector3.zero;

        Vector3 m_TorsoOffset = Vector3.zero;
        Vector3 m_PelvisOffset = Vector3.zero;

        Vector3 m_LeftShoulderClosed = Vector3.zero;
        Vector3 m_RightShoulderClosed = Vector3.zero;

        Vector3 m_LeftFinger1Base = Vector3.zero;
        Vector3 m_LeftFinger2Base = Vector3.zero;
        Vector3 m_LeftThumbBase = Vector3.zero;

        Vector3 m_RightFinger1Base = Vector3.zero;
        Vector3 m_RightFinger2Base = Vector3.zero;
        Vector3 m_RightThumbBase = Vector3.zero;


        public float Speed
        {
            get { return m_Speed; }
            set { m_Speed = value; }
        }

        public float Tilt
        {
            get { return m_Tilt; }
            set { m_Tilt = value; }
        }

        public float EyeTilt
        {
            get { return m_EyeTilt; }
            set { m_EyeTilt = value; }
        }

        public float EyeOpen
        {
            get { return m_EyeOpen; }
            set { m_EyeOpen = value; }
        }

        public float Mood
        {
            get { return m_Mood; }
            set { m_Mood = value; }
        }

        public float ArmInOut
        {
            get { return m_ArmInOut; }
            set { m_ArmInOut = value; }
        }

        public float ShoulderUpDown
        {
            get { return m_ShoulderUpDown; }
            set { m_ShoulderUpDown = value; }
        }

        public float ArmsForwardBack
        {
            get { return m_ArmsForwardBack; }
            set { m_ArmsForwardBack = value; }
        }

        public float HandsForwardBack
        {
            get { return m_HandsForwardBack; }
            set { m_HandsForwardBack = value; }
        }

        public float HandsCloseOpen
        {
            get { return m_HandsCloseOpen; }
            set { m_HandsCloseOpen = value; }
        }

        public float ArmUpDown
        {
            get { return m_ArmUpDown; }
            set { m_ArmUpDown = value; }
        }

        void Awake()
        {
            // Initialize default control point positions while the robot is in the bind pose
            m_TorsoOffset = m_PelvisControl.InverseTransformPoint(m_TorsoControl.position);
            m_PelvisOffset = transform.InverseTransformPoint(m_PelvisCenter.position);
            m_UpperEyelidClosed = m_EyelidUpper.localPosition;
            m_LowerEyelidClosed = m_EyelidLower.localPosition;
            m_LeftShoulderClosed = m_LeftShoulder.localPosition;
            m_RightShoulderClosed = m_RightShoulder.localPosition;

            m_LeftFinger1Base = m_LeftFinger1.localPosition;
            m_LeftFinger2Base = m_LeftFinger2.localPosition;
            m_LeftThumbBase = m_LeftThumb.localPosition;

            m_RightFinger1Base = m_RightFinger1.localPosition;
            m_RightFinger2Base = m_RightFinger2.localPosition;
            m_RightThumbBase = m_RightThumb.localPosition;
        }

        void Update()
        {
            m_CurrentWheelAngle += Time.deltaTime * m_Speed * k_RotationToSpeed;
            var wheelRotation = Quaternion.Euler(m_CurrentWheelAngle, 0.0f, 0.0f);

            m_LeftWheel.localRotation = wheelRotation;
            m_RightWheel.localRotation = wheelRotation;

            // Rotate the pelvis
            var bodyRotation = Quaternion.Euler(m_Tilt, 0.0f, 0.0f);
            m_PelvisControl.localRotation = bodyRotation;

            // Recenter around the wheel base
            var centerOffset = m_PelvisOffset - transform.InverseTransformPoint(m_PelvisCenter.position);
            m_PelvisControl.localPosition = m_PelvisControl.localPosition + centerOffset;

            m_TorsoControl.localRotation = bodyRotation;
            m_TorsoControl.position = m_PelvisControl.TransformPoint(m_TorsoOffset);

            // Rotate eyes
            m_EyeLids.localRotation = Quaternion.Euler(0.0f, 0.0f, m_EyeTilt);

            // Figure out upper and lower bounds from mood
            var localUpper = Mathf.Lerp(k_AngryUpperOffset, k_HappyUpperOffset, m_Mood);
            var localLower = Mathf.Lerp(k_AngryLowerOffset, k_HappyLowerOffset, m_Mood);

            // Apply eye position
            m_EyelidUpper.localPosition = m_UpperEyelidClosed + new Vector3(0.0f, localUpper * m_EyeOpen, 0.0f);
            m_EyelidLower.localPosition = m_LowerEyelidClosed + new Vector3(0.0f, localLower * m_EyeOpen, 0.0f);

            // Shoulder level work
            // Move arms in/out by raw value
            var inOutClamped = Mathf.Clamp01(m_ArmInOut);

            // Rotate the shoulders up and down, gated by in/out
            var shoulderRotation = m_ShoulderUpDown * Mathf.Rad2Deg * inOutClamped;

            // Rotate the arms back and forth through the shoulders as well
            var armRotation = (m_ArmsForwardBack * inOutClamped) * 0.5f * Mathf.Rad2Deg;
            m_LeftShoulder.localRotation = Quaternion.Euler(0, -armRotation, shoulderRotation);
            m_RightShoulder.localRotation = Quaternion.Euler(-180.0f, armRotation, shoulderRotation);

            // The pivot point of the arm is floating - always needing to be at the edge of the robot's size
            m_LeftShoulder.localPosition = m_LeftShoulderClosed + new Vector3(-k_ShoulderOutOffset * inOutClamped, 0.0f, 0.0f);
            m_RightShoulder.localPosition = m_RightShoulderClosed + new Vector3(k_ShoulderOutOffset * inOutClamped, 0.0f, 0.0f);

            var handRotation = (m_HandsForwardBack * inOutClamped) * 0.5f * Mathf.Rad2Deg;

            // Forearm Up/ Down
            var armUpDownRotation = m_ArmUpDown * inOutClamped * Mathf.Rad2Deg;

            // Arms forward/ back
            m_LeftHand.localRotation = Quaternion.Euler(handRotation - 90.0f, 0.0f, 0.0f) * Quaternion.Euler(0.0f, -armUpDownRotation, 0.0f);
            m_RightHand.localRotation = Quaternion.Euler(handRotation, 0.0f, armUpDownRotation);

            // Hands open/ close
            var handsValue = Mathf.Clamp01(m_HandsCloseOpen);
            var localFingerDrop = new Vector3(0.0f, 0.0f, -handsValue * k_FingerDropDistance);
            m_LeftFinger1.localPosition = m_LeftFinger1Base + localFingerDrop;
            m_LeftFinger2.localPosition = m_LeftFinger2Base + localFingerDrop;
            m_LeftThumb.localPosition = m_LeftThumbBase + localFingerDrop;
            m_LeftFinger1.localRotation = Quaternion.Euler(90.0f - k_FingerAngle * handsValue, 0.0f, -k_FingerSpreadAngle * handsValue);
            m_LeftFinger2.localRotation = Quaternion.Euler(90.0f - k_FingerAngle * handsValue, 0.0f, k_FingerSpreadAngle * handsValue);
            m_LeftThumb.localRotation = Quaternion.Euler(90.0f + k_FingerAngle * handsValue, 0.0f, 0.0f);

            localFingerDrop = new Vector3(0.0f, -handsValue * k_FingerDropDistance, 0.0f);
            m_RightFinger1.localPosition = m_RightFinger1Base + localFingerDrop;
            m_RightFinger2.localPosition = m_RightFinger2Base + localFingerDrop;
            m_RightThumb.localPosition = m_RightThumbBase + localFingerDrop;
            m_RightFinger1.localRotation = Quaternion.Euler(-k_FingerAngle * handsValue, 0.0f, -k_FingerSpreadAngle * handsValue);
            m_RightFinger2.localRotation = Quaternion.Euler(-k_FingerAngle * handsValue, 0.0f, k_FingerSpreadAngle * handsValue);
            m_RightThumb.localRotation = Quaternion.Euler(k_FingerAngle * handsValue, 0.0f, 0.0f);

        }
    }
}
