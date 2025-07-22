using UnityEngine;
using UnityEngine.UI;

namespace Interactable
{
    public class InteractionManager : MonoBehaviour
    {
        [SerializeField] private float interactionRange = 2.0f;
        [SerializeField] private LayerMask interactableLayers;
        [SerializeField] private GameObject interactionText;

        private Camera mainCamera = null;
        private RaycastHit hit;
        private Text interactionString = null;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Start()
        {
            interactionString = interactionText.GetComponent<Text>();
        }

        private void Update()
        {
            // do nothing if camera is missing
            if (mainCamera == null)
                return;

            if (RaycastCheck())
            {
                interactionText.SetActive(true);
                switch (hit.transform.gameObject.tag)
                {
                    case "Database":
                        interactionString.text = "Press E to interact";
                        break;
                    case "Collectible":
                        interactionString.text = "Press E to collect";
                        break;
                    case "Briefcase":
                        interactionString.text = "Press E to open";
                        break;
                    case "Workstation":
                        interactionString.text = "Press E to dive";
                        break;
                    default:
                        interactionString.text = "Press E to interact";
                        break;
                }
            }
            else
                interactionText.SetActive(false);
        }

        /// <summary>
        /// Process the requested interaction
        /// </summary>
        public void RequestInteraction()
        {
            // do nothing if camera is missing
            if (mainCamera == null)
                return;

            // check the outcome of the raycast
            if (RaycastCheck())
            {
                // interact if the object has the InteractableObject component
                InteractableObject interactableObject = hit.transform.gameObject.GetComponent<InteractableObject>();
                if (interactableObject != null)
                    interactableObject.Interact();
            }
        }

        /// <summary>
        /// Check if raycast has hit something interactable
        /// </summary>
        /// <returns>the raycast outcome</returns>
        private bool RaycastCheck()
        {
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            bool raycastHasHit = Physics.Raycast(ray, out hit, interactionRange, ~LayerMask.GetMask("Player"));

            return raycastHasHit && interactableLayers == (interactableLayers | (1 << hit.transform.gameObject.layer));
        }
    }
}
