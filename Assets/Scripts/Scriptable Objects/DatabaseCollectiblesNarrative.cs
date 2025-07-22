using UnityEngine;

namespace Interactable
{
    [System.Serializable]
    public struct CollectiblesNarrative
    {
        public RegistryID collectibleRegistryID;
        [Min(0)]
        public int subIndex;

        [TextArea(5, 15)]
        public string narrative;
    }

    [CreateAssetMenu(fileName = "DatabaseCollectiblesNarrative", menuName = "ScriptableObjects/Database_Collectibles_Narrative")]
    public class DatabaseCollectiblesNarrative : ScriptableObject
    {

        /*
            this scriptable object will contain the narrative of a collectible item.
            it is bound to a specific registry and it has a subIndex to handle more collectible items referring 
            to the same registry
         */

        [Header("Story for each collectible item")]
        public CollectiblesNarrative[] narratives = new CollectiblesNarrative[1];

    }
}
