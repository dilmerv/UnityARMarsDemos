using UnityEngine;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Enables a monobehaviour to react to the 'ActionFinished' animation event
    /// </summary>
    public interface IAnimationEventActionFinished
    {
        void ActionFinished();
    }

    /// <summary>
    /// Calls the 'ActionFinished' function on any supported monobehaviour when the target animation exits
    /// </summary>
    internal class AnimationEventActionFinished : StateMachineBehaviour
    {
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var eventReceiver = animator.GetComponentInParent<IAnimationEventActionFinished>();
            if (eventReceiver != null)
                eventReceiver.ActionFinished();
        }
    }
}
