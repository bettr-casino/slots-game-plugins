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

        private float waitTime = 15.0f;

        [NonSerialized] private VideoPlayer VideoPlayer;
        
        private void Awake()
        {
            VideoPlayer = gameObject.GetComponent<VideoPlayer>();
        }

        private IEnumerator Start()
        {
            // Set up the video player events
            VideoPlayer.prepareCompleted += OnVideoPrepared;
            VideoPlayer.loopPointReached += OnVideoEnded;

            // Ensure video is not playing initially
            VideoPlayer.Stop();
            // Prepare the video
            VideoPlayer.Prepare();

            yield return new WaitForSeconds(waitTime);

            while (true)
            {
                if (!gameObject.activeInHierarchy)
                {
                    yield return null;
                }
                
                VideoPlayer.Play();
                
                yield return new WaitForSeconds(waitTime);
            }
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