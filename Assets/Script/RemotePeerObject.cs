using Microsoft.MixedReality.WebRTC.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePeerObject
{
    private NodeDssSignaler _signaler;
    public string remotePeerId { get; private set; }

    public RemotePeerObject(NodeDssSignaler signaler)
    {
        _signaler = signaler;
        this.remotePeerId = _signaler.RemotePeerId;
    }
}
