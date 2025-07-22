using UnityEngine;
using Cameras;
using Input;
using GauntletActions;
using Interactable;
using UI;
using System.Collections;

namespace Controller
{
    public class HumanController : BaseController
    {
        [SerializeField] private BaseCamera defaultControlledCamera = null;
        [SerializeField] private GauntletManager gauntletManager = null;
        [SerializeField] private InteractionManager interactionManager = null;
        [SerializeField] private UIManager uiManager = null;

        private BaseCamera currentControlledCamera;
        private Controls controls;
        private bool wasInTeleportMode = false;

        private void Awake()
        {
            controls = new Controls();
        }

        private void OnEnable()
        {
            controls.Camera.Enable();
            controls.Player.Enable();
            controls.Gauntlet.Enable();
            controls.UI.Enable();

            // teleport is disabled by default
            controls.Gauntlet.Teleport.Disable();
        }

        protected override void Start()
        {
            base.Start();

            if (defaultControlledCamera != null && currentControlledCamera == null)
                SetCameraTarget(defaultControlledCamera);

            if (uiManager != null)
            {
                uiManager.onInGameUIEnabled.AddListener(HandleInGameUIEnabled);
                uiManager.onInGameUIDisabled.AddListener(HandleInGameUIDisabled);
                uiManager.onEnding.AddListener(HandleEndingControls);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (currentControlledCamera != null)
                ControlCamera();

            if (gauntletManager != null)
                ListenGauntletAction();

            if (interactionManager != null)
                ListenInteraction();

            if (uiManager != null)
                ListenUIAction();
        }



        private void OnDisable()
        {
            controls.Camera.Disable();
            controls.Player.Disable();
            controls.Gauntlet.Disable();
            controls.UI.Disable();
        }

        /// <summary>
        /// set camera target
        /// </summary>
        /// <param name="cameraTarget">the camera to control</param>
        public void SetCameraTarget(BaseCamera cameraTarget)
        {
            currentControlledCamera = cameraTarget;
        }


        /// <summary>
        /// Control the camera movement
        /// </summary>
        private void ControlCamera()
        {
            // read input
            Vector2 cameraShift = controls.Camera.Move.ReadValue<Vector2>();

            // move camera
            currentControlledCamera.ProcessCameraMovement(cameraShift);
        }

        /// <summary>
        /// Control the character movement
        /// </summary>
        protected override void ControlCharacter()
        {
            // read input
            Vector2 characterShift = controls.Player.Move.ReadValue<Vector2>();

            Vector3 rightDirection = new Vector3(currentControlledCamera.transform.right.x,
                                                 0.0f, 
                                                 currentControlledCamera.transform.right.z).normalized;

            Vector3 forwardDirection = new Vector3(currentControlledCamera.transform.forward.x,
                                                   0.0f,
                                                   currentControlledCamera.transform.forward.z).normalized;

            // move character
            currentControlledCharacter.AddMovement(characterShift.x * rightDirection);
            currentControlledCharacter.AddMovement(characterShift.y * forwardDirection);

            // jump if requested
            if (controls.Player.Jump.WasPressedThisFrame())
                currentControlledCharacter.Jump();
        }

        /// <summary>
        /// Listen the input for the UI
        /// </summary>
        private void ListenUIAction()
        {
            if (controls.UI.Cancel.WasPressedThisFrame())
                uiManager.BackOrPause();
        }

        /// <summary>
        /// Listen the input for the interaction
        /// </summary>
        private void ListenInteraction()
        {
            if (controls.Player.Interact.WasPressedThisFrame())
                interactionManager.RequestInteraction();
        }

        /// <summary>
        /// Listen the input for each possible gauntlet action
        /// </summary>
        private void ListenGauntletAction()
        {
            CheckPickUpObjectRequest();
            CheckThrowObjectRequest();
            CheckMoveObject();
            CheckModeSwitch();
            CheckTeleport();
        }

        /// <summary>
        /// Check if Pick Up Object action is requested
        /// </summary>
        private void CheckPickUpObjectRequest()
        {
            if (controls.Gauntlet.PickUpObject.WasPressedThisFrame())
                gauntletManager.PerformAction(ActionIDs.PICK_UP_OBJECT);
        }

        /// <summary>
        /// Check if Throw Object action is requested
        /// </summary>
        private void CheckThrowObjectRequest()
        {
            if (controls.Gauntlet.ThrowObject.WasPressedThisFrame())
                gauntletManager.PerformAction(ActionIDs.THROW_OBJECT);
        }

        /// <summary>
        /// Check if Move Object action is requested
        /// </summary>
        private void CheckMoveObject()
        {
            float scrollValue = controls.Gauntlet.MoveObject.ReadValue<float>();
            if (scrollValue != 0)
                gauntletManager.PerformAction(ActionIDs.MOVE_OBJECT, scrollValue);
        }

        /// <summary>
        /// Check if gauntlet mode switch is requested and enable/disable the controls accordingly
        /// </summary>
        private void CheckModeSwitch()
        {
            if (controls.Gauntlet.SwitchGauntletModes.WasPressedThisFrame() && 
                gauntletManager.SwitchMode(!controls.Gauntlet.Teleport.enabled))
            {
                SwitchGauntletControls(controls.Gauntlet.Teleport.enabled);
            }
        }

        /// <summary>
        /// Switch the gauntlet controls based on the input condition
        /// </summary>
        /// <param name="condition">the condition</param>
        private void SwitchGauntletControls(bool condition)
        {
            if (condition)
            {
                controls.Gauntlet.Teleport.Disable();
                controls.Gauntlet.PickUpObject.Enable();
                controls.Gauntlet.MoveObject.Enable();
                controls.Gauntlet.ThrowObject.Enable();
            }
            else
            {
                controls.Gauntlet.PickUpObject.Disable();
                controls.Gauntlet.MoveObject.Disable();
                controls.Gauntlet.ThrowObject.Disable();
                controls.Gauntlet.Teleport.Enable();
            }
        }

        /// <summary>
        /// Check if Teleport action is requested
        /// </summary>
        private void CheckTeleport()
        {
            if (controls.Gauntlet.Teleport.WasPressedThisFrame())
                gauntletManager.PerformAction(ActionIDs.TELEPORT);
        }


        /// <summary>
        /// Disable the controls if the in game ui (pause, database, etc...) is enabled
        /// </summary>
        private void HandleInGameUIEnabled()
        {
            controls.Camera.Disable();
            controls.Player.Disable();

            // save the gauntlet mode currently in use
            wasInTeleportMode = controls.Gauntlet.Teleport.enabled;
            controls.Gauntlet.Disable();

            Cursor.lockState = CursorLockMode.Confined;
        }

        /// <summary>
        /// Re-enable the controls if the in game ui (pause, database, etc...) is disabled
        /// </summary>
        private void HandleInGameUIDisabled()
        {
            controls.Camera.Enable();
            controls.Player.Enable();
            controls.Gauntlet.Enable();

            //enable the correct gauntlet mode
            SwitchGauntletControls(!wasInTeleportMode);

            Cursor.lockState = CursorLockMode.Locked;
        }


        /// <summary>
        /// Handle the player movement during the ending
        /// </summary>
        /// <param name="slowDownDuration">the duration of the slowing process</param>
        private void HandleEndingControls(float slowDownDuration)
        {
            StartCoroutine(SlowDownPlayer(slowDownDuration));

            // disable controls except the movement
            controls.Player.Jump.Disable();
            controls.Player.Interact.Disable();
            controls.Gauntlet.Disable();
            controls.UI.Disable();
            interactionManager.gameObject.SetActive(false);
        }

        /// <summary>
        /// slow down the player over time 
        /// </summary>
        /// <param name="slowDuration">the slowing duration process</param>
        /// <returns></returns>
        private IEnumerator SlowDownPlayer(float slowDuration)
        {
            float startTime = Time.time;
            float elapsedTime;

            while (currentControlledCharacter.movementSpeed > 2.0f)
            {
                elapsedTime = Time.time - startTime;

                currentControlledCharacter.movementSpeed = Mathf.Lerp(currentControlledCharacter.movementSpeed, 2.0f, elapsedTime / slowDuration);
                yield return null;
            }

            currentControlledCharacter.movementSpeed = 2.0f;
            yield return null;
        }
    }
}
