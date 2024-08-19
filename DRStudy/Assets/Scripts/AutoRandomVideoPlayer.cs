using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.Video;

public class AutoRandomVideoPlayer : MonoBehaviour
{
    #region PUBLIC FIELDS
    /// <summary>
    /// Array of <see cref="VideoClip"/> objects that correspond to all clips in the <see href="https://www.nature.com/articles/s41597-020-0366-1">CAAV</see> dataset.
    /// </summary>
    public VideoClip[] videoClips;
    #endregion

    #region PRIVATE FIELDS
    /// <summary>
    /// The VideoPlayer component that displays videos on the current GameObject.
    /// </summary>
    private VideoPlayer videoPlayer;
    #endregion

    #region CONSTANTS
    /// <summary>
    /// Minimum waiting time between the end of the previous clip and the beginning of the next one, expressed in seconds.
    /// </summary>
    private const float MIN_WAITING_TIME = 1.0f;
    /// <summary>
    /// Maximum waiting time between the end of the previous clip and the beginning of the next one, expressed in seconds.
    /// </summary>
    private const float MAX_WAITING_TIME = 5.0f;

    /// <summary>
    /// Directory in which the CAAV dataset clip files are stored.
    /// </summary>
    private static string BASE_CLIPS_DIRECTORY;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        BASE_CLIPS_DIRECTORY = Path.Combine(Application.streamingAssetsPath, "CAAV_mp4");
        Debug.Log($"[DEBUG] Main clips directory set to {BASE_CLIPS_DIRECTORY}");

        // Read all files from caav dataset folder
        Debug.Log($"[DEBUG] Getting clip files from directory {BASE_CLIPS_DIRECTORY}");

        if (videoClips == null)
            videoClips = new VideoClip[0];

        Debug.Log($"[DEBUG] Detected {videoClips.Length} .mp4 files");

        Debug.Log($"[DEBUG] Obtaining video player");
        videoPlayer = gameObject.GetComponent<VideoPlayer>();
        Debug.Log($"[DEBUG] Obtained video player {videoPlayer}");
        if (videoPlayer != null)
        {
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.Prepare();
            videoPlayer.loopPointReached += ClipEnded;
            Debug.Log("[DEBUG] Initializing new timer");
            InitializeTimer();
        }
    }

    //private void InitializeClips()
    //{
    //    string[] videoFilePaths = Directory.GetFiles(BASE_CLIPS_DIRECTORY, "*.mp4");
    //    clipFiles = new VideoClip[videoFilePaths.Length];

    //    for (int i = 0; i < clipFiles.Length; i++)
    //    {
    //        clipFiles[i] = LoadClip(videoFilePaths[i]);
    //    }
    //}

    //private VideoClip LoadClip(string videoFilePath)
    //{
    //    VideoClip videoClip = null;

    //    UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(videoFilePath);
    //    www.SendWebRequest();
    //    if (www.result != UnityWebRequest.Result.Success)
    //    {
    //        Debug.LogError($"File loading error: {www.error}");
    //        return null;
    //    }
    //    AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(www);
    //    videoClip = assetBundle.LoadAsset<VideoClip>(Path.GetFileNameWithoutExtension(videoFilePath));
    //    assetBundle.Unload(false);

    //    return videoClip;
    //}

    private void ClipEnded(VideoPlayer source)
    {
        if (!videoPlayer.isPrepared || !videoPlayer.isPlaying)
        {
            Debug.Log("[DEBUG] Initializing new timer");
            InitializeTimer();
        }
    }

    private void InitializeTimer()
    {
        float waitingTime = UnityEngine.Random.Range(MIN_WAITING_TIME, MAX_WAITING_TIME);
        Debug.Log($"[DEBUG] New timer set to {waitingTime} seconds");
        Invoke(nameof(OnTimerElapsed), waitingTime);
    }

    private void OnTimerElapsed()
    {
        Debug.Log($"[DEBUG] Timer elapsed");
        VideoClip randClip = PickRandomClip();
        Debug.Log($"[DEBUG] Found clip {randClip}");
        videoPlayer.clip = randClip;
        Debug.Log($"[DEBUG] Attempting to play clip");
        videoPlayer.Play();
    }

    private VideoClip PickRandomClip()
    {
        // Get index of clip to play at random from all clips available
        int clipIndex = UnityEngine.Random.Range(0, videoClips.Length);
        Debug.Log($"[DEBUG] Picking {clipIndex}-th clip from the dataset");

        // Get video clip's filename by index of file in folder
        VideoClip newClip = videoClips[clipIndex];
        Debug.Log($"[DEBUG] {clipIndex}-th clip from the dataset corresponds to {newClip}");

        return newClip;
    }
}
