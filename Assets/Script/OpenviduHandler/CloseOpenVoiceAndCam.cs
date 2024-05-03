using Microsoft.MixedReality.WebRTC.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseOpenVoiceAndCam : MonoBehaviour
{
    public static CloseOpenVoiceAndCam instance;

    //Handle unpublished video
    public WebcamSource camSource;
    public MicrophoneSource micSource;
    public GameObject offScreen;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void stopStream()
    {
        camSource.enabled = false;
        micSource.enabled = false;
        offScreen.SetActive(true);
    }

    public void startStream()
    {
        camSource.enabled = true;
        micSource.enabled = true;
        offScreen.SetActive(false);
    }
}
