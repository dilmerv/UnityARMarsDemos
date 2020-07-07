#if INCLUDE_MARS
using Unity.MARS;
using Unity.MARS.Data;
using Unity.MARS.Query;
using Unity.MARS.Templates.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.MARS.Templates
{
    /// <summary>
    /// Action that changes button behavior depending if flat or spatial
    /// </summary>
    internal class ResponsiveButtonAction : MonoBehaviour, IMatchAcquireHandler, IMatchLossHandler, IRequiresTraits
    {
        static readonly TraitRequirement[] k_RequiredTraits = { TraitDefinitions.User, new TraitRequirement(TraitDefinitions.DisplaySpatial, false), new TraitRequirement(TraitDefinitions.DisplayFlat, false) };

        public void OnMatchAcquire(QueryResult queryResult)
        {
            
            // Check if the device is flat
            // Then turn off all spatial button behaviors
            queryResult.TryGetTrait(TraitNames.DisplaySpatial, out bool worldMode);

            if (!worldMode)
            {
                var buttonactions = gameObject.GetComponentsInChildren(typeof(ButtonActions), true );
                foreach (ButtonActions button in buttonactions)
                {
                    button.enabled = false;
                }
            }
        }

        public void OnMatchLoss(QueryResult queryResult)
        {
            // Restore the original transform settings
        }

        public TraitRequirement[] GetRequiredTraits()
        {
            return k_RequiredTraits;
        }
    }
}
#endif
