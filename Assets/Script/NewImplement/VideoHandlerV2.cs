using Microsoft.MixedReality.WebRTC.Unity;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoHandlerV2 : MonoBehaviourPunCallbacks
{
    public NodeDssSignaler nodeDss;
    public PhotonView _PhotonView;
    public WebcamSource _WebcamSource;
    public MicrophoneSource _MicrophoneSource;

    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("Call attach peerConnection");
        //nodeDss.PeerConnection = FindAnyObjectByType< Microsoft.MixedReality.WebRTC.Unity.PeerConnection>();
        _WebcamSource = FindAnyObjectByType<WebcamSource>();
        _MicrophoneSource = FindAnyObjectByType<MicrophoneSource>();
    }

    private void Start()
    {
        if (!_PhotonView.IsMine)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Destroy(gameObject);
            }
        }
    }

    [PunRPC]
    public void SetUpRemoteVideoInMaster(string playerLocalId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            nodeDss.RemotePeerId = playerLocalId;
            RemotePeerManager.CreateNewRemotePeer(nodeDss);
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("MasterId", out var masterId))
            {
                nodeDss.LocalPeerId = (string)masterId + RemotePeerManager.peers.Count;
            }

            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("IpAddress", out var ipAddress))
            {
                nodeDss.HttpServerAddress = (string)ipAddress;
            }


            var temp1 = nodeDss.PeerConnection.AddMediaLine(Microsoft.MixedReality.WebRTC.MediaKind.Video);
            temp1.Source = _WebcamSource;
            temp1.Receiver = GetComponentInChildren<VideoReceiver>();

            var temp2 = nodeDss.PeerConnection.AddMediaLine(Microsoft.MixedReality.WebRTC.MediaKind.Audio);
            temp2.Source = _MicrophoneSource;
            temp2.Receiver = GetComponentInChildren<AudioReceiver>();

            //nodeDss.PeerConnection.StartConnection();   
        } else if(!_PhotonView.IsMine)
        {

        }
    }

    public void SetUp(string playerLocalId)
    {
        PhotonNetwork.RemoveBufferedRPCs(photonView.ViewID, "SetUpRemoteVideoInMaster");
        photonView.RPC("SetUpRemoteVideoInMaster", RpcTarget.MasterClient, playerLocalId);
        PhotonNetwork.SendAllOutgoingCommands();
    }
}
