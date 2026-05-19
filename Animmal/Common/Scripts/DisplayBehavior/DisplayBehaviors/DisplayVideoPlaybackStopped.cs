using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace Animmal
{
    [AddComponentMenu("Animmal/DisplayBehavior/Hide on Video Playback Stopped")]
    public class DisplayVideoPlaybackStopped : DisplayBehaviorBase
    {
        public VideoPlayer VideoPlayer;
        IEnumerator VideoPlaybackTrackingCoroutine;


        protected override void ShowItem(bool _Instant)
        {
            base.ShowItem(_Instant);
            if (VideoPlayer == null)
            {
                VideoPlayer = GetComponent<VideoPlayer>();
                if (VideoPlayer == null)
                {
                    Debug.LogError("Ultimate Notification System - DisplayVideoPlaybackStopped - Video Player Missing on gameobject: " + gameObject.name);
                    return;
                }
            }
            VideoPlaybackTrackingCoroutine = TrackAudioPlayback();
            StartCoroutine(VideoPlaybackTrackingCoroutine);
        }

        protected override void HideItem(bool _Instant)
        {
            base.HideItem(_Instant);
            if (VideoPlaybackTrackingCoroutine != null)
                StopCoroutine(VideoPlaybackTrackingCoroutine);
        }

        IEnumerator TrackAudioPlayback()
        {
            yield return WaitForEndOfFrame;
            while (VideoPlayer.isPlaying)
            {
                yield return WaitForEndOfFrame;
            }
            StartCoroutine(HidingFinishedDelayed());
        }

    }
}