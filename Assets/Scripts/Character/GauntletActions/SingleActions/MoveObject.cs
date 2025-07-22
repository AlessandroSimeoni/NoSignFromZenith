using UnityEngine;

namespace GauntletActions
{
    public class MoveObject : GauntletAction
    {
        [Header("Distance options")]
        [SerializeField, Tooltip("The variation of the object distance with a single mouse scroll")] 
        private float deltaDistanceModifier = 0.25f;
        [SerializeField, Tooltip("The minimum reachable distance from the camera")] 
        private float minDistance = 2.0f;
        [SerializeField, Tooltip("The maximum reachable distance from the camera")] 
        private float maxDistance = 10.0f;

        public MoveObject() : base(ActionIDs.MOVE_OBJECT) { }

        public override void PerformAction(GauntletAction dependantAction = null, float value = 0.0f)
        {
            PickUpObject pickUpAction = (PickUpObject)dependantAction;

            // update the object distance
            float newDistance = pickUpAction.currentObjectDistance + deltaDistanceModifier * value;
            pickUpAction.currentObjectDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);
        }
    }
}
