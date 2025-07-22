using UnityEngine;

namespace Cameras
{
    public class FirstPersonCamera : BaseCamera
    {
        [Header("Inverted Axis")]
        [SerializeField] private bool invertedVerticalAxis = false;
        [SerializeField] private bool invertedHorizontalAxis = false;
        [Header("Sensitivities")]
        [SerializeField, Min(0.0f)] private float verticalSensitivity = 2.0f;
        [SerializeField, Min(0.0f)] private float horizontalSensitivity = 2.0f;
        [Header("Offsets")]
        [SerializeField] private float heightOffset = 1.75f;
        [SerializeField] private float forwardOffset = 0.5f;
        [Header("Vertical Angle Limits")]
        [SerializeField, Tooltip("Below horizontal")] private float maxVerticalAngle = 45f;
        [SerializeField, Tooltip("Above horizontal")] private float minVerticalAngle = -70f;

        private float currentVerticalAngle = 0.0f;

        private void LateUpdate()
        {
            // update camera position and vertical rotation
            transform.position = CalculatePosition();
            transform.rotation = CalculateVerticalRotation();
        }

        /// <summary>
        /// Process the direction for the camera movement
        /// </summary>
        /// <param name="direction">the input direction</param>
        public override void ProcessCameraMovement(Vector2 direction)
        {
            AddVerticalAngle((invertedVerticalAxis ? direction.y : -direction.y) * verticalSensitivity);
            HorizontalRotation((invertedHorizontalAxis ? -direction.x : direction.x) * horizontalSensitivity);
        }

        /// <summary>
        /// Increment the camera vertical angle
        /// </summary>
        /// <param name="value">the quantity to add</param>
        private void AddVerticalAngle(float value)
        {
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle + value, minVerticalAngle, maxVerticalAngle);
        }

        /// <summary>
        /// Rotate target's forward, thus the camera
        /// </summary>
        /// <param name="value">the rotation value</param>
        private void HorizontalRotation(float value)
        {
            currentTarget.forward = Quaternion.AngleAxis(value, currentTarget.up) * currentTarget.forward;
        }

        /// <summary>
        /// Calculate camera's position
        /// </summary>
        /// <returns>the new camera position</returns>
        private Vector3 CalculatePosition()
        {
            return currentTarget.position + currentTarget.forward * forwardOffset + currentTarget.up * heightOffset;
        }

        /// <summary>
        /// Calculate camera's vertical rotation
        /// </summary>
        /// <returns>the new camera rotation</returns>
        private Quaternion CalculateVerticalRotation()
        {
            return Quaternion.LookRotation(
                Quaternion.AngleAxis(currentVerticalAngle, currentTarget.right) * currentTarget.forward,
                currentTarget.up
                );
        }
    }
}
