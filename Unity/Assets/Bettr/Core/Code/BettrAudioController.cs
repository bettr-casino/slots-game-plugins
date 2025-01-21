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
        public static bool UseFileSystemAudio = true;
        public static string FileSystemAudioBaseURL => "Bettr/LocalStore/LocalAudio";
        public static string AudioServerBaseURL;
        
        public AudioSource AudioSource { get; private set; }

        // ReSharper disable once InconsistentNaming
       [SerializeField] private AudioClip[] AudioClips;

        public static BettrAudioController Instance { get; private set; }
        
        public void Awake()
        {
            var audioSource = gameObject.GetComponent<AudioSource>();
            AudioSource = audioSource;
            AudioSource.Stop();

            Instance = this;
            
            // get the player prefs, default is IsVolumeOn == 1
            var isVolumeOn = PlayerPrefs.GetInt("IsVolumeOn", 1);
            AudioSource.mute = isVolumeOn == 0;
        }

        private bool ClipExists(string clipName)
        {
            if (clipName != null && AudioClips != null)
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
            var backgroundAudioClipName = $"{bundleName}BackgroundMusic";
            yield return LoadAudio(backgroundAudioClipName);
        }

        public IEnumerator LoadAudio(string bundleName)
        {
            if (UseFileSystemAudio)
            {
                yield return LoadFileSystemAudio(bundleName);
            }
            else
            {
                yield return LoadS3SystemAudio(bundleName);
            }
        }

        public IEnumerator LoadFileSystemAudio(string bundleName)
        {
            var audioClipName = bundleName;
            var assetPath = Path.Combine(FileSystemAudioBaseURL, $"{audioClipName}.mp3");
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
            if (clip != null)
            {
                clip.name = audioClipName;
                AddToClips(clip);
                Debug.Log("Audio clip successfully loaded from file system.");
            }
            else
            {
                Debug.Log($"Failed to load audio clip from {absoluteFileUrl}");
            }
        }
        
        public IEnumerator LoadS3SystemAudio(string bundleName)
        {
            // Create the URL for the background music
            var audioClipName = bundleName;
            var assetUrl = $"{AudioServerBaseURL}/audio/latest/{audioClipName}.mp3";

            // Use UnityWebRequest to load the AudioClip from S3
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(assetUrl, AudioType.MPEG))
            {
                // Send the request
                yield return request.SendWebRequest();

                // Check for errors
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log($"Error loading audio clip from {assetUrl}: {request.error}");
                    yield break;
                }
                // Get the AudioClip from the request
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                if (clip != null)
                {
                    clip.name = audioClipName;
                    AddToClips(clip);
                    Debug.Log($"Audio clip successfully loaded from s3 {assetUrl}");
                }
                else
                {
                    Debug.Log($"Failed to load audio clip from {assetUrl}");
                }
            }
        }

        public void ToggleVolume()
        {
            AudioSource.mute = !AudioSource.mute;
            // save to player prefs
            PlayerPrefs.SetInt("IsVolumeOn", AudioSource.mute ? 0 : 1);
        }
        
        public bool IsVolumeOn()
        {
            var isVolumeOn = PlayerPrefs.GetInt("IsVolumeOn", 1) == 1;
            return isVolumeOn;
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
        
        public void PlayGamePreviewAudioLoop(string bundleName, string bundleVariant, string audioClipName)
        {
            // TODO: FIXME turn ai auto clip off
            // if (ClipExists(audioClipName))
            // {
            //     PlayAudioLoop(audioClipName);
            //     return;
            // }
            
            var genreKey = $"{bundleName}{bundleVariant}";
            
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
                if (genreKey.StartsWith(genre, StringComparison.InvariantCultureIgnoreCase))
                {
                    // remove the Game<NNN> from the genre but keep for example the "Epic"
                    var clipName = genre.Substring("Game001".Length);
                    PlayAudioLoop(clipName);
                    break;
                }
            }
            
        }
        
        public void PlayGameAudioLoop(string bundleName, string bundleVariant, string audioClipName)
        {
            // TODO: FIXME turn ai auto clip off
            // if (ClipExists(audioClipName))
            // {
            //     PlayAudioLoop(audioClipName);
            //     return;
            // }
            
            var genreKey = $"{bundleName}{bundleVariant}";
            
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
                if (genreKey.StartsWith(genre, StringComparison.InvariantCultureIgnoreCase))
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