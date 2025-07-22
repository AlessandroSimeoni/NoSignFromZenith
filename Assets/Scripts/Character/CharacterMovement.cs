using UnityEngine;

namespace Character {

    [RequireComponent(typeof(CharacterController))]
    public class CharacterMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0.0f)] public float movementSpeed = 1.0f;
        [Header("Jump")]
        [SerializeField, Min(0.0f)] private float jumpHeight = 1.0f;
        [Header("Ground Check")]
        [SerializeField] private float sphereCastRadius = 0.5f;
        [SerializeField] private float sphereCastDistance = 1.0f;
        [SerializeField] private float floorDetectionOffset = 0.01f;
        [SerializeField] private LayerMask ignoreLayers;

        private Vector3 movementDirection = Vector3.zero;
        private CharacterController characterController = null;
        private Vector3 inputDirectionBuffer = Vector3.zero;
        private Vector3 currentAppliedGravity = Vector3.zero;
        private float verticalVelocity = 0.0f;
        private bool isJumping = false;

        public sealed class TeleportEvent : UnityEngine.Events.UnityEvent { }
        public TeleportEvent onTeleport = new TeleportEvent();

        private bool isGrounded
        { 
            get
            { 
                Ray ray = new Ray(transform.TransformPoint(characterController.center), Physics.gravity.normalized);
                float trueDistance = sphereCastDistance - sphereCastRadius + characterController.skinWidth + floorDetectionOffset;
                return Physics.SphereCast(ray, sphereCastRadius, trueDistance, ~ignoreLayers);
            } 
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            // set heigth considering the skin width of the character controller
            characterController.Move(transform.up * characterController.skinWidth);
        }

        private void Update()
        {
            Move();
            HandleJump();
        }

        private void HandleJump()
        {
            if (isGrounded || verticalVelocity < 0)
            {
                verticalVelocity = 0.0f;
                isJumping = false;
            }

            if (isJumping)
                verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }

        private void FixedUpdate()
        {
            if (!isJumping)
            {
                SetGravityToApply();
                // apply gravity
                characterController.Move(currentAppliedGravity * Time.fixedDeltaTime);
            }            
        }
        
        /// <summary>
        /// Calculate the gravity to be applied
        /// </summary>
        private void SetGravityToApply()
        {
            currentAppliedGravity = isGrounded ? Vector3.zero : currentAppliedGravity + Physics.gravity * Time.fixedDeltaTime;
        }


        /// <summary>
        /// move the character
        /// </summary>
        private void Move()
        {
            movementDirection = GetMovementDirection();
            
            Vector3 movementVector = new Vector3(movementDirection.x * movementSpeed,
                                                 verticalVelocity,
                                                 movementDirection.z * movementSpeed);

            characterController.Move(movementVector * Time.deltaTime);
        }

        /// <summary>
        /// Get the movement direction from the input direction buffer
        /// </summary>
        /// <returns>the movement direction</returns>
        private Vector3 GetMovementDirection()
        {
            // consume the inputDirectionBuffer
            Vector3 movement = inputDirectionBuffer;
            inputDirectionBuffer = Vector3.zero;

            // normalize direction if magnitude is greater than 1
            if (movement.sqrMagnitude > 1)
                movement.Normalize();

            return movement;
        }

        /// <summary>
        /// Increase the input direction buffer
        /// </summary>
        /// <param name="value">the value to add</param>
        public void AddMovement(Vector3 value)
        {
            // ignore the y axis
            value.y = 0;
            inputDirectionBuffer += value;
        }

        /// <summary>
        /// Perform jump
        /// </summary>
        public void Jump()
        {
            if (isGrounded)
            {
                isJumping = true;
                // physics formula found online
                verticalVelocity += Mathf.Sqrt(jumpHeight  * -2.0f * Physics.gravity.y);
            }
        }

        /// <summary>
        /// Teleport the character to the target position
        /// </summary>
        /// <param name="targetPosition">the target position</param>
        public void Teleport(Vector3 targetPosition)
        {
            characterController.enabled = false;
            transform.position = targetPosition;
            characterController.enabled = true;

            // tell the listeners about the teleport
            onTeleport.Invoke();
        }
    }
}
