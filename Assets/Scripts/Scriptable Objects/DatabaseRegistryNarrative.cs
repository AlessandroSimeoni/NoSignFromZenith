using Level;
using UnityEngine;

namespace Interactable 
{
    public enum RegistryID
    {
        GENERIC,
        FREE,
        KYNTO,
        VENUS,
        WILL
    }

    [System.Serializable]
    public struct RegistryNarrative
    {
        public RegistryID registryID;
        public LevelIndex levelCompleted;

        [TextArea(5, 15)]
        public string narrative;
    }

    [CreateAssetMenu(fileName = "DatabaseRegistryNarrative", menuName = "ScriptableObjects/Database_Registry_Narrative")]
    public class DatabaseRegistryNarrative : ScriptableObject
    {
        /*
            this scriptable object will contain the registry narrative to write in
            a specific registry when a specific level is completed.
        */

        public RegistryNarrative[] narratives = new RegistryNarrative[0];
    }
}