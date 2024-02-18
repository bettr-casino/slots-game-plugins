using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class BettrAudioController
    {
        private List<AudioSource> _audioSources = new List<AudioSource>();

        public BettrAudioController()
        {
            TileController.RegisterType<BettrAudioController>("BettrAudioController");
            TileController.AddToGlobals("BettrAudioController", this);
        }
        
        public void AddAudioSource(AudioSource audioSource)
        {
            _audioSources.Add(audioSource);
        }
        
        public void RemoveAudioSource(AudioSource audioSource)
        {
            _audioSources.Remove(audioSource);
        }
        
        public void SetVolume(float volume)
        {
            foreach (var audioSource in _audioSources)
            {
                audioSource.volume = volume;
            }
        }
        
        public void ToggleMusic(bool isMusicOff)
        {
            foreach (var source in _audioSources)
            {
                source.mute = isMusicOff;
            }
        }
    }
}