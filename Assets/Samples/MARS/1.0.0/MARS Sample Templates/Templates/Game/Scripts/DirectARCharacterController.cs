using System;
using UnityEngine;

namespace Unity.MARS.Templates.Game
{
    /// <summary>
    /// Controls a character directly through mouse or touch based input.
    /// The character will move directly to the targeted location.
    /// If the path is obstructed, the character will instead jump
    /// A valid location is any physics collider on the given layer
    /// </summary>
    public class DirectARCharacterController : MonoBehaviour, IAnimationEventActionFinished
    {
        static RaycastHit[] s_RaycastResults = new RaycastHit[10];
        static int k_AnimIDState = Animator.StringToHash("State");
        static int k_AnimIDSpeed = Animator.StringToHash("Speed");
        static int k_AnimIDJumpAngle = Animator.StringToHash("JumpAngle");


        const float k_MaxRayDistance = 20.0f;       // How far we raycast into the distance to find a movement destination
        const float k_MinMoveDistance = 0.1f;       // The threshhold of motion where a straight-up jump is checked
        const float k_JumpTestDistance = 0.3f;      // The step size along the collider path to ensure the path is unobstructed
        const float k_GroundLookHeight = 0.25f;     // Offset from the character's position to look for valid ground
        const float k_GroundLockDistance = 0.75f;   // How far down to look on the ground for a valid position

        const float k_BaseJumpHeight = 0.5f;

        enum State
        {
            OnGround,
            JumpStart,
            Jumping,
            Landing
        }

#pragma warning disable 649
        [SerializeField]
        [Tooltip("The animator belonging to this character")]
        Animator m_Animator;
#pragma warning restore 649

        [SerializeField]
        [Tooltip("How quickly the character reaches full speed")]
        float m_Acceleration = 10.0f;

        [SerializeField]
        [Tooltip("How quickly the character slows to a stop")]
        float m_Deceleration = 10.0f;

        [SerializeField]
        [Tooltip("Character's maximum speed")]
        float m_Speed = 2.0f;

        [SerializeField]
        [Tooltip("Which layers this character considers as valid surfaces")]
        LayerMask m_InteractionLayer = 1 << 29;

        Camera m_Camera;

        State m_State = State.OnGround;

        Vector3 m_GroundTarget = Vector3.zero;

        Vector3 m_JumpStart = Vector3.zero;
        Vector3 m_JumpTarget = Vector3.zero;
        float m_JumpDuration = 0.0f;
        float m_JumpTimer = 0.0f;
        float m_JumpHeight = 0.0f;
        float m_JumpAngle = 0.0f;

        float m_CurrentSpeed = 0.0f;

        
        void Start()
        {
            m_Camera = Camera.main;
            m_GroundTarget = transform.position;
        }

