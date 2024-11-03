using System;
using System.Collections;
using System.Collections.Generic;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;
using UnityEngine.Video;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    // attached to Core MainScene
    public class BettrVideoPlayerController : MonoBehaviour
    {

        [NonSerialized] private VideoPlayer VideoPlayer;

        [NonSerialized] private bool isVideoPlaying;
        
        public static BettrVideoPlayerController Instance { get; private set; }
        
        private void Awake()
        {
            VideoPlayer = gameObject.GetComponent<VideoPlayer>();
            
            Instance = this;
        }

        private IEnumerator Start()
        {
            // preload Audio
            yield return BettrAudioController.Instance.LoadAudio("BettrVideo");
            
            // Set up the video player events
            VideoPlayer.prepareCompleted += OnVideoPrepared;
            VideoPlayer.loopPointReached += OnVideoEnded;

            // Ensure video is not playing initially
            VideoPlayer.Stop();
            // Prepare the video
            VideoPlayer.Prepare();
        }

        private void OnVideoPrepared(VideoPlayer _)
        {
            PlayAudioAndVideo();
        }

        private void OnVideoEnded(VideoPlayer _)
        {
            VideoPlayer.Stop();
            isVideoPlaying = false;
        }

        public void PlayAudioAndVideo()
        {
            if (isVideoPlaying) return;
            isVideoPlaying = true;
            VideoPlayer.Play();
            BettrAudioController.Instance.PlayAudioOnce("BettrVideo");
        }
    }
}