using Character;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ControllableObjects
{
    public class PressurePlate : MonoBehaviour
    {
        [SerializeField] private ControllableObject[] targets;
        [SerializeField] private bool playerCanTrigger = false;
        [SerializeField] private LayerMask layers;
        [SerializeField, Min(0.0f)] private float activationDelay = 0.0f;
        [SerializeField, Min(0.0f)] private float deactivationDelay = 0.0f;

        private List<GameObject> objectsOnPlate = new List<GameObject>();
        private GameObject playerGameobject = null;

        //Christian's Code
        private Material material;

        void Start()
        {
            material = GetComponent<Renderer>().material;

            material.SetColor("_EmissionColor", new Color(1.0f, 0.0f, 0.0f) * 2.0f);
        }



        private void OnCollisionEnter(Collision collision)
        {
            if (layers == (layers | (1 << collision.gameObject.layer)))
            {
                // activates the target if there are no objects on the pressure plate
                if (objectsOnPlate.Count == 0)
                    StartCoroutine(ActivateTargets());

                //add object to list
                objectsOnPlate.Add(collision.gameObject);
                
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (layers == (layers | (1 << collision.gameObject.layer)))
            {
                // remove the object from list
                objectsOnPlate.Remove(collision.gameObject);

                // deactivates the target if there are no objects on the pressure plate
                if (objectsOnPlate.Count == 0)
                    StartCoroutine(DeactivateTargets());

                
            }
        }


        //on triggers checks the trigger collider of the pressure plate's child only for the player
        private void OnTriggerEnter(Collider other)
        {
            if (playerCanTrigger && other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                // activates the target if there are no objects on the pressure plate
                if (objectsOnPlate.Count == 0)
                    StartCoroutine(ActivateTargets());

                SetPlayer(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (playerCanTrigger && other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                RemovePlayer();

                // deactivates the target if there are no objects on the pressure plate
                if (objectsOnPlate.Count == 0)
                    StartCoroutine(DeactivateTargets());
            }
        }

        /// <summary>
        /// set the player: add to list and add listener
        /// </summary>
        /// <param name="other">the collider</param>
        private void SetPlayer(Collider other)
        {
            playerGameobject = other.gameObject;
            objectsOnPlate.Add(playerGameobject);
            playerGameobject.GetComponent<CharacterMovement>().onTeleport.AddListener(HandlePlayerTeleport);
        }

        /// <summary>
        /// remove the player from list
        /// </summary>
        private void RemovePlayer()
        {
            objectsOnPlate.Remove(playerGameobject);
            playerGameobject.GetComponent<CharacterMovement>().onTeleport.RemoveListener(HandlePlayerTeleport);
            playerGameobject = null;
        }

        /// <summary>
        /// Activate the targets waiting for a delay
        /// </summary>
        /// <returns></returns>
        private IEnumerator ActivateTargets()
        {
            // wait for the delay
            yield return new WaitForSeconds(activationDelay);

            // activate targets
            foreach (ControllableObject target in targets)
                target.Activate();
            //Added by Christian
            material.SetColor("_EmissionColor", new Color(0.0f, 1.0f, 0.0f) * 2);
        }

        /// <summary>
        /// Deactivate the targets waiting for a delay
        /// </summary>
        /// <returns></returns>
        private IEnumerator DeactivateTargets()
        {
            // wait for the delay
            yield return new WaitForSeconds(deactivationDelay);

            // activate targets
            foreach (ControllableObject target in targets)
                target.Deactivate();
            //Added by Christian
            material.SetColor("_EmissionColor", new Color(1.0f, 0.0f, 0.0f) * 2);
        }

        /// <summary>
        /// check if the player has moved away from the plate and if so remove it
        /// </summary>
        private void HandlePlayerTeleport()
        {
            RaycastHit[] boxcast = Physics.BoxCastAll(transform.position,
                                                      new Vector3(transform.localScale.x / 2, 0.25f, transform.localScale.z / 2),
                                                      Vector3.up,
                                                      transform.rotation,
                                                      0.5f,
                                                      1 << LayerMask.NameToLayer("Player"));

            if (boxcast.Length == 0)
            {
                RemovePlayer();
                if (objectsOnPlate.Count == 0)
                    StartCoroutine(DeactivateTargets());
            }
        }
    }
}