        void Update()
        {
            switch (m_State)
            {
                case State.OnGround:
                    if (m_Animator)
                        m_Animator.speed = 1.0f;

                    // Lock to ground
                    // Find out what surface the object is currently over
                    if (Physics.Raycast(transform.position + (Vector3.up * k_GroundLookHeight), Vector3.down, out var rayCastHit, k_GroundLockDistance, m_InteractionLayer))
                    {
                        transform.position = rayCastHit.point;
                    }

                    // If we just started interacting with the screen this frame, try to find a valid destination from that point
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (!GetTapLocation(Input.mousePosition))
                            break;

                        // See if we can move unobstructed to the destination
                        var currentPosition = transform.position;
                        var destination = s_RaycastResults[0].point;

                        var toDestination = (destination - currentPosition);
                        var distToDest = toDestination.sqrMagnitude;
                        var motionDirection = toDestination.normalized;

                        bool successfulPath = true;

                        while (distToDest > k_MinMoveDistance * k_MinMoveDistance)
                        {
                            currentPosition += motionDirection * k_MinMoveDistance;

                            // Move along ground in direction of motion in min distance steps
                            if (Physics.Raycast(currentPosition + (Vector3.up * k_GroundLookHeight), Vector3.down, out rayCastHit, k_JumpTestDistance, m_InteractionLayer))
                            {
                                currentPosition = rayCastHit.point;
                                toDestination = (destination - currentPosition);
                                distToDest = toDestination.sqrMagnitude;
                            }
                            else
                            {
                                successfulPath = false;
                                break;
                            }

                            // If XZ distance is near 0 but Y still varies, path is not successful either
                            var xzOffset = toDestination;
                            xzOffset.y = 0;
                            if (xzOffset.sqrMagnitude <= k_MinMoveDistance * k_MinMoveDistance && toDestination.y > k_GroundLookHeight)
                            {
                                successfulPath = false;
                                break;
                            }
                        }

                        // If we can't make it, initiate a jump.
                        if (!successfulPath)
                        {
                            m_State = State.JumpStart;
                            m_JumpTarget = destination;
                        }

                        // Otherwise, do nothing and normal motion starts next frame
                    }
                    else
                    {
                        // If we're holding input accelerate to the destination, if we're not, slow down
                        if (Input.GetMouseButton(0))
                        {
                            // Raycast from the touch/mouse point to find what, if anything, we have tapped on
                            m_GroundTarget = GetPlanarTapLocation(Input.mousePosition, transform.position.y);
                            m_CurrentSpeed = Mathf.Clamp(m_CurrentSpeed + (0.5f * m_Acceleration * Time.deltaTime), 0.0f, m_Speed);
                        }
                        else
                        {
                            if (m_CurrentSpeed > 0)
                                m_CurrentSpeed = Mathf.Clamp(m_CurrentSpeed - (0.5f * m_Deceleration * Time.deltaTime), 0.0f, m_Speed);
                        }


                        // Move towards ray hit
                        var toHit = (m_GroundTarget - transform.position);
                        toHit.y = 0;

                        if (toHit.magnitude < k_MinMoveDistance)
                            break;

                        toHit.Normalize();
                        transform.forward = toHit;

                        var newDestination = transform.position + (toHit * Time.deltaTime * m_CurrentSpeed);

                        // Ensure the destination is valid
                        if (Physics.Raycast(newDestination + (Vector3.up * k_GroundLookHeight), Vector3.down, out rayCastHit, k_GroundLockDistance, m_InteractionLayer))
                            transform.position = rayCastHit.point;
                    }

                    break;

                case State.JumpStart:

                    // Allow for target updating until mouse button is released
                    if (Input.GetMouseButton(0))
                    {
                        if (!GetTapLocation(Input.mousePosition))
                            break;

                        m_JumpTarget = s_RaycastResults[0].point;
                    }

                    // Determine the proper jump arc
                    m_JumpStart = transform.position;
                    var toJumpTarget = m_JumpTarget - transform.position;
                    toJumpTarget.y = 0.0f;
                    transform.forward = toJumpTarget.normalized;

                    var jumpDist = (m_JumpStart - m_JumpTarget).magnitude;
                    var heightDifference = Mathf.Abs(m_JumpStart.y - m_JumpTarget.y);
                    m_JumpHeight = heightDifference * 0.5f + k_BaseJumpHeight;

                    m_JumpAngle = Mathf.Atan2(jumpDist * 0.5f, m_JumpHeight) * Mathf.Rad2Deg;

                    // Then move onto jumping
                    if (!Input.GetMouseButton(0))
                    {
                        // Duration is based on airtime, rather than horizontal speed, as this is more physically based and
                        // looks more 'wrong' when it plays too fast or slow
                        var hangTime = Mathf.Sqrt(Mathf.Abs((k_BaseJumpHeight / Physics.gravity.y))) +
                            Mathf.Sqrt(Mathf.Abs(((k_BaseJumpHeight + heightDifference) / Physics.gravity.y)));

                        m_JumpDuration = hangTime;
                        m_JumpTimer = 0.0f;

                        m_State = State.Jumping;
                        if (m_Animator)
                            m_Animator.speed = 1.0f / hangTime;
                    }

                    break;
                case State.Jumping:

                    // Move the character along the jump arc over the duration and manually go to landing
                    m_JumpTimer += Time.deltaTime;
                    var jumpPercent = m_JumpTimer / m_JumpDuration;

                    var jumpPosition = Vector3.Lerp(m_JumpStart, m_JumpTarget, jumpPercent);
                    jumpPosition.y += m_JumpHeight * (1.0f - (2.0f * jumpPercent - 1) * (2.0f * jumpPercent - 1));
                    transform.position = jumpPosition;
                    
                    if (m_JumpTimer > m_JumpDuration)
                    {
                        transform.position = m_JumpTarget;
                        m_State = State.Landing;
                        m_JumpTimer = 0.0f;
                        if (m_Animator)
                            m_Animator.speed = 1.0f;
                    }

                    break;
                case State.Landing:
                    // Wait for animation event if an animator exists, otherwise just go back to idle
                    if (m_Animator)
                        m_Animator.speed = 1.0f;
                    else
                        m_State = State.OnGround;
                    break;
            }
            m_Animator.SetInteger(k_AnimIDState, (int)m_State);
            m_Animator.SetFloat(k_AnimIDSpeed, m_CurrentSpeed);
            m_Animator.SetFloat(k_AnimIDJumpAngle, m_JumpAngle);
        }

        static int CompareRayHitsByDistance(RaycastHit x, RaycastHit y)
        {
            return x.distance.CompareTo(y.distance);
        }

        bool GetTapLocation(Vector3 tapPosition)
        {
            // Raycast from the touch/mouse point to find what, if anything, we have tapped on
            var screenRay = m_Camera.ScreenPointToRay(tapPosition, Camera.MonoOrStereoscopicEye.Mono);

            var foundHits = Physics.RaycastNonAlloc(screenRay, s_RaycastResults, k_MaxRayDistance, m_InteractionLayer);
            var rayHits = foundHits;

            // Don't allow jumping on ourself
            var rayCounter = 0;
            
            while (rayCounter < foundHits)
            {
                if (s_RaycastResults[rayCounter].collider.GetComponentInParent<DirectARCharacterController>() == this)
                {
                    s_RaycastResults[rayCounter].distance = k_MaxRayDistance + 1.0f;
                    rayHits--;
                }
                rayCounter++;
            }

            while (rayCounter < s_RaycastResults.Length)
            {
                s_RaycastResults[rayCounter].distance = k_MaxRayDistance + 1.0f;
                rayCounter++;
            }

            Array.Sort(s_RaycastResults, CompareRayHitsByDistance);

            return (rayHits > 0);
        }

        Vector3 GetPlanarTapLocation(Vector3 tapPosition, float targetHeight)
        {
            var screenRay = m_Camera.ScreenPointToRay(tapPosition, Camera.MonoOrStereoscopicEye.Mono);
            var heightDistance = screenRay.origin.y - targetHeight;
            var timeToHeight = -heightDistance / screenRay.direction.y;

            return screenRay.origin + (screenRay.direction * timeToHeight);
        }

        void IAnimationEventActionFinished.ActionFinished()
        {
            if (m_State != State.Landing)
                return;

            m_GroundTarget = m_JumpTarget;
            m_CurrentSpeed = 0.0f;
            m_State = State.OnGround;
        }
    }
}
