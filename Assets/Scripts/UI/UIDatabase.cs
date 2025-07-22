using Game;
using Interactable;
using Level;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public enum RegistryState
    {
        Undefined = 0,
        NotRead = 1,
        Read = 2
    }

    public class UIDatabase : UIScreen
    {
        [SerializeField] private GameObject homeScreen;
        [Header("Brief")]
        [SerializeField] private GameObject briefScreen;
        [SerializeField] private DatabaseHomeNarrative homeNarrative;
        [SerializeField] private Text briefMissionText;
        [SerializeField] private Text logText;
        [SerializeField] private Image[] timelineConnections = new Image[4];
        [SerializeField] private Button[] timelineButtons = new Button[4];
        [Header("Registry")]
        [SerializeField] private Button genericButton;
        [SerializeField] private GameObject[] registryRobotButtons = new GameObject[3];
        [SerializeField] private Text registryText;
        [SerializeField] private DatabaseRegistryNarrative registryNarrative;
        [SerializeField] private DatabaseCollectiblesNarrative collectiblesNarrative;
        [Header("New registry content options")]
        [SerializeField] private Text[] registryButtonsText = new Text[5];
        [SerializeField] private Color oldRegistryContentColor = Color.white;
        [SerializeField] private Color newRegistryContentColor = Color.yellow;
        [Header("Save")]
        [SerializeField] private GameObject saveScreen;

        public Screen currentScreen { get; private set; } = Screen.databaseHome;
        public UIEvent onDBActive = new UIEvent();
        public static RegistryState[] registryCurrentState = new RegistryState[5];

        private bool firstInteraction;


        private void Start()
        {
            firstInteraction = true;
        }

        public override void ShowScreenElements()
        {
            ShowRegistryScreen();
            base.ShowScreenElements();
            onDBActive.Invoke();
        }

        /// <summary>
        /// Check if there is new content to read in the registries
        /// </summary>
        private void CheckNewContent()
        {
            for(int i = 0; i < registryCurrentState.Length; i++)
            {
                if (registryCurrentState[i] == RegistryState.NotRead)
                    continue;

                int newDefaultContent = Array.FindIndex(registryNarrative.narratives,
                                                        n => n.registryID == (RegistryID) i &&
                                                             n.levelCompleted == GameState.levelCompleted);

                if (newDefaultContent != -1 && (!GameData.Exist() || (GameData.Exist() && registryCurrentState[i] == RegistryState.Undefined)))
                {
                    registryCurrentState[i] = RegistryState.NotRead;
                    continue;
                }

                for(int j = 0; j < Database.collectibles.Count; j++)
                {
                    if (Database.collectibles[j].hasBeenRead == false && Database.collectibles[j].ID == (RegistryID)i)
                    {
                        registryCurrentState[i] = RegistryState.NotRead;
                        break;
                    }
                }
            }

            firstInteraction = false;
        }

        /// <summary>
        /// Set the registry name color
        /// </summary>
        private void SetRegistryColors()
        {
            for (int i = 0;i < registryCurrentState.Length; i++)
            {
                if (registryCurrentState[i] == RegistryState.NotRead)
                    registryButtonsText[i].color = newRegistryContentColor;
                else
                    registryButtonsText[i].color = oldRegistryContentColor;
            }
        }

        /// <summary>
        /// Enable the timeline buttons base on the level completed
        /// </summary>
        private void EnableTimelineButtons()
        {
            for (int i = 0; i < timelineButtons.Length; i++)
            {
                if (i <= (int)GameState.levelCompleted)
                    timelineButtons[i].interactable = true;
                else
                    timelineButtons[i].interactable = false;
            }
        }

        /// <summary>
        /// set the brief mission text based on the level completed
        /// </summary>
        private void SetBriefMissionText()
        {
            // get the array index
            int arrayIndex = Array.FindIndex(homeNarrative.narratives, n => n.levelCompleted == GameState.levelCompleted);

            //set the brief mission text
            if (arrayIndex != -1)
                briefMissionText.text = homeNarrative.narratives[arrayIndex].briefMission;
        }

        /// <summary>
        /// Show the brief screen
        /// </summary>
        public void ShowBriefScreen()
        {
            SetBriefMissionText();
            EnableTimelineButtons();
            SetTimelineLog((int)GameState.levelCompleted);

            homeScreen.SetActive(false);
            saveScreen.SetActive(false);
            briefScreen.SetActive(true);

            currentScreen = Screen.databaseBrief;
        }

        /// <summary>
        /// show the registry screen
        /// </summary>
        public void ShowRegistryScreen()
        {
            if (firstInteraction)
                CheckNewContent();
            SetRegistryColors();

            EnableRegistryButtons();

            int regID = 0;
            if ((int)GameState.levelCompleted > 0)
                regID = (int)GameState.levelCompleted + 1;

            SetRegistryNarrative(regID);

            if (regID == 0)
                genericButton.Select();
            else
                registryRobotButtons[regID - 2].GetComponentInChildren<Button>().Select();

            saveScreen.SetActive(false);
            briefScreen.SetActive(false);
            homeScreen.SetActive(true);
            currentScreen = Screen.databaseHome;
        }

        /// <summary>
        /// Enable/disable the registry buttons based on the level completed
        /// GENERIC and FREE buttons will be enabled by default
        /// </summary>
        private void EnableRegistryButtons()
        {
            for (int i = 0; i < registryRobotButtons.Length; i++)
            {
                if (i <= (int)GameState.levelCompleted - 1)
                    registryRobotButtons[i].SetActive(true);
                else
                    registryRobotButtons[i].SetActive(false);
            }
        }

        /// <summary>
        /// show the save screen
        /// </summary>
        public void ShowSaveScreen()
        {
            homeScreen.SetActive(false);
            briefScreen.SetActive(false);
            saveScreen.SetActive(true);
            currentScreen = Screen.databaseSave;
        }

        /// <summary>
        /// set the timeline log based on the level
        /// </summary>
        /// <param name="level">the level</param>
        public void SetTimelineLog(int level)
        {
            // get the array index
            int arrayIndex = Array.FindIndex(homeNarrative.narratives, n => n.levelCompleted == (LevelIndex)level);
            // set the timeline log text
            if (arrayIndex != -1)
                logText.text = homeNarrative.narratives[arrayIndex].timelineLog;

            // enbable/disable the connections images
            for(int i = 0; i<timelineConnections.Length; i++)
            {
                if (i == level)
                    timelineConnections[i].enabled = true;
                else
                    timelineConnections[i].enabled = false;
            }
        }

        /// <summary>
        /// Show the correct registry narrative
        /// </summary>
        public void SetRegistryNarrative(int registryID)
        {
            // registry is signed as read
            registryCurrentState[registryID] = RegistryState.Read;
            SetRegistryColors();

            string output;

            //select the default narrative to be shown in the registry
            output = DefaultRegistryNarrative(registryID);

            // handle collectibles
            output += ExtraRegistryNarrative(registryID);

            registryText.text = output;
        }

        /// <summary>
        /// Select the extra registry narrative to be shown based on the collectibles collected and the registry ID
        /// </summary>
        /// <param name="registryID">the registry ID</param>
        /// <returns></returns>
        private string ExtraRegistryNarrative(int registryID)
        {
            string output = "";
            List<RegistryNarrative> narrativeToBeShown = new List<RegistryNarrative>();

            //search in the collectibles the player picked up and select the correct piece of narrative
            for (int i = 0; i < Database.collectibles.Count; i++)
            {
                int arrayIndex = Array.FindIndex(collectiblesNarrative.narratives,
                                                 n => n.collectibleRegistryID == (RegistryID)registryID
                                                      && n.collectibleRegistryID == Database.collectibles[i].ID
                                                      && n.subIndex == Database.collectibles[i].subIndex);

                if (arrayIndex != -1)
                {
                    Database.collectibles[i].hasBeenRead = true;
                    output += collectiblesNarrative.narratives[arrayIndex].narrative;
                    output += "\n--------------------------\n";
                }
            }

            return output;
        }

        /// <summary>
        /// select the default narrative to be shown in the registry, based on the registry ID and the last level completed
        /// </summary>
        /// <param name="registryID">the registry ID</param>
        /// <returns></returns>
        private string DefaultRegistryNarrative(int registryID)
        {
            // select the narrative
            List<RegistryNarrative> narrativeToBeShown = new List<RegistryNarrative>();

            for (int i = 0; i < registryNarrative.narratives.Length; i++)
            {
                if (registryNarrative.narratives[i].registryID == (RegistryID)registryID &&
                    registryNarrative.narratives[i].levelCompleted <= GameState.levelCompleted)
                {
                    narrativeToBeShown.Add(registryNarrative.narratives[i]);
                }
            }

            // prepare the output
            string output = "";
            for (int i = 0; i < narrativeToBeShown.Count; i++)
            {
                output += narrativeToBeShown[i].narrative;
                output += "\n--------------------------\n";
            }

            return output;
        }
    }
}