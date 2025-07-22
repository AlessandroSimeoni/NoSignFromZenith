using UnityEngine;
using Character;

namespace Controller
{
    public abstract class BaseController : MonoBehaviour
    {
        [SerializeField] private CharacterMovement defaultControlledCharacter = null;

        protected CharacterMovement currentControlledCharacter { get; private set; } = null;

        protected virtual void Start()
        {
            if (defaultControlledCharacter != null && currentControlledCharacter == null)
                SetTarget(defaultControlledCharacter);
        }

        protected virtual void Update()
        {
            if (currentControlledCharacter != null)
                ControlCharacter();
        }

        /// <summary>
        /// set target character
        /// </summary>
        /// <param name="target">the target character</param>
        public void SetTarget(CharacterMovement target) => currentControlledCharacter = target;

        protected abstract void ControlCharacter();
    }
}
