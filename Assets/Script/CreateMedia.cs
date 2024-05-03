using Microsoft.MixedReality.WebRTC.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMedia : MonoBehaviour
{
    // Start is called before the first frame update
    public WebcamSource camSource;
    public MicrophoneSource audioSource;
    public NodeDssSignaler nodeDss;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateMediaLine()
    {
        if(nodeDss == null)
        {
            nodeDss = FindAnyObjectByType<NodeDssSignaler>();
            if (nodeDss == null) return;
        }
        PeerConnection peer = nodeDss.PeerConnection;
        if (peer == null)
        {
            Debug.Log("Peer null");
        }
        var temp1 = peer.AddMediaLine(Microsoft.MixedReality.WebRTC.MediaKind.Video);
        if (temp1 == null)
        {
            Debug.Log("Cannot create MediaLine");
        }
        temp1.Source = camSource;
        temp1.Receiver = FindAnyObjectByType<VideoReceiver>();

        var temp2 = peer.AddMediaLine(Microsoft.MixedReality.WebRTC.MediaKind.Audio);
        temp2.Source = audioSource;
        temp2.Receiver =FindAnyObjectByType<AudioReceiver>();
    }
}
