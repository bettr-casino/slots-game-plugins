using System;
using CrayonScript.Code;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [RequireComponent(typeof(AudioSource))]
    [Serializable]
    public class BettrAudioBehaviour : MonoBehaviour
    {
        public bool IsVolumeMuted { get; set; }
        
        public AudioSource AudioSource { get; private set; }

        // ReSharper disable once InconsistentNaming
       [SerializeField] private AudioClip[] AudioClips;
        
        public static BettrAudioBehaviour Instance { get; private set; }
        
        public void Awake()
        {
            TileController.RegisterType<BettrAudioBehaviour>("BettrAudioController");
            TileController.AddToGlobals("BettrAudioController", this);
            
            Instance = this;
            
            var audioSource = gameObject.GetComponent<AudioSource>();
            AudioSource = audioSource;
        }

        public void PlayAudioOnce(string audioClipName)
        {
            AudioClip clip = GetAudioClipByName(audioClipName);
            if (clip == null) return;

            if (AudioSource.isPlaying)
            {
                AudioSource.Stop();
            }

            if (IsVolumeMuted) return;

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

            if (IsVolumeMuted) return;

            AudioSource.clip = clip;
            AudioSource.loop = true;
            AudioSource.Play();
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

            Debug.LogWarning($"Audio clip '{audioClipName}' not found in BettrAudioBehaviour.");
            return null;
        }
    }
}