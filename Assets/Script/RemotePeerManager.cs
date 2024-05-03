using Microsoft.MixedReality.WebRTC.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePeerManager : MonoBehaviour
{
    public static List<RemotePeerObject> peers = new List<RemotePeerObject>();
    // Start is called before the first frame update

    public static void AddRemotePeer(RemotePeerObject peerObject)
    {
        peers.Add(peerObject);
    }

    public static void CreateNewRemotePeer(NodeDssSignaler signaler)
    {
        RemotePeerObject peerObject = new RemotePeerObject(signaler);
        AddRemotePeer(peerObject);
    }
}
