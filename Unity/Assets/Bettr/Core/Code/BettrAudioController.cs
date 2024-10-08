using System;
using CrayonScript.Code;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [RequireComponent(typeof(AudioSource))]
    [Serializable]
    // attached to Core MainScene
    public class BettrAudioController : MonoBehaviour
    {
        public AudioSource AudioSource { get; private set; }

        // ReSharper disable once InconsistentNaming
       [SerializeField] private AudioClip[] AudioClips;
        
        public static BettrAudioController Instance { get; private set; }
        
        public void Awake()
        {
            var audioSource = gameObject.GetComponent<AudioSource>();
            AudioSource = audioSource;

            Instance = this;
        }

        public void ToggleVolume()
        {
            AudioSource.volume = AudioSource.volume == 0 ? 1 : 0;
        }

        public void PlayAudioOnce(string audioClipName)
        {
            AudioClip clip = GetAudioClipByName(audioClipName);
            if (clip == null) return;

            if (AudioSource.isPlaying)
            {
                AudioSource.Stop();
            }

            AudioSource.clip = clip;
            AudioSource.loop = false;
            AudioSource.Play();
        }
        
        public void PlayAudioLoop(string audioClipName)
        {
            AudioClip clip = GetAudioClipByName(audioClipName);
            if (clip == null) return;

            if (AudioSource.isPlaying)
            {
                AudioSource.Stop();
            }

            AudioSource.clip = clip;
            AudioSource.loop = true;
            AudioSource.Play();
        }
        
        public void PlayGameAudioLoop(string bundleName, string bundleVariant, string audioClipName)
        {
            PlayAudioLoop(audioClipName);
        }
        
        public void StopAudio()
        {
            AudioSource.Stop();
        }
        
        private AudioClip GetAudioClipByName(string audioClipName)
        {
            foreach (var audioClip in AudioClips)
            {
                if (audioClip.name == audioClipName)
                {
                    return audioClip;
                }
            }

            Debug.LogWarning($"Audio clip '{audioClipName}' not found in BettrAudioController.");
            return null;
        }
    }
}