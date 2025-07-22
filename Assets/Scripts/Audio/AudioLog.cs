using System.Collections;
using TMPro;
using UnityEngine;

namespace Audio
{
    public class AudioLog : MonoBehaviour
    {
        [System.Serializable]
        private struct Subtitles
        {
            [TextArea(2, 5)]
            public string subtitle;
            [Min(0.0f)]
            public float subtitleTimer;
        }

        [SerializeField] private AudioClip audioClip = null;
        [SerializeField] private Subtitles[] subtitles = new Subtitles[0];
        [SerializeField] private TextMeshProUGUI subtitleTextArea = null;

        /// <summary>
        /// play an audio log and show subtitles
        /// </summary>
        /// <returns></returns>
        public IEnumerator PlayAudioLog()
        {
            float previousBGMVolume; 
            AudioPlayer.bgmMixer.audioMixer.GetFloat("BGMVolume", out previousBGMVolume);

            if (audioClip != null)
            {
                if (previousBGMVolume > -20.0f)
                    AudioPlayer.SetBGMVolume(1.0f);
                AudioPlayer.PlayAudioLog(audioClip);
            }

            for (int i = 0; i < subtitles.Length; i++)
            {
                subtitleTextArea.text = $"<font=\"CreatoDisplay-Medium SDF\"><mark=#000000ee> {subtitles[i].subtitle} </mark></font>";
                yield return new WaitForSeconds(subtitles[i].subtitleTimer);
            }

            AudioPlayer.bgmMixer.audioMixer.SetFloat("BGMVolume", previousBGMVolume);
            subtitleTextArea.text = "";
        }
    }
}