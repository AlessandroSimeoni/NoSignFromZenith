using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Level;
using UI;
using Hub;
using Interactable;
using Audio;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private UIManager uiManager = null;
        [SerializeField] private LevelManager levelManager = null;
        [SerializeField] private HubManager hubManager = null;
        [SerializeField] private AudioClip mainMenuBGM;

#if UNITY_EDITOR
        [Header("DEBUG")]
        [SerializeField] private LevelIndex levelCompleted;

        [ContextMenu("SetLevelCompleted")]
        private void SetLevelCompleted()
        {
            GameState.levelCompleted = levelCompleted;
        }
#endif

        private GameData gameData = new GameData();
        private bool isMainMenuScene = false;

        private const string SCENE_MENU = "Scenes/MainMenu";
        private const string SCENE_HUB = "Scenes/Hub";
        private const string SCENE_TUTORIAL = "Scenes/Tutorial";

        private void Awake()
        {
            Time.timeScale = 1.0f;
            gameData.Load();
            if (GameData.Exist() && GameState.firstLoad)
                LoadFromSaveData();
        }

        private void Start()
        {
            isMainMenuScene = SceneManager.GetActiveScene().name == "MainMenu";
            if (isMainMenuScene)
                AudioPlayer.PlayBGM(mainMenuBGM);

            PrepareUIManager();
            PrepareLevelManager();
            PrepareHubManager();
        }

        /// <summary>
        /// prepare the hub manager for the game
        /// </summary>
        private void PrepareHubManager()
        {
            if (hubManager != null)
            {
                hubManager.InitHub();
                hubManager.onRequestSceneLoad.AddListener(HandleSceneLoadFromHub);
                hubManager.onEpilogue.AddListener(HandleEpilogue);
            }
        }

        /// <summary>
        /// Prepare the level manager for the game
        /// </summary>
        private void PrepareLevelManager()
        {
            if (levelManager != null)
                levelManager.onExitEvent.AddListener(ReturnToHub);
        }

        /// <summary>
        /// prepare the UI manager for the game
        /// </summary>
        private void PrepareUIManager()
        {
            if (uiManager != null)
            {
                uiManager.onInGameUIEnabled.AddListener(EnablePauseState);
                uiManager.onInGameUIDisabled.AddListener(DisablePauseState);
                uiManager.onDBExit.AddListener(HandleDatabaseEvent);

                if (isMainMenuScene)
                    uiManager.ShowMainMenu();
            }
        }

        /// <summary>
        /// set the game datas equal to the saved ones
        /// </summary>
        private void LoadFromSaveData()
        {
            GameState.levelCompleted = gameData.levelCompleted;
            Database.collectibles = gameData.collectibles;
            UIDatabase.registryCurrentState = gameData.registryDBState;
        }

        /// <summary>
        /// Handle the database exit event
        /// </summary>
        private void HandleDatabaseEvent()
        {
            hubManager.HandleDatabaseEvents();
        }

        /// <summary>
        /// handle the request of loading a new scene from the hub
        /// </summary>
        /// <param name="sceneToLoad"></param>
        private void HandleSceneLoadFromHub(string sceneToLoad)
        {
            StartCoroutine(LoadScene(sceneToLoad));
        }

        /// <summary>
        /// start the game ending
        /// </summary>
        private void HandleEpilogue(AudioLog endingLog)
        {
            StartCoroutine(StartEnding(endingLog));
        }

        /// <summary>
        /// Load a scene
        /// </summary>
        /// <param name="sceneName">the path of the scene</param>
        /// <returns></returns>
        private IEnumerator LoadScene(string sceneName)
        {
            // disable first load boolean to avoid overwriting the game state if there is a save file
            GameState.firstLoad = sceneName == SCENE_MENU;

            AsyncOperation sceneLoading = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            sceneLoading.allowSceneActivation = false;

            while (sceneLoading.progress < 0.9f)
                yield return null;

            yield return null;

            sceneLoading.allowSceneActivation = true;
        }

        /// <summary>
        /// Process the ending
        /// </summary>
        /// <param name="endingLog">the ending audio log</param>
        /// <returns></returns>
        private IEnumerator StartEnding(AudioLog endingLog)
        {
            // 5 seconds before the ending starts
            yield return new WaitForSeconds(5.0f);

            // black fade
            uiManager.Ending();

            // wait the audio log ending
            yield return StartCoroutine(endingLog.PlayAudioLog());

            // quit the game
            QuitGame();
        }

        /// <summary>
        /// increase the level completed and load the hub scene
        /// </summary>
        private void ReturnToHub()
        {
            GameState.levelCompleted = (LevelIndex) (GameState.levelCompleted + 1);
            StartCoroutine(LoadScene(SCENE_HUB));
        }

        /// <summary>
        /// set timescale to 1
        /// </summary>
        private void DisablePauseState()
        {
            Time.timeScale = 1.0f;
            AudioPlayer.ToggleAudioLog();
        }

        /// <summary>
        /// set timescale to 0
        /// </summary>
        private void EnablePauseState()
        {
            Time.timeScale = 0.0f;
            AudioPlayer.ToggleAudioLog();
        }


        /// <summary>
        /// Start a new game (called when "New Game" button is pressed in the main menu)
        /// </summary>
        public void NewGame()
        {
            gameData.Delete();
            GameState.levelCompleted = LevelIndex.None;
            StartCoroutine(LoadScene(SCENE_TUTORIAL));
        }

        /// <summary>
        /// Continue the game (called when "Continue" button is pressed in the main menu)
        /// </summary>
        public void Continue()
        {
            StartCoroutine(LoadScene(SCENE_HUB));
        }

        /// <summary>
        /// Quit the game
        /// </summary>
        public void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            Debug.Log("QUIT!");
#endif
        }

        /// <summary>
        /// Load the menu scene
        /// </summary>
        public void ReturnToMenu()
        {
            StartCoroutine(LoadScene(SCENE_MENU));
        }

        /// <summary>
        /// Save the game
        /// </summary>
        public void SaveGame()
        {
            gameData.levelCompleted = GameState.levelCompleted;
            gameData.collectibles = Database.collectibles;
            gameData.registryDBState = UIDatabase.registryCurrentState;
            gameData.Save();
        }
    }
}