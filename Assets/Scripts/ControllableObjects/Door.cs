using Audio;
using System.Collections;
using UnityEngine;

namespace ControllableObjects
{
    [RequireComponent(typeof(Rigidbody))]
    public class Door : ControllableObject
    {
        [SerializeField] private Vector3 closedPosition;
        [SerializeField] private AudioClip openingSFX;
        [SerializeField] private Vector3 openedPosition;
        [SerializeField] private AudioClip closingSFX;
        [SerializeField, Range(0.0f, 20.0f)] private float openCloseSpeed = 8.0f;
        [SerializeField] private bool isPhysics = false;
        [SerializeField] private LayerMask cubeLayers;

        private Rigidbody rb;
        private Coroutine coroutine = null;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        public override void Activate()
        {
            rb.isKinematic = true;

            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = StartCoroutine(OpenDoor());
        }

        private IEnumerator OpenDoor()
        {
            if (transform.position != openedPosition)
                AudioPlayer.PlaySFX(openingSFX, transform.position, 0.75f);

            while (transform.position != openedPosition)
            {
                transform.position = Vector3.MoveTowards(transform.position, openedPosition, openCloseSpeed * Time.deltaTime);
                yield return null;
            }

            coroutine = null;
        }

        public override void Deactivate()
        {
            if (coroutine != null)
                StopCoroutine(coroutine);

            if (isPhysics)
                rb.isKinematic = false;
            else
                coroutine = StartCoroutine(CloseDoor());

            AudioPlayer.PlaySFX(closingSFX, transform.position, 0.75f);
        }

        private IEnumerator CloseDoor()
        {
            while (transform.position != closedPosition)
            {
                transform.position = Vector3.MoveTowards(transform.position, closedPosition, openCloseSpeed * Time.deltaTime);
                yield return null;
            }

            coroutine = null;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (cubeLayers == (cubeLayers | (1 << collision.gameObject.layer)))
                rb.isKinematic = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            if (cubeLayers == (cubeLayers | (1 << collision.gameObject.layer)))
                if (isPhysics && coroutine == null)
                    rb.isKinematic = false;
        }
    }
}
