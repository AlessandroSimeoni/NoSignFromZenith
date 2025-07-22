using UnityEngine;

namespace ControllableObjects
{
    public abstract class ControllableObject : MonoBehaviour
    {
        public abstract void Activate();
        public abstract void Deactivate();
    }
}
