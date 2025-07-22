using Audio;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GauntletActions
{
    public class GauntletManager : MonoBehaviour
    {
        [Header("Gauntlet")]
        [SerializeField] private GameObject gauntlet = null;
        [SerializeField] private LayerMask gauntletLayers;
        [SerializeField] private Image crosshair;
        [SerializeField] private Sprite notInRangeCrosshair;
        [SerializeField] private Sprite inRangeCrosshair;
        [Header("Gauntlet Action List")]
        [SerializeField] private List<GauntletAction> actions;
        [Header("SFX")]
        [SerializeField] private AudioClip switchModeSFX;

        private bool teleportMode = false;
        private PickUpObject pickUpAction = null;
        private MeshRenderer meshRenderer = null;
        private Material grabbingMaterial = null;
        private Material teleportModeMaterial = null;
        private Material grabModeMaterial = null;
        private Camera mainCamera;
        private Ray ray;
        private RaycastHit hit;

        private void Start()
        {
            // order the list by action id to avoid the swap of actions by human error in the inspector
            actions = actions.OrderBy<GauntletAction, ActionIDs>(a => a.actionID).ToList<GauntletAction>();

            // add listener to pick up action event
            if (actions[(int)ActionIDs.PICK_UP_OBJECT] is PickUpObject)
            {
                pickUpAction = (PickUpObject)actions[(int)ActionIDs.PICK_UP_OBJECT];
                pickUpAction.onGrabbedObjectCollision.AddListener(HandlePickUpEvent);
                pickUpAction.onGrabOrRelease.AddListener(HandleGrabEmission);
            }

            meshRenderer = gauntlet.GetComponent<MeshRenderer>();
            grabbingMaterial = meshRenderer.materials[0];
            teleportModeMaterial = meshRenderer.materials[1];
            grabModeMaterial = meshRenderer.materials[2];

            mainCamera = Camera.main;
        }

        private void Update()
        {
            SetCrosshair();
        }

        /// <summary>
        /// Set the crosshair based on the object pointed
        /// </summary>
        private void SetCrosshair()
        {
            ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            bool rayHit = Physics.Raycast(ray, out hit, pickUpAction.pickUpRange, ~pickUpAction.ignoreLayers);

            if (rayHit)
            {
                if (gauntletLayers == (gauntletLayers | (1 << hit.transform.gameObject.layer)))
                    crosshair.sprite = inRangeCrosshair;
                else
                    crosshair.sprite = notInRangeCrosshair;
            }
            else
                crosshair.sprite = notInRangeCrosshair;
        }

        /// <summary>
        /// Perform the action specified by the id
        /// </summary>
        /// <param name="id">the action id</param>
        /// <param name="value">the value if necessary</param>
        public void PerformAction(ActionIDs id, float value = 0.0f)
        {
            switch (id)
            {
                case ActionIDs.THROW_OBJECT: 
                case ActionIDs.MOVE_OBJECT:
                    CallPickUpDependantAction(id, value);
                    break;
                default:
                    actions[(int)id].PerformAction();
                    break;
            }
        }


        /// <summary>
        /// Call an action that depends on the PickUpObject action, if possible
        /// </summary>
        /// <param name="id">the action id</param>
        /// <param name="value">the input value</param>
        private void CallPickUpDependantAction(ActionIDs id, float value)
        {
            if (pickUpAction == null)
                return;

            // perform the action if the player is grabbing an object
            if (pickUpAction.isGrabbing)
                actions[(int)id].PerformAction(pickUpAction, value);
        }

        private void HandlePickUpEvent()
        {
            CallPickUpDependantAction(ActionIDs.MOVE_OBJECT, -0.5f);
        }

        /// <summary>
        /// Switch gauntlet mode if possible and return success or failure
        /// </summary>
        /// <param name="enterTeleportMode">the boolean value to enter/exit the teleport mode</param>
        /// <returns>true if the mode can be switched, false otherwise</returns>
        public bool SwitchMode(bool enterTeleportMode)
        {
            if (!pickUpAction.isGrabbing)
            {
                teleportMode = enterTeleportMode;
                AudioPlayer.PlaySFX(switchModeSFX, transform.position, 0.0f);
                SwitchEmissions();
                return true;
            }

            return false;
        }

        /// <summary>
        /// switch gauntlet emissions
        /// </summary>
        private void SwitchEmissions()
        {
            if (teleportMode)
            {
                grabModeMaterial.DisableKeyword("_EMISSION");
                teleportModeMaterial.EnableKeyword("_EMISSION");
            }
            else
            {
                teleportModeMaterial.DisableKeyword("_EMISSION");
                grabModeMaterial.EnableKeyword("_EMISSION");
            }
        }

        /// <summary>
        /// enable/disable grab emission
        /// </summary>
        private void HandleGrabEmission()
        {
            if (grabbingMaterial.IsKeywordEnabled("_EMISSION"))
                grabbingMaterial.DisableKeyword("_EMISSION");
            else
                grabbingMaterial.EnableKeyword("_EMISSION");
        }

        /// <summary>
        /// Force the object release
        /// </summary>
        public void ForceObjectRelease()
        {
            pickUpAction.ReleaseObject();
        }
    }
}
