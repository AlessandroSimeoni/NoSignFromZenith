using System.IO;
using UnityEngine;

namespace Game
{
    [System.Serializable]
    public class SettingsData : ISaveData
    {
        private const string SAVE_PATH = "NSFZ_Settings.save";

        public float bgmVolume = 0.0f;
        public float sfxVolume = 0.0f;
        public float audioLogsVolume = 0.0f;

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