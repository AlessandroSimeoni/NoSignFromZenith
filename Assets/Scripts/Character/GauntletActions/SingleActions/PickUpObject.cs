using UnityEngine;
using PickableObjects;
using System.Collections;
using Audio;

namespace GauntletActions
{
    public class PickUpObject : GauntletAction
    {
        #region Serialized Variables
        [Header("Object Pick Up")]
        [SerializeField, Min(0.0f)]
        public float pickUpRange = 2.0f;
        [SerializeField, Tooltip("Determines which layer of objects should be considered for the grab")]
        private LayerMask pickupLayers;
        [SerializeField, Min(0.0f), Tooltip("The default distance of the object from the camera")]
        private float grabbedObjectDefaultDistance = 2.0f;
        [SerializeField] public LayerMask ignoreLayers;
        [SerializeField] private AudioClip grabSFX;
        [Header("Object Movement")]
        [SerializeField, Min(0.0f), Tooltip("The force applied at the moment of pick up")]
        private float movementForce = 50.0f;
        [SerializeField, Min(0.0f), Tooltip("The drag of the object while grabbed")]
        public float objectDrag = 2.0f;
        [SerializeField, Range(0.0f, 1.0f), Tooltip("Determines how fast the grabbed object will align with the camera forward direction")]
        private float objectForwardRotationSpeed = 0.1f;
        [SerializeField, Tooltip("Determines the behaviour of the object if it collides with the environment")]
        private CollisionModes onCollision;
        [SerializeField, Min(0.0f), Tooltip("No force will be applied if the distance of the object from the target position is lesser or equal than this offset")]
        private float deadzoneMovementOffset = 0.001f;
        #endregion

        #region Events
        public GrabbedObjectEvent onGrabbedObjectCollision = new GrabbedObjectEvent();
        public GrabbedObjectEvent onGrabOrRelease = new GrabbedObjectEvent();
        #endregion

        #region Properties
        public bool isGrabbing { get; private set; } = false;
        public Camera mainCamera { get; private set; } = null;
        public Rigidbody grabbedObjectRb { get; private set; } = null;
        public float currentObjectDistance { get; set; }
        #endregion

        #region Private Variables
        private bool requestMovement = false;
        private RaycastHit hitInfo;
        private PickableObject grabbedObject = null;
        private Vector3 targetPosition;
        private Vector3 distanceFromTargetPosition;

        // grabbed object previous options
        private int grabbedObjectOriginalLayer;
        private bool grabbedObjectPreviousUseGravity;
        private float grabbedObjectPreviousDrag;
        #endregion

        #region Enum
        private enum CollisionModes
        {
            DoNothing,
            ReleaseObject,
            ReduceDistance
        }
        #endregion

        public PickUpObject() : base(ActionIDs.PICK_UP_OBJECT) { }

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            // do nothing if is not grabbing
            if (!isGrabbing)
                return;

            // calculate target position and current distance from target position
            targetPosition = mainCamera.transform.position + mainCamera.transform.forward * currentObjectDistance;
            distanceFromTargetPosition = targetPosition - grabbedObject.transform.position;

            // request movement if distance magnitude is greater than the offset
            if (distanceFromTargetPosition.sqrMagnitude > deadzoneMovementOffset)
                requestMovement = true;
        }

        private void FixedUpdate()
        {
            if (grabbedObject != null)
            {
                // align the grabbed object transform forward with the camera one
                grabbedObjectRb.MoveRotation(
                    Quaternion.Lerp(grabbedObjectRb.rotation,
                                    Quaternion.LookRotation(mainCamera.transform.forward, mainCamera.transform.up),
                                    objectForwardRotationSpeed));

                if (requestMovement)
                    MoveGrabbedObject();
            }
        }

        private void MoveGrabbedObject()
        {
            Vector3 forceToApply = Vector3.ClampMagnitude(distanceFromTargetPosition * movementForce, movementForce);
            grabbedObjectRb.AddForce(forceToApply, ForceMode.Impulse);

            //cancel angular velocity
            grabbedObjectRb.angularVelocity = Vector3.zero;

            requestMovement = false;
        }

