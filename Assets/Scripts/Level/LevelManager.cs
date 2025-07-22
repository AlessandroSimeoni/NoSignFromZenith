using Audio;
using UnityEngine;

namespace Level
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelExit levelExit = null;
        [SerializeField] private AudioClip levelBGM = null;
        
        public ExitEvent onExitEvent = new ExitEvent();

        private void Start()
        {
            if (levelBGM != null)
                AudioPlayer.PlayBGM(levelBGM, 0.75f);

            levelExit.onExitEvent.AddListener(HandleLevelExit);
        }

        /// <summary>
        /// handle the level completion
        /// </summary>
        private void HandleLevelExit()
        {
            onExitEvent.Invoke();
        }
    }
}
