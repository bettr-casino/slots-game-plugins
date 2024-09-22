using System;
using CrayonScript.Code;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class BettrAudioController
    {
        public bool IsVolumeMuted { get; set; }

        public BettrAudioController()
        {
            TileController.RegisterType<BettrAudioController>("BettrAudioController");
            TileController.AddToGlobals("BettrAudioController", this);
        }

        public void PlayAudioOnce(GameObject gameObjectWithAudioSource, string audioClipName)
        {
            // get the audio source components
            var audioSources = gameObjectWithAudioSource.GetComponents<AudioSource>();
            // find the audio source with the audio clip "spinbutton"
            foreach (var audioSource in audioSources)
            {
                if (audioSource.clip.name == audioClipName)
                {
                    audioSource.loop = false;
                    // play the audio clip
                    audioSource.Play();
                    break;
                }
            }
        }
        
        public void PlayAudioLoop(GameObject gameObjectWithAudioSource, string audioClipName)
        {
            // get the audio source components
            var audioSources = gameObjectWithAudioSource.GetComponents<AudioSource>();
            // find the audio source with the audio clip "spinbutton"
            foreach (var audioSource in audioSources)
            {
                if (audioSource.clip.name == audioClipName)
                {
                    audioSource.loop = true;
                    // play the audio clip
                    audioSource.Play();
                    break;
                }
            }
        }
        
        public void StopAudio(GameObject gameObjectWithAudioSource, string audioClipName)
        {
            // get the audio source components
            var audioSources = gameObjectWithAudioSource.GetComponents<AudioSource>();
            // find the audio source with the audio clip "spinbutton"
            foreach (var audioSource in audioSources)
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