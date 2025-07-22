using Audio;
using Hub;
using Level;
using UnityEngine;

namespace Interactable
{
    public class LevelAccess : InteractableObject
    {
        [SerializeField] private AudioClip unlockSFX;
        [SerializeField] private LevelIndex levelToLoad;

        public LevelIndex _levelToLoad { get { return levelToLoad; } private set { } }

        public sealed class LoadingRequest : UnityEngine.Events.UnityEvent<LevelIndex> { }
        public LoadingRequest onLoadingRequest = new LoadingRequest();

        public override void Interact()
        {
            onLoadingRequest.Invoke(levelToLoad);
            gameObject.layer = LayerMask.NameToLayer("Default");
        }

        public void UnlockLevel()
        {
            AudioPlayer.PlaySFX(unlockSFX, transform.position);

            MeshRenderer meshRenderer = gameObject.transform.parent.GetComponent<MeshRenderer>();
            Material mat = meshRenderer.materials[2]; // screen sx
            mat.SetColor("_EmissionColor", mat.GetColor("_EmissionColor") * 50.0f);

            gameObject.layer = LayerMask.NameToLayer("Interactable");
        }
    }
}