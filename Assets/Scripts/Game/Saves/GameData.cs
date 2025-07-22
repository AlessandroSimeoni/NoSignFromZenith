using Interactable;
using Level;
using System.Collections.Generic;
using System.IO;
using UI;
using UnityEngine;

namespace Game
{
    [System.Serializable]
    public class GameData : ISaveData
    {
        private const string SAVE_PATH = "NoSignFromZenith.save";

        public LevelIndex levelCompleted = LevelIndex.None;
        public List<Collectibles> collectibles = new List<Collectibles>();
        public RegistryState[] registryDBState = new RegistryState[5]; 

        public void Save()
        {
            string jsonString = JsonUtility.ToJson(this);
            File.WriteAllText(SAVE_PATH, jsonString);
        }

        public void Load()
        {
            if (!File.Exists(SAVE_PATH))
                return;

            JsonUtility.FromJsonOverwrite(File.ReadAllText(SAVE_PATH), this);
        }

        public void Delete()
        {
            if (!File.Exists(SAVE_PATH))
                return;

            File.Delete(SAVE_PATH);
        }
        
        public static bool Exist()
        {
            return File.Exists(SAVE_PATH);
        }
    }
}