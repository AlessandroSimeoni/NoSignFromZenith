using Audio;
using Character;
using UnityEngine;

namespace GauntletActions
{
    public class Teleport : GauntletAction
    {
        [SerializeField, Min(0.0f)] private float teleportRange = 20.0f;
        [SerializeField, Tooltip("The layers where the player can teleport")] private LayerMask teleportLayers;
        [SerializeField] private LayerMask ignoreLayers;
        [SerializeField] private AudioClip teleportSFX;

        private Camera mainCamera = null;
        private CharacterMovement characterMovement = null;
        private CharacterController characterController = null;
        private RaycastHit hitInfo;
        private Ray ray;
        private Vector3 targetPosition;
        private bool canTeleport;

        private float pickableObjectOffset = 0.2f;
        private const float normalOrientationOffset = 0.0001f;

        private enum TeleportSurface
        {
            floor,
            wall,
            ceiling
        }

        public Teleport() : base(ActionIDs.TELEPORT) { }

        private void Awake()
        {
            mainCamera = Camera.main;
            characterMovement = GetComponentInParent<CharacterMovement>();
            characterController = characterMovement.GetComponent<CharacterController>();
        }

        public override void PerformAction(GauntletAction dependantAction = null, float value = 0)
        {
            canTeleport = false;
            ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

            if (Physics.Raycast(ray, out hitInfo, teleportRange, teleportLayers))
            {
                // calculate the dot product to know the normal orientation
                float normalOrientation = Vector3.Dot(Vector3.up, hitInfo.normal);

                // check the normal orientation using an offset to avoid edge cases that can cause problems due to high precision of the dot product
                switch (normalOrientation)
                {
                    // normal points downwards
                    case < -normalOrientationOffset:
                        canTeleport = CanTeleport(hitInfo, TeleportSurface.ceiling);
                        // handle a specific case where the object pointed is a pickable object with the normal of the face points downwards
                        if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("TeleportPickableObject"))
                        {
                            Vector3 flattedNormal = new Vector3(hitInfo.normal.x, 0.0f, hitInfo.normal.z).normalized;
                            targetPosition = hitInfo.point + flattedNormal * (characterController.skinWidth + characterController.radius + pickableObjectOffset);
                        }
                        else
                            targetPosition = hitInfo.point + hitInfo.normal * (characterController.skinWidth + characterController.height);
                        break;
                    // normal points upwards
                    case > normalOrientationOffset:
                        canTeleport = CanTeleport(hitInfo, TeleportSurface.floor);
                        targetPosition = hitInfo.point + hitInfo.normal * characterController.skinWidth;
                        break;
                    // normal is parallel to the ground
                    default:
                        canTeleport = CanTeleport(hitInfo, TeleportSurface.wall);
                        targetPosition = hitInfo.point + hitInfo.normal * (characterController.radius + characterController.skinWidth);
                        break;
                }

                // teleport if possible
                if (canTeleport)
                {
                    characterMovement.Teleport(targetPosition);
                    AudioPlayer.PlaySFX(teleportSFX, targetPosition);
                }
            }
        }

        /// <summary>
        /// Check if there is enough space to teleport the player
        /// </summary>
        /// <param name="hit">the raycast hit info</param>
        /// <param name="surface">the type of surface selected for the teleport</param>
        /// <returns>true if there is enough space to teleport, false otherwise</returns>
        private bool CanTeleport(RaycastHit hit, TeleportSurface surface)
        {
            Ray sphereRay;

            // check the teleport area based on the type of surface pointed
            switch (surface)
            {
                case TeleportSurface.floor:
                    sphereRay = new Ray(hit.point +
                        hit.normal * (characterController.radius + characterController.skinWidth),
                        Vector3.up);
                    break;
                case TeleportSurface.wall:
                    sphereRay = new Ray(hit.point +
                        hit.normal * (characterController.radius + characterController.skinWidth) +
                        Vector3.up * characterController.radius,
                        Vector3.up);
                    break;
                default:
                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer("TeleportPickableObject"))
                    {
                        Vector3 flattedNormal = new Vector3(hitInfo.normal.x, 0.0f, hitInfo.normal.z).normalized;

                        sphereRay = new Ray(hit.point +
                        flattedNormal * (characterController.radius + characterController.skinWidth + pickableObjectOffset) +
                        Vector3.up * pickableObjectOffset,
                        Vector3.up);
                    }
                    else
                        sphereRay = new Ray(hit.point +
                            hit.normal * (characterController.radius + characterController.skinWidth),
                            Vector3.down);
                    break;
            }

            // if there is space for the player then returns true
            return Physics.SphereCastAll(sphereRay,
                                         characterController.radius,
                                         characterController.height - 2 * characterController.radius,
                                         ~ignoreLayers  //ignore trigger areas
                                         ).Length == 0;
        }
    }
}
