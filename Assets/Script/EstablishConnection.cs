using Microsoft.MixedReality.WebRTC.Unity;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EstablishConnection : MonoBehaviour
{
    public NodeDssSignaler nodeDss;
    public PhotonView photonView;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartDelay()
    {
        Invoke("StartConnection", 1.5f);
    }

    public void StartConnection()
    {
        if (PhotonNetwork.IsMasterClient || !photonView.IsMine) return;
        if(nodeDss.PeerConnection.Peer == null)
        {
            Debug.Log("PeerConnection not init");
            return;
        }

        Debug.Log("call start connection");
        nodeDss.PeerConnection.StartConnection();
    }

    public void StartConnectionWithNoCondition()
    {
        Debug.Log("Reconnection");
        nodeDss.PeerConnection.StartConnection();
    }
}
