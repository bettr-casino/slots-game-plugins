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

        private void Awake()
        {
            VideoPlayer = gameObject.GetComponent<VideoPlayer>();
        }

        void Start()
        {
            // Set up the video player events
            VideoPlayer.prepareCompleted += OnVideoPrepared;
            VideoPlayer.loopPointReached += OnVideoEnded;

            // Ensure video is not playing initially
            VideoPlayer.Stop();
            // Prepare the video
            VideoPlayer.Prepare();
        }

        private void OnVideoPrepared(VideoPlayer vp)
        {
            // Play the video
            VideoPlayer.Play();
        }

        private void OnVideoEnded(VideoPlayer vp)
        {
            // Stop the video
            VideoPlayer.Stop();
        }
    }
}