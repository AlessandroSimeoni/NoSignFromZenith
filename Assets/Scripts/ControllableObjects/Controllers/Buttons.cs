using UnityEngine;

namespace ControllableObjects
{
    public class Buttons : MonoBehaviour
    {
        // Reference to the controllable objects
        [SerializeField] private ControllableObject[] targets;
        [SerializeField] private LayerMask layers;
        [SerializeField] private bool playerCanTrigger = false;



        //Christian's Code
        private Material material;
        void Start()
        {
            material = GetComponent<Renderer>().material;
            material.SetColor("_EmissionColor", new Color(1.0f, 0.0f, 0.0f) * 2.0f);
        }


        // On collision with the button, activate the targets
        private void OnCollisionEnter(Collision collision)
        {
            if (layers == (layers | (1 << collision.gameObject.layer)))
            {
                foreach (ControllableObject target in targets)
                    target.Activate();

                // Play sound when avaiable
                // Button color change if necessary
                material.SetColor("_EmissionColor", new Color(0.0f, 1.0f, 0.0f) * 2.0f);
            }
        }

        // handle player interaction
        private void OnTriggerEnter(Collider other)
        {
            if (playerCanTrigger && other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                foreach (ControllableObject target in targets)
                    target.Activate();
            }
        }
    }
}