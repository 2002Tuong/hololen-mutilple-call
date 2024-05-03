using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceRemoteMedia : MonoBehaviour
{
    public GameObject instanceObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void InstanceObject()
    {
        var spawnPosition = new Vector3(0, 0, -1);
        var spawnRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        var client = Instantiate(instanceObject);

        //var videoHandler = client.GetComponent<VideoHandler>();
        //videoHandler.nodeDss.LocalPeerId = "pc2";
        //videoHandler.nodeDss.RemotePeerId = "pc1";
        //videoHandler.nodeDss.HttpServerAddress = "http://192.168.1.20:3000/";

    }
}
