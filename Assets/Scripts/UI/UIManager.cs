using Audio;
using Game;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public enum Screen
    {
        none,
        mainMenu,
        pause,
        settings,
        belowSettings,  //used for the main menu settings
        credits,
        quit,
        database,
        databaseHome,
        databaseBrief,
        databaseSave
    }

    public class UIManager : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] private UIScreen mainMenu = null;
        [SerializeField] private UIScreen pause = null;
        [SerializeField] private UIScreen quit = null;
        [SerializeField] private UIScreen settings = null;
        [SerializeField] private UIScreen BGMSettings = null;
        [SerializeField] private UIScreen SFXSettings = null;
        [SerializeField] private UIScreen AudioLogSettings = null;
        [SerializeField] private UIScreen credits = null;
        [SerializeField] private UIDatabase database = null;
        [Header("Main menù elements")]
        [SerializeField] private Button continueMenuButton = null;
        [SerializeField] private GameObject menuTitle = null;
        [Header("Settings elements")]
        [SerializeField] private Slider bgmSlider = null;
        [SerializeField] private Slider sfxSlider = null;
        [SerializeField] private Slider audioLogSlider = null;
        [Header("Ending")]
        [SerializeField] private Image endingBlackFade = null;
        [SerializeField, Min(0.0f), Tooltip("Fade duration in seconds")] private float fadeDuration = 10.0f;
        [Header("Audio")]
        [SerializeField] private AudioClip buttonSelectionSFX;
        [SerializeField] private AudioClip backSFX;
        [SerializeField] private AudioClip menuOpenSFX;
        [SerializeField] private AudioClip menuCloseSFX;

        public UIEvent onInGameUIEnabled = new UIEvent();
        public UIEvent onInGameUIDisabled = new UIEvent();
        public UIEvent onDBExit = new UIEvent();

        public sealed class EndingEvent : UnityEngine.Events.UnityEvent<float> { }
        public EndingEvent onEnding = new EndingEvent();

        // the current state of the ui
        private Screen currentScreen = Screen.none;
        private bool isMainMenuScene = false;
        private SettingsData settingsData = new SettingsData();

        private void Awake()
        {   
            ToggleContinueButton();
        }

        private void Start()
        {
            isMainMenuScene = SceneManager.GetActiveScene().name == "MainMenu";

            if (database != null)
                database.onDBActive.AddListener(HandleDatabaseActivation);

            HandleAudioVolumes();
        }

        /// <summary>
        /// handle the volumes and the sliders
        /// </summary>
        private void HandleAudioVolumes()
        {
            settingsData.Load();
            if (SettingsData.Exist())
            {
                bgmSlider.value = settingsData.bgmVolume;
                sfxSlider.value = settingsData.sfxVolume;
                audioLogSlider.value = settingsData.audioLogsVolume;
            }

            bgmSlider.onValueChanged.Invoke(bgmSlider.value);
            sfxSlider.onValueChanged.Invoke(sfxSlider.value);
            audioLogSlider.onValueChanged.Invoke(audioLogSlider.value);
        }

        /// <summary>
        /// Enable/disable the continue button 
        /// </summary>
        private void ToggleContinueButton()
        {
            if (continueMenuButton == null)
                return;

            bool gameData = GameData.Exist();

            if (!gameData)
            {
                Image buttonImage = continueMenuButton.gameObject.GetComponent<Image>();
                buttonImage.material = null;
            }
            continueMenuButton.interactable = gameData;
        }

        /// <summary>
        /// called when in the main menu scene or returning to main menu from quit or settings screens.
        /// Hide the quit and settings elements on screen and show the main menu
        /// </summary>
        public void ShowMainMenu() 
        {
            if (quit != null)
                quit.HideScreenElements();
            if (settings != null)
                settings.HideScreenElements();
            if (credits != null)
                credits.HideScreenElements();
            if (mainMenu != null)
            {
                mainMenu.ShowScreenElements();
                currentScreen = Screen.mainMenu;
            }
        }

        /// <summary>
        /// called when pausing the game or returning from settings or quit screens
        /// </summary>
        private void ShowPause()
        {
            if (settings != null)
                settings.HideScreenElements();
            if (quit != null)
                quit.HideScreenElements();
            if (pause != null)
            {
                pause.ShowScreenElements();
                currentScreen = Screen.pause;
            }
        }

        /// <summary>
        /// called when quit is clicked in the main menu or in the pause screen
        /// </summary>
        public void ShowQuit()
        {
            if (mainMenu != null)
                mainMenu.HideScreenElements();
            if (pause != null)
                pause.HideScreenElements();
            if (quit != null)
            {
                quit.ShowScreenElements();
                currentScreen = Screen.quit;
            }
        }

        /// <summary>
        /// called when settings is clicked in the main menu or in the pause screen
        /// </summary>
        public void ShowSettings()
        {
            if (mainMenu != null)
                mainMenu.HideScreenElements();
            if (pause != null)
                pause.HideScreenElements();
            if (isMainMenuScene)
            {
                if (BGMSettings != null)
                    BGMSettings.HideScreenElements();
                if (SFXSettings != null)
                    SFXSettings.HideScreenElements();
                if (AudioLogSettings != null)
                    AudioLogSettings.HideScreenElements();
            }
            if (settings != null)
            {
                settings.ShowScreenElements();
                currentScreen = Screen.settings;
            }
        }

        /// <summary>
        /// show the single setting screen below the main settings screen in the main menu
        /// </summary>
        /// <param name="screen"></param>
        public void ShowSubSettings(UIScreen screen)
        {
            if (settings != null)
                settings.HideScreenElements();
            if (screen != null)
            {
                screen.ShowScreenElements();
                currentScreen = Screen.belowSettings;
            }
        }

        /// <summary>
        /// called when credits is clicked in the main menu
        /// </summary>
        public void ShowCredits()
        {
            if (mainMenu != null)
                mainMenu.HideScreenElements();
            if (menuTitle != null)
                menuTitle.SetActive(false);
            if (credits != null)
            {
                credits.ShowScreenElements();
                currentScreen = Screen.credits;
            }
        }

        /// <summary>
        /// pause the game or return to previous screen (this method will be called by the controller)
        /// </summary>
        public void BackOrPause()
        {
            switch (currentScreen)
            {
                case Screen.settings:
                    AudioPlayer.PlaySFX(backSFX, transform.position, 0.0f);
                    SaveSettings();
                    if (isMainMenuScene)
                        ShowMainMenu();
                    else
                        ShowPause();
                    break;
                case Screen.pause:
                    DisableInGameUI(pause);
                    break;
                case Screen.database:
                    if (database.currentScreen == Screen.databaseHome)
                    {
                        DisableInGameUI(database);
                        onDBExit.Invoke();
                    }
                    else
                    {
                        AudioPlayer.PlaySFX(backSFX, transform.position, 0.0f);
                        database.ShowRegistryScreen();
                    }
                    break;
                case Screen.credits:
                    AudioPlayer.PlaySFX(backSFX, transform.position, 0.0f);
                    menuTitle.SetActive(true);
                    ShowMainMenu();
                    break;
                case Screen.belowSettings:
                    AudioPlayer.PlaySFX(backSFX, transform.position, 0.0f);
                    ShowSettings();
                    break;
                case Screen.none:
                    if (!isMainMenuScene)
                        RequestPause();
                    break;
            }
        }

        /// <summary>
        /// Save settings
        /// </summary>
        private void SaveSettings()
        {
            settingsData.bgmVolume = bgmSlider.value;
            settingsData.sfxVolume = sfxSlider.value;
            settingsData.audioLogsVolume = audioLogSlider.value;
            settingsData.Save();
        }

        /// <summary>
        /// disable the in game screen T
        /// </summary>
        /// <typeparam name="T">the type of the ui screen to be disabled</typeparam>
        /// <param name="screen">the screen to disable</param>
        private void DisableInGameUI<T>(T screen) where T : UIScreen
        {
            AudioPlayer.PlaySFX(menuCloseSFX, transform.position, 0.0f);
            screen.HideScreenElements();
            onInGameUIDisabled.Invoke();
            currentScreen = Screen.none;
        }

        /// <summary>
        /// Request the pause during the game
        /// </summary>
        private void RequestPause()
        {
            AudioPlayer.PlaySFX(menuOpenSFX, transform.position, 0.0f);
            ShowPause();
            onInGameUIEnabled.Invoke();
        }

        /// <summary>
        /// Don't quit; show menu if we are in the main menu scene, otherwise show the pause
        /// </summary>
        public void DontQuit()
        {
            AudioPlayer.PlaySFX(backSFX, transform.position, 0.0f);
            if (isMainMenuScene)
                ShowMainMenu();
            else
                ShowPause();
        }

        /// <summary>
        /// play the button selection clip
        /// </summary>
        public void PlaySelectionClip()
        {
            AudioPlayer.PlaySFX(buttonSelectionSFX, transform.position, 0.0f, 1.0f, true);
        }

        private void HandleDatabaseActivation()
        {
            currentScreen = Screen.database;
            onInGameUIEnabled.Invoke();
        }

        /// <summary>
        /// Handle the ending
        /// </summary>
        public void Ending()
        {
            onEnding.Invoke(fadeDuration);
            StartCoroutine(BlackFade());
        }

        /// <summary>
        /// fade to black in fadeDuration seconds
        /// </summary>
        /// <returns></returns>
        private IEnumerator BlackFade()
        {
            Color color = endingBlackFade.color;
            float startTime = Time.time;
            float elapsedTime;
            float alpha;

            while (endingBlackFade.color.a < 1)
            {
                elapsedTime = Time.time - startTime;
                alpha = Mathf.Lerp(0.0f, 1.0f, elapsedTime / fadeDuration);

                endingBlackFade.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            endingBlackFade.color = new Color(color.r, color.g, color.b, 1.0f);
            yield return null;
        }
    }
}
