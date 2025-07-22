using UnityEngine;

namespace Cameras
{
    public abstract class BaseCamera : MonoBehaviour
    {
        [SerializeField] private Transform defaultTarget = null;

        protected Transform currentTarget { get; private set;} = null;

        protected virtual void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            if (defaultTarget != null && currentTarget == null)
                SetTarget(defaultTarget);
        }

        /// <summary>
        /// Set the camera target
        /// </summary>
        /// <param name="target"> the target to set</param>
        public void SetTarget(Transform target) => currentTarget = target;

        public abstract void ProcessCameraMovement(Vector2 direction);
    }
}
