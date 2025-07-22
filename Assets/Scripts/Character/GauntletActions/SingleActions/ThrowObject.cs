using Audio;
using UnityEngine;

namespace GauntletActions
{
    public class ThrowObject : GauntletAction
    {
        [SerializeField] private float throwingForce = 10.0f;
        [SerializeField] private AudioClip pushSFX;

        public ThrowObject() : base(ActionIDs.THROW_OBJECT) { }

        public override void PerformAction(GauntletAction dependantAction = null, float value = 0.0f)
        {
            PickUpObject pickUpAction = (PickUpObject) dependantAction;

            // throw the object
            Vector3 cameraForward = pickUpAction.mainCamera.transform.forward;
            pickUpAction.grabbedObjectRb.AddForce(cameraForward * throwingForce, ForceMode.Impulse);
            AudioPlayer.PlaySFX(pushSFX, transform.position, 0.0f);

            // release the object
            pickUpAction.ReleaseObject();
        }
    }
}
