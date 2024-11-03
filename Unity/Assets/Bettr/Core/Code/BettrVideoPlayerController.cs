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

        private float delayBetweenLoops = 90.0f;
        private bool loop = true;

        [NonSerialized] private VideoPlayer VideoPlayer;
        
        private void Awake()
        {
            VideoPlayer = gameObject.GetComponent<VideoPlayer>();
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

            yield return new WaitForSeconds(delayBetweenLoops);
            
            if (!loop) yield break;

            while (true)
            {
                if (!loop) yield break;
                
                if (!gameObject.activeInHierarchy)
                {
                    yield return null;
                }
                
                PlayAudioAndVideo(VideoPlayer);
                
                yield return new WaitForSeconds(delayBetweenLoops);
            }
        }

        private void OnVideoPrepared(VideoPlayer vp)
        {
            PlayAudioAndVideo(vp);
        }

        private void OnVideoEnded(VideoPlayer vp)
        {
            vp.Stop();
        }

        private void PlayAudioAndVideo(VideoPlayer vp)
        {
            vp.Play();
            BettrAudioController.Instance.PlayAudioOnce("BettrVideo");
        }
    }
}