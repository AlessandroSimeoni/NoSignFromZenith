using Audio;
using System.Collections;
using UnityEngine;

namespace Interactable
{
    public class Briefcase : InteractableObject
    {
        [Header("Lock")]
        [SerializeField, Min(0.0f)] private float lockedIntensity = 5.0f;
        [SerializeField, Min(0.0f)] private float blinkingSpeed = 1.0f;
        [Header("Unlock")]
        [SerializeField] private AudioClip unlockSFX;
        [Header("Opening")]
        [SerializeField] private float openDegrees = -100.0f;
        [SerializeField, Range(0.0f, 1.0f)] private float openingSpeed = 0.5f;
        [Header("Keycard")]
        [SerializeField] private BriefcaseKeycard keycard;

        private Quaternion initialRotation = Quaternion.identity;
        private Quaternion targetRotation = Quaternion.identity;
        private MeshRenderer meshRenderer = null;
        private Material material = null;
        private bool isLocked = true;
        private float lockedColorIntensity = -5.0f;
        private float time = 0.0f;

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            material = meshRenderer.material;
            initialRotation = transform.localRotation;
            targetRotation = Quaternion.AngleAxis(openDegrees, transform.InverseTransformDirection(transform.right));
        }

        private void Update()
        {
            if (!isLocked)
                return;

            time = (time + Time.deltaTime * blinkingSpeed) % (2 * Mathf.PI);
            lockedColorIntensity = lockedIntensity * Mathf.Sin(2 * Mathf.PI * time) + lockedIntensity;
            material.SetColor("_EmissionColor", Color.red * Mathf.Pow(2.0f, lockedColorIntensity));
        }

        public void EnableBriefcase()
        {
            isLocked = false;
            AudioPlayer.PlaySFX(unlockSFX, transform.position, 0.8f);
            gameObject.layer = LayerMask.NameToLayer("Interactable");
            material.SetColor("_EmissionColor", Color.green * Mathf.Pow(2.0f, 10.0f));
        }

        public override void Interact()
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            StartCoroutine(OpenBriefcase());
        }

        private IEnumerator OpenBriefcase()
        {            
            float interpolationValue = 0.0f;

            while (transform.localRotation != targetRotation)
            {
                interpolationValue = Mathf.Clamp01(interpolationValue + openingSpeed * Time.deltaTime);
                transform.localRotation = Quaternion.Lerp(initialRotation, targetRotation, interpolationValue);
                yield return null;
            }

            transform.localRotation = targetRotation;
            material.DisableKeyword("_EMISSION");
            yield return null;
        }

        public void ForceOpening()
        {
            isLocked = false;
            transform.localRotation = targetRotation;
            material.DisableKeyword("_EMISSION");
            if (keycard != null)
                Destroy(keycard.gameObject);
        }
    }
}