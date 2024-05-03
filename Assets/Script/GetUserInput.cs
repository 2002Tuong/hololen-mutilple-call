using Defective.JSON;
using Microsoft.MixedReality.WebRTC.Unity;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class GetUserInput : MonoBehaviour
{
    //Simulate user input by input on inspector
    [Header("User input")]
    public string username;
    public string roomId;
    public bool isPublisedVideo;
    public bool unPublisedVideo;
    public bool startJoinRoom;
    public bool leaveRoom;
    public bool muteAudio;
    public bool muteVideo;

    //for master
    [Header("for master")]
    public bool unpublisedAll;
    public bool muteAll;


    public WebSocketProcess webSocketProcess;
    public PeerConnection localPeer;
    public HandleResponse handleResponse;

    public GameObject videoSourceObject;
    public GameObject audioSourceObject;
    private void Update()
    { 
        if(startJoinRoom &&  localPeer.Peer.Initialized)
        {
            //Do S.T
            if(!roomId.IsNullOrEmpty())
            {
                webSocketProcess.Room = roomId;
            }
            //This will connect to server and then call joinRoom function
            webSocketProcess.StartConnect();
            startJoinRoom = false;
        }

        if(leaveRoom)
        {
            //Do S.T
            handleResponse.leaveRoom();
            leaveRoom = false;
        }

        if(isPublisedVideo)
        {
            //Do S.T
            isPublisedVideo = false;
            CloseOpenVoiceAndCam.instance.startStream();
            Session.instance.localParticipant.Signaler.setToStartConnection(true);
            
        }

        if(unPublisedVideo)
        {
            handleResponse.unpublishedVideo();
            unPublisedVideo=false;
        }

        if(muteAudio)
        {
            muteAudio = false;
            videoSourceObject.SetActive(!videoSourceObject.activeSelf);
            //var localUser = Session.instance.localParticipant;
            //handleResponse.changeStreamProperty(localUser.ConnectionId, localUser.StreamId, "audioActive", false, "publishAudio");
        }
        
        if(muteVideo)
        {
            muteVideo= false;
            audioSourceObject.SetActive(!audioSourceObject.activeSelf);
            //var localUser = Session.instance.localParticipant;
            //handleResponse.changeStreamProperty(localUser.ConnectionId, localUser.StreamId, "videoActive", false, "publishVideo");
        }

        if(PhotonNetwork.IsMasterClient)
        {
            if(unpublisedAll)
            {

            }

            if(muteAll)
            {

            }
        }
    }



}
