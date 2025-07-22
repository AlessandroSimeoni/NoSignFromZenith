using Audio;
using Game;
using Level;
using UnityEngine;

namespace Interactable {
    public class BriefcaseKeycard : InteractableObject
    {
        [SerializeField] private AudioClip collectSFX;
        [SerializeField] private LevelAccess levelAccessToUnlock = null;

        public sealed class EpilogueEvent : UnityEngine.Events.UnityEvent { }
        public EpilogueEvent requestEpilogue = new EpilogueEvent();

        public override void Interact()
        {
            if (GameState.levelCompleted == LevelIndex.Level3)
                requestEpilogue.Invoke();
            else
                levelAccessToUnlock.UnlockLevel();

            AudioPlayer.PlaySFX(collectSFX, transform.position);
            Destroy(gameObject);
        }
    }
}