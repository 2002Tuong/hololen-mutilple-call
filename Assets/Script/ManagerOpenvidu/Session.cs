using Microsoft.MixedReality.WebRTC.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Session : MonoBehaviour
{
    public PeerConnection localPeer;
    public OpenViduSignaler localSignaler;

    public GameObject prefabRemote;
    public static Session instance;
    public WebSocketProcess webSocketProcess;

    public Participant localParticipant;
    public Dictionary<string, Participant> remoteParticipants = new Dictionary<string, Participant>();
    private string sessionId;
    private string token;
    public string hololensDeviceId = "";
    public string roomDemoId = "";
    private List<ConfigurableIceServer> iceServersDefault = new List<ConfigurableIceServer>()
    {
        new ConfigurableIceServer()
        {
            Type = IceType.Stun,
            Uri = "stun.l.google.com:19302"
        },
        new ConfigurableIceServer(){ 
            Type = IceType.Turn,
            Uri = "relay1.expressturn.com:3478"
        }
    };

    private string defaultUsername = "ef69HDY3PDLB9RSIKE";
    private string defaultPassword = "6JcxrpKwgM6oBRKy";
    public List<ConfigurableIceServer> iceServers = new List<ConfigurableIceServer>();
    private string username = "";
    private string password = "";
  

    private void Start()
    {
        instance = this;
    }

    public string SessionId => sessionId;
    public void setSessionId(string sessionId)
    {
        this.sessionId = sessionId;
    }
    public string Token => token;  
    public void setToken(string token)
    {
        this.token = token;
    }

    
    public void CreateLocalParticipant(string connectionId, string name)
    {
        localParticipant = new Participant(connectionId, name);
        localParticipant.setPeerConnection(localPeer);
        localParticipant.setSignaler(localSignaler);
    }

    public void CreateRemoteParticipant(string connectionId, string name)
    {
        //name is sett in join room call
        //if hololens device not create 
        // return;
        if(name == "hololens")
        {
            hololensDeviceId = connectionId;
            return;
        }

        if(name == "room_demo")
        {
            roomDemoId = connectionId;
        }
        var spawnPosition = PositionPool.pool[remoteParticipants.Count];
        var spawnRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        GameObject remoteParticipant =  Instantiate(prefabRemote, spawnPosition, spawnRotation);
        var peerConnection = remoteParticipant.GetComponentInChildren<PeerConnection>();
        var signaler = remoteParticipant.GetComponentInChildren<OpenViduSignaler>();

        if(iceServers.Count != 0)
        {
            peerConnection.IceServers = iceServers;
            peerConnection.IceUsername = username;
            peerConnection.IceCredential = password;
        }
        
        var remoteUser = new Participant(connectionId, name);
        remoteUser.setPeerConnection(peerConnection);
        remoteUser.setSignaler(signaler);
        remoteUser.setRemoteView(remoteParticipant);
        remoteParticipants.Add(connectionId, remoteUser);
    }

    public Participant FindRemoteWithConnectionId(string connectionId)
    {
        foreach(var item in remoteParticipants)
        {
            if(item.Key == connectionId)
            {
                return item.Value;
            }
        }
        return null;
    }
    
    public bool RemoveParticipantWithConnectionId(string connectionId)
    {
        try
        {
            var remoteParticipant = remoteParticipants[connectionId];
            remoteParticipants.Remove(connectionId);

            remoteParticipant.dispose();
            return true;
        }catch (Exception e)
        {
            Debug.LogError("Not exist remote user");
            return false;
        }
            
    }


    public void setIceServers(List<ConfigurableIceServer> iceServers)
    {
        this.iceServers = iceServers;
    }

    public void setUsername(string username)
    {
        this.username = username;
    }

    public void setPassword( string password)
    {
        this.password = password;
    }
}
