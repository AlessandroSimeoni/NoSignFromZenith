using UnityEngine;
using Level;
using Interactable;
using Game;
using System;
using Audio;
using System.Collections;

namespace Hub 
{
    public class HubManager : MonoBehaviour
    {
        [SerializeField] private Database database = null;
        [SerializeField] private Briefcase afterDive1Briefcase = null;
        [SerializeField] private Briefcase afterDive3Briefcase = null;
        [SerializeField] private BriefcaseKeycard epilogueKeycard = null;
        [SerializeField] private LevelAccess[] levelAccesses;
        [Header("Background Audio")]
        [SerializeField] private AudioClip hubBGM;
        [Header("Audio Logs")]
        [SerializeField] private float secondsBeforeAudioLog = 4.0f;
        [SerializeField] private AudioLog firstAudioLog = null;
        [SerializeField] private AudioLog secondAudioLog = null;
        [SerializeField] private AudioLog thirdAudioLog = null;
        [SerializeField] private AudioLog endingAudioLog = null;

        private bool firstDBInteraction = true;

        public sealed class RequestSceneLoad : UnityEngine.Events.UnityEvent<string> { }
        public RequestSceneLoad onRequestSceneLoad = new RequestSceneLoad();

        public sealed class PlayEpilogue : UnityEngine.Events.UnityEvent<AudioLog> { }
        public PlayEpilogue onEpilogue = new PlayEpilogue();

        private void Start()
        {
            AudioPlayer.PlayBGM(hubBGM);

            foreach (LevelAccess levelAccess in levelAccesses)
                levelAccess.onLoadingRequest.AddListener(HandleLoadingRequest);

            epilogueKeycard.requestEpilogue.AddListener(HandleEpilogueRequest);
        }

        /// <summary>
        /// handle the hub after quitting the database for the first time after a level completed
        /// </summary>
        /// <param name="levelCompleted">the level completed</param>
        public void HandleDatabaseEvents()
        {
            if (firstDBInteraction)
            {
                switch (GameState.levelCompleted)
                {
                    case LevelIndex.Tutorial:
                        GetLevelAccess().UnlockLevel();
                        break;
                    case LevelIndex.Level1:
                        afterDive1Briefcase.EnableBriefcase();
                        break;
                    case LevelIndex.Level2:
                        GetLevelAccess().UnlockLevel();
                        break;
                    case LevelIndex.Level3:
                        afterDive3Briefcase.EnableBriefcase();
                        break;
                }

                firstDBInteraction = false;
            }
        }

        /// <summary>
        /// get the correct level access elements in the array
        /// </summary>
        /// <returns>the level access element</returns>
        private LevelAccess GetLevelAccess()
        {
            return Array.Find<LevelAccess>(levelAccesses, la => la._levelToLoad == GameState.levelCompleted + 1);
        }

        /// <summary>
        /// Initialize the hub based on the level completed
        /// </summary>
        /// <param name="levelCompleted">the last level completed</param>
        public void InitHub()
        {
            firstDBInteraction = true;

            switch (GameState.levelCompleted)
            {
                case LevelIndex.Tutorial:
                    StartCoroutine(StartAudioLog(firstAudioLog));
                    break;
                case LevelIndex.Level1:
                    StartCoroutine(StartAudioLog(secondAudioLog));
                    break;
                case LevelIndex.Level2:
                    StartCoroutine(StartAudioLog(thirdAudioLog));
                    afterDive1Briefcase.ForceOpening();
                    break;
                case LevelIndex.Level3:
                    StartCoroutine(database.Jingle());
                    afterDive1Briefcase.ForceOpening();
                    break;
            }
        }

        /// <summary>
        /// Start the audio log after secondsBeforeAudioLog seconds
        /// </summary>
        /// <param name="audioLog">the audio log</param>
        /// <returns></returns>
        private IEnumerator StartAudioLog(AudioLog audioLog)
        {
            yield return new WaitForSeconds(secondsBeforeAudioLog);
            StartCoroutine(audioLog.PlayAudioLog());
        }

        /// <summary>
        /// handle the loading request after player interaction with level access
        /// </summary>
        /// <param name="levelToLoad">the level to load</param>
        private void HandleLoadingRequest(LevelIndex levelToLoad)
        {
            string sceneToLoad = "";
            switch (levelToLoad)
            {
                case LevelIndex.Level1:
                    sceneToLoad = "Scenes/Level1";
                    break;
                case LevelIndex.Level2:
                    sceneToLoad = "Scenes/Level2";
                    break;
                case LevelIndex.Level3:
                    sceneToLoad = "Scenes/Level3";
                    break;
            }

            onRequestSceneLoad.Invoke(sceneToLoad);
        }

        /// <summary>
        /// handle the game ending request
        /// </summary>
        private void HandleEpilogueRequest()
        {
            onEpilogue.Invoke(endingAudioLog);
        }
    }
}