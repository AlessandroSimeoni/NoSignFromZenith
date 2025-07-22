using Level;
using UnityEngine;

namespace Interactable
{
    [System.Serializable]
    public struct HomeNarrative
    {
        public LevelIndex levelCompleted;

        [TextArea(5, 15)]
        public string briefMission;

        [TextArea(5, 15)]
        public string timelineLog;
    }

    [CreateAssetMenu(fileName = "DatabaseHomeNarrative", menuName = "ScriptableObjects/Database_Home_Narrative")]
    public class DatabaseHomeNarrative : ScriptableObject
    {
        /*
         * this scriptable object will contain the narrative to write in the home 
         * when a specific level is completed
        */

        [Header("Home narrative for each level")]
        public HomeNarrative[] narratives = new HomeNarrative[4];
    }
}