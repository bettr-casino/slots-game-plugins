using System;
using System.Collections;
using System.IO;
using CrayonScript.Code;
using UnityEditor;
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
        
        public string FileSystemAudioBaseURL => "Assets/Bettr/LocalStore/LocalAudio";
        
        public void Awake()
        {
            var audioSource = gameObject.GetComponent<AudioSource>();
            AudioSource = audioSource;

            Instance = this;
        }

        public AudioClip LoadAudioClip(string audioClip)
        {
            var assetPath = Path.Combine(FileSystemAudioBaseURL, $"{audioClip}.mp3");
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            return clip;
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
            var clip = LoadAudioClip(audioClipName);
            if (clip != null)
            {
                if (AudioSource.isPlaying)
                {
                    AudioSource.Stop();
                }

                AudioSource.clip = clip;
                AudioSource.loop = false;
                AudioSource.Play();
                return;
            }
            
            bundleName = bundleName.ToLower();
            
            var genres = new[]
            {
                "Game001Epic", 
                "Game002Buffalo", 
                "Game003HighStakes", 
                "Game004Riches", 
                "Game005Fortunes", 
                "Game006Wheels", 
                "Game007TrueVegas", 
                "Game008Gods", 
                "Game009SpaceInvaders"
            };

            foreach (var genre in genres)
            {
                if (bundleName.StartsWith(genre, StringComparison.InvariantCultureIgnoreCase))
                {
                    // remove the Game<NNN> from the genre but keep for example the "Epic"
                    var clipName = genre.Substring("Game001".Length);
                    PlayAudioOnce(clipName);
                    break;
                }
            }
            
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