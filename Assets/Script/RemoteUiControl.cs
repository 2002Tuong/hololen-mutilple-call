using Microsoft.MixedReality.WebRTC.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteUiControl : MonoBehaviour
{
    public NodeDssSignaler nodeDss;
    public TextMesh remoteName;
    // Start is called before the first frame update
    void Start()
    {
        remoteName.text = nodeDss.RemotePeerId;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
