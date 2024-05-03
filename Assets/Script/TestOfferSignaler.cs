using Microsoft.MixedReality.WebRTC.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestOfferSignaler : MonoBehaviour
{
    public TMP_Text remoteId;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        NodeDssSignaler signaler = FindAnyObjectByType<NodeDssSignaler>();
        //Debug.Log("call is not null: " + signaler != null);
        //Debug.Log("remote name " + signaler.RemotePeerId);
        if (signaler != null)
        {
            //Debug.Log(signaler.RemotePeerId);
            remoteId.text = signaler.RemotePeerId;
        }
    }

    public void CreateOffer()
    {
        Debug.Log("call click");
        NodeDssSignaler signaler = FindAnyObjectByType<NodeDssSignaler>();
        if(signaler != null)
        {
            Debug.Log("remote:" + signaler.RemotePeerId);
            bool res = signaler.PeerConnection.StartConnection();
            Debug.Log("create offer result : " + res);
        }
    }
}
