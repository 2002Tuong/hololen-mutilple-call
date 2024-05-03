using Microsoft.MixedReality.WebRTC;
using Microsoft.MixedReality.WebRTC.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Participant 
{
    protected string connectionId;
    protected string streamId;
    protected string name="";
    protected List<IceCandidate> iceCandidateList = new List<IceCandidate>();
    protected Microsoft.MixedReality.WebRTC.Unity.PeerConnection peerConnection;
    protected OpenViduSignaler signaler;
    protected GameObject remoteView;

    
    public Participant(string connectionId, string name)
    {
        this.connectionId = connectionId;
        this.name = name;
    }

    public string ConnectionId => connectionId;
    public string Name => name;
    public string StreamId => streamId;
    public Microsoft.MixedReality.WebRTC.Unity.PeerConnection PeerConnection => peerConnection;
    public OpenViduSignaler Signaler => signaler;
    public void setPeerConnection(Microsoft.MixedReality.WebRTC.Unity.PeerConnection peerConnection)
    {
        this.peerConnection = peerConnection;
    }

    public void setSignaler(OpenViduSignaler signaler)
    {
        this.signaler = signaler;
    }

    public void setRemoteView(GameObject participant)
    {
        this.remoteView = participant;
    }

    public void setStreamId(string streamId)
    {
        this.streamId = streamId;
    }
    public GameObject getAudioObject() 
    {
        return remoteView.transform.GetChild(0).gameObject;
    }

    public GameObject getVideoRender()
    {
        return remoteView.transform.GetChild(1).gameObject;
    }

    public void dispose()
    {
        Object.Destroy(remoteView);
    }
}