        public override void PerformAction(GauntletAction dependantAction = null, float value = 0.0f)
        {
            // do nothing if camera is missing
            if (mainCamera == null)
                return;

            if (isGrabbing)
                ReleaseObject();
            else
                PickObject();
        }

        /// <summary>
        /// pick the object to grab
        /// </summary>
        private void PickObject()
        {
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

            bool raycast = Physics.Raycast(ray, out hitInfo, pickUpRange, ~ignoreLayers);

            if (raycast && pickupLayers == (pickupLayers | (1 << hitInfo.transform.gameObject.layer)))
                StartCoroutine(GrabObject(hitInfo));
        }

        /// <summary>
        /// Grab the object picked by the raycast
        /// </summary>
        /// <param name="hitInfo">the info of the raycast hit</param>
        private IEnumerator GrabObject(RaycastHit hitInfo)
        {
            requestMovement = false;
            currentObjectDistance = grabbedObjectDefaultDistance;

            grabbedObject = hitInfo.transform.gameObject.GetComponent<PickableObject>();
            grabbedObject.invokeRelease.AddListener(ReleaseObject);
            grabbedObjectRb = grabbedObject.GetComponent<Rigidbody>();

            // save object's options to later revert the changes
            SaveGrabbedObjectOptions();

            // change layer to avoid player collision
            grabbedObject.gameObject.layer = LayerMask.NameToLayer("GrabbedObject");

            // cancel velocity applied to rigidbody before grabbing
            grabbedObjectRb.velocity = Vector3.zero;

            // disable the use of gravity
            grabbedObjectRb.useGravity = false;

            // set the drag
            grabbedObjectRb.drag = objectDrag;

            isGrabbing = true;
            AudioPlayer.PlaySFX(grabSFX, transform.position, 0.0f);
            onGrabOrRelease.Invoke();

            // Wait for a little bit before setting up the object collision event.
            // The grabbed object uses the onCollisionStay method, which works great with the ReduceDistance mode,
            // but not with the ReleaseObject mode.
            // In this way the object will not be released at the moment of pick up.
            // (yield return null; was not enough)
            yield return new WaitForSeconds(0.1f);

            // check if the object has not already been released
            if (grabbedObject != null)
            {
                grabbedObject.onCollision.AddListener(HandleGrabbedObjectCollision);
                grabbedObject.GrabbedState(true);
            }
        }

        /// <summary>
        /// save grabbed object's options
        /// </summary>
        private void SaveGrabbedObjectOptions()
        {
            grabbedObjectOriginalLayer = grabbedObject.gameObject.layer;
            grabbedObjectPreviousUseGravity = grabbedObjectRb.useGravity;
            grabbedObjectPreviousDrag = grabbedObjectRb.drag;
        }

        /// <summary>
        /// Release the grabbed object
        /// </summary>
        public void ReleaseObject()
        {
            if (!isGrabbing)
                return;

            isGrabbing = false;
            onGrabOrRelease.Invoke();

            ResetGrabbedObjectOptions();

            grabbedObject.onCollision.RemoveListener(HandleGrabbedObjectCollision);
            grabbedObject.invokeRelease.RemoveListener(ReleaseObject);
            grabbedObject.GrabbedState(false);

            // reset rigidbody options previously used
            grabbedObject = null;
            grabbedObjectRb = null;
        }

        /// <summary>
        /// Reset the object's options
        /// </summary>
        private void ResetGrabbedObjectOptions()
        {
            grabbedObject.gameObject.layer = grabbedObjectOriginalLayer;
            grabbedObjectRb.useGravity = grabbedObjectPreviousUseGravity;
            grabbedObjectRb.drag = grabbedObjectPreviousDrag;
        }

        /// <summary>
        /// Handle the collision of the grabbed object with the environment
        /// </summary>
        private void HandleGrabbedObjectCollision()
        {
            switch (onCollision)
            {
                case CollisionModes.ReleaseObject:
                    ReleaseObject();
                    break;
                case CollisionModes.ReduceDistance:
                    onGrabbedObjectCollision.Invoke();
                    break;
            }
        }
    }
}
