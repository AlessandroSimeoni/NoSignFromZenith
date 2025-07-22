using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    public class AudioPlayer : MonoBehaviour
    {
        [SerializeField] private AudioMixerGroup bgmMixerGroup = null;
        [SerializeField] private AudioMixerGroup sfxMixerGroup = null;
        [SerializeField] private AudioMixerGroup audioLogMixerGroup = null;

        public static AudioMixerGroup bgmMixer = null;
        private static AudioMixerGroup sfxMixer = null;
        private static AudioMixerGroup audioLogMixer = null;

        private static GameObject bgmObject = null;
        private static GameObject audioLogObject = null;

        private void Start()
        {
            bgmMixer = bgmMixerGroup;
            sfxMixer = sfxMixerGroup;
            audioLogMixer = audioLogMixerGroup;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// set up the gameobject of the sfx
        /// </summary>
        /// <param name="name">the clip name</param>
        /// <param name="position">the position of the clip</param>
        /// <returns>the gameobject</returns>
        private static GameObject SetupGameObject(string name, Vector3 position)
        {
            GameObject go = new GameObject($"sfx {name}");
            go.transform.position = position;
            return go;
        }

        /// <summary>
        /// Setup the audio source component
        /// </summary>
        /// <param name="go">the gameobject</param>
        /// <param name="clip">the clip</param>
        /// <param name="volume">the volume</param>
        /// <param name="spatialBlend">is an SFX?</param>
        /// <returns>the audiosource component</returns>
        private static AudioSource SetupAudioSource(GameObject go, AudioClip clip, AudioMixerGroup mixerGroup, float volume, float spatialBlend)
        {
            AudioSource audioSource = go.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = mixerGroup;
            audioSource.volume = volume;
            audioSource.spatialBlend = spatialBlend;
            audioSource.clip = clip;

            return audioSource;
        }

        /// <summary>
        /// play an sfx once
        /// </summary>
        /// <param name="clip">the audio clip</param>
        /// <param name="position">the position</param>
        /// <param name="spatialBlend">the spatial blend</param>
        /// <param name="dontDestroyOnLoad">don't destroy on load?</param>
        /// <param name="volume">the volume</param>
        public static void PlaySFX(AudioClip clip, Vector3 position, float spatialBlend = 1.0f, float volume = 1.0f, bool dontDestroyOnLoad = false)
        {
            // create game object
            GameObject go = SetupGameObject(clip.name, position);

            // set up the audio source and play the clip
            AudioSource audioSource = SetupAudioSource(go, clip, sfxMixer, volume, spatialBlend);
            audioSource.Play();

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(go);

            // destroy the game object at the end of the clip
            Destroy(go, clip.length);
        }

        /// <summary>
        /// Play an sfx in loop
        /// </summary>
        /// <param name="clip">the audio clip</param>
        /// <param name="position">the position of the clip</param>
        /// <param name="spatialBlend">the spatial blend</param>
        /// <param name="volume">the volume of the clip</param>
        /// <returns>the gameobject playing the clip</returns>
        public static GameObject LoopSFX(AudioClip clip, Vector3 position, float spatialBlend = 1.0f, float volume = 1.0f)
        {
            // create gameobject
            GameObject go = SetupGameObject(clip.name, position);

            // set up the audio source and play the clip
            AudioSource audioSource = SetupAudioSource(go, clip, sfxMixer, volume, spatialBlend);
            audioSource.loop = true;
            audioSource.Play();

            return go;
        }

        /// <summary>
        /// Play an audio log
        /// </summary>
        /// <param name="clip">the audio clip</param>
        /// <param name="volume">the volume</param>
        public static void PlayAudioLog(AudioClip clip, float volume = 1.0f)
        {
            // only one audio log can be played at the same time
            if (audioLogObject != null)
                Destroy(audioLogObject);

            // create gameobject
            audioLogObject = SetupGameObject(clip.name, Vector3.zero);

            // set up the audio source and play the audio log
            AudioSource audioSource = SetupAudioSource(audioLogObject, clip, audioLogMixer, volume, 0.0f);
            audioSource.Play();

            // destroy the game object at the end of the audio log
            Destroy(audioLogObject, clip.length);
        }

        /// <summary>
        /// Play bgm music 
        /// </summary>
        /// <param name="clip">the clip</param>
        /// <param name="volume">the volume</param>
        public static void PlayBGM(AudioClip clip, float volume = 1.0f)
        {
            // only one bgm music can be played at the same time
            if (bgmObject != null)
                Destroy(bgmObject);

            bgmObject = SetupGameObject(clip.name, Vector3.zero);

            AudioSource audioSource = SetupAudioSource(bgmObject, clip, bgmMixer, volume, 0.0f);
            audioSource.loop = true;
            audioSource.Play();
        }

        /// <summary>
        /// interrupt an audio by destroying the corresponding gameobject, useful to stop looping SFX
        /// </summary>
        /// <param name="go">the gameobject</param>
        public static void CancelAudio(GameObject go)
        {
            // do nothing if the gameobject doesn't have an audio source
            AudioSource audioSource = go.GetComponent<AudioSource>();
            if (audioSource == null)
                return;

            // handle audio fade before stopping it? 

            Destroy(go);
        }

        /// <summary>
        /// Pause/Play the audio log
        /// </summary>
        public static void ToggleAudioLog()
        {
            if (audioLogObject == null)
                return;

            AudioSource audioSource = audioLogObject.GetComponent<AudioSource>();
            if (audioSource.isPlaying)
                audioSource.Pause();
            else
                audioSource.Play();
        }

        /// <summary>
        /// set bgm volume
        /// </summary>
        /// <param name="volume">the volume</param>
        public static void SetBGMVolume(float volume)
        {
            float trueVolume = Mathf.Log10( Mathf.Clamp(volume, 0.0001f, 10.0f) / 10) * 20;
            bgmMixer.audioMixer.SetFloat("BGMVolume", trueVolume);
        }
        
        /// <summary>
        /// set sfx volume
        /// </summary>
        /// <param name="volume">the volume</param>
        public void SetSFXVolume(float volume)
        {
            float trueVolume = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 10.0f) / 10) * 20;
            sfxMixer.audioMixer.SetFloat("SFXVolume", trueVolume);
        }

        /// <summary>
        /// set audio log volume
        /// </summary>
        /// <param name="volume">the volume</param>
        public void SetAudioLogVolume(float volume)
        {
            float trueVolume = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 10.0f) / 10) * 20;
            audioLogMixer.audioMixer.SetFloat("AudioLogVolume", trueVolume);
        }
    }
}
