using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [RequireComponent(typeof(AudioSource))]
    [Serializable]
    // attached to Core MainScene
    public class BettrAudioController : MonoBehaviour
    {
        public static bool UseFileSystemOutcomes = true;
        
        public AudioSource AudioSource { get; private set; }

        // ReSharper disable once InconsistentNaming
       [SerializeField] private AudioClip[] AudioClips;

        public static BettrAudioController Instance { get; private set; }
        
        public string FileSystemAudioBaseURL => "Bettr/LocalStore/LocalAudio";
        
        public void Awake()
        {
            var audioSource = gameObject.GetComponent<AudioSource>();
            AudioSource = audioSource;

            Instance = this;
        }

        private bool ClipExists(string clipName)
        {
            if (AudioClips != null)
            {
                foreach (var audioClip in AudioClips)
                {
                    if (string.Equals(audioClip.name, clipName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void AddToClips(AudioClip clip)
        {
            if (AudioClips == null)
            {
                AudioClips = Array.Empty<AudioClip>();
            }
            var audioClips = new AudioClip[AudioClips.Length + 1];
            AudioClips.CopyTo(audioClips, 0);
            audioClips[AudioClips.Length] = clip;
            AudioClips = audioClips;
        }

        public IEnumerator LoadBackgroundAudio(string bundleName)
        {
            if (UseFileSystemOutcomes)
            {
                yield return LoadFileSystemBackgroundAudio(bundleName);
            }
        }

        public IEnumerator LoadFileSystemBackgroundAudio(string bundleName)
        {
            var backgroundAudioClipName = $"{bundleName}BackgroundMusic";
            var assetPath = Path.Combine(FileSystemAudioBaseURL, $"{backgroundAudioClipName}.mp3");
            var absolutePath = Path.Combine(Application.dataPath, assetPath);
            var absoluteFileUrl = $"file://{absolutePath}";
            AudioClip clip = null;
            using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(absoluteFileUrl, AudioType.MPEG);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error loading audio clip from {absoluteFileUrl}: {request.error}");
                yield break;
            }
            clip = DownloadHandlerAudioClip.GetContent(request);
            clip.name = backgroundAudioClipName;
            if (clip != null)
            {
                AddToClips(clip);
                Debug.Log("Audio clip successfully loaded from file system.");
            }
            else
            {
                Debug.Log($"Failed to load audio clip from {absoluteFileUrl}");
            }
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
            if (ClipExists(audioClipName))
            {
                PlayAudioLoop(audioClipName);
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