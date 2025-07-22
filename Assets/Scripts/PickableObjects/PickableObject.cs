using UnityEngine;

namespace PickableObjects
{
    public class PickableObject : MonoBehaviour
    {
        [SerializeField] private GameObject particleObject = null;
        [SerializeField] private float rayDistance = 5.0f;
        [SerializeField] private LayerMask ignoreLayers;

        public GrabbedObjectEvent onCollision = new GrabbedObjectEvent();
        public GrabbedObjectEvent invokeRelease = new GrabbedObjectEvent();

        private bool isGrabbed = false;

        private Ray ray;
        private RaycastHit hit;

        private void Update()
        {
            if (!isGrabbed)
                return;

            MoveParticleMarker();
        }

        private void OnCollisionStay(Collision collision)
        {
            if (isGrabbed)
                onCollision.Invoke();
        }

        /// <summary>
        /// move the particle marker on the ground
        /// </summary>
        private void MoveParticleMarker()
        {
            ray = new Ray(transform.position, Vector3.down);
            if (Physics.Raycast(ray, out hit, rayDistance,~ignoreLayers))
                particleObject.transform.position = hit.point;
        }

        /// <summary>
        /// set the object grabbed state
        /// </summary>
        /// <param name="grabState">the grab state</param>
        public void GrabbedState(bool grabState)
        {
            isGrabbed = grabState;
            particleObject.SetActive(grabState);
        }
    }
}
