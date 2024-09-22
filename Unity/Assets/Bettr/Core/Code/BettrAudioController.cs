using System;
using System.Collections.Generic;
using CrayonScript.Code;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class BettrAudioController
    {
        public bool IsVolumeMuted { get; set; }
        
        public AudioSource[] AudioSources { get; private set; }

        public BettrAudioController()
        {
            TileController.RegisterType<BettrAudioController>("BettrAudioController");
            TileController.AddToGlobals("BettrAudioController", this);
        }

        public void Initialize(GameObject audioGameObject)
        {
            // clear out the current audio sources
            AudioSources = Array.Empty<AudioSource>();
            // get the audio source components
            var audioSources = audioGameObject.GetComponents<AudioSource>();
            // add the audio sources to the list
            AudioSources = audioSources;
        }

        public void PlayAudioOnce(GameObject gameObjectWithAudioSource, string audioClipName)
        {
            // find the audio source with the audio clip "spinbutton"
            foreach (var audioSource in AudioSources)
            {
                if (audioSource.clip.name == audioClipName)
                {
                    // check if the audio source is playing
                    if (audioSource.isPlaying)
                    {
                        // stop the audio source
                        audioSource.Stop();
                    }
                    if (IsVolumeMuted)
                    {
                        continue;
                    }
                    audioSource.loop = false;
                    // play the audio clip
                    audioSource.Play();
                    break;
                }
            }
        }
        
        public void PlayAudioLoop(string audioClipName)
        {
            // find the audio source with the audio clip "spinbutton"
            foreach (var audioSource in AudioSources)
            {
                if (audioSource.clip.name == audioClipName)
                {
                    // check if the audio source is playing
                    if (audioSource.isPlaying)
                    {
                        // stop the audio source
                        audioSource.Stop();
                    }
                    if (IsVolumeMuted)
                    {
                        continue;
                    }
                    audioSource.loop = true;
                    // play the audio clip
                    audioSource.Play();
                    break;
                }
            }
        }
        
        public void StopAudio(string audioClipName)
        {
            // find the audio source with the audio clip "spinbutton"
            foreach (var audioSource in AudioSources)
            {
                if (audioSource.clip.name == audioClipName)
                {
                    audioSource.Stop();
                    break;
                }
            }
        }
    }
}