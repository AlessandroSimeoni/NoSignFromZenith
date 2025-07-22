using UnityEngine;

namespace GauntletActions
{
    public abstract class GauntletAction : MonoBehaviour
    {
        // each gauntlet action has a specific id
        public ActionIDs actionID { get; private set; }

        public GauntletAction(ActionIDs id)
        {
            actionID = id;
        }

        /// <summary>
        /// Each gauntlet action will define its logic in this method.
        /// If the gauntlet action depends on another action, it can be passed as a parameter
        /// </summary>
        /// <param name="dependantAction">The dependant action if any</param>
        /// <param name="value">The value needed by the action if any</param>
        public abstract void PerformAction(GauntletAction dependantAction = null, float value = 0.0f);
    }
}
