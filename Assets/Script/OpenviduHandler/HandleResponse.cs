using Defective.JSON;
using Microsoft.MixedReality.WebRTC;
using Microsoft.MixedReality.WebRTC.Unity;
using Photon.Pun;
using PimDeWitte.UnityMainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
/*
 * Current Logic: the PC user will only connect to Hololen user and not connect each other
 * Hololen user will be one create room and is master
 * Not need to check user already in Room
 */


public class HandleResponse : MonoBehaviour
{

    public WebSocketProcess webSocketProcess;
    public string name = "test";
    public string mediaServer = "mediasoup";
    private int RPC_ID = 0;
    private int ID_PING = -1;
    private int ID_JOINROOM = -1;
    private int ID_LEAVEROOM = -1;
    private int ID_PUBLISHVIDEO = -1;
    private int ID_SENDMESSAGE = -1;
    private int ID_UNPUBLISHVIDEO = -1;
    private int ID_CHANGESTREAMPROPERTY = -1;
    private Dictionary<int, (string, string)> IDS_PREPARERECEIVEVIDEO = new Dictionary<int, (string, string)>();
    private Dictionary<int, string> IDS_IDS_RECEIVEVIDEO = new Dictionary<int, string>();
    private HashSet<int> IDS_ONICECANDIDATE = new HashSet<int>();
    // Start is called before the first frame update

    public void HandleServerResponse(JSONObject json)
    {
        int rpcId = json.GetField(JsonConstant.ID).intValue;
        JSONObject result = json.GetField(JsonConstant.RESULT);
        Debug.Log("rpcId: " + rpcId);
        
        if(result.HasField("value") && result.GetField("value").stringValue == "pong")
        {
            // Response to ping
            Debug.Log("Pong");
        } else if (rpcId == this.ID_JOINROOM) 
        {
            // Response to joinRoom
            Debug.Log(" <color=cyan> JoinRoom Call </color>");
            string connectionId = result.GetField(JsonConstant.ID).stringValue;
            Debug.Log("ID: " + connectionId);
            Debug.Log("Media server: " + result.GetField(JsonConstant.MEDIA_SERVER).stringValue);
            mediaServer = result.GetField(JsonConstant.MEDIA_SERVER).stringValue;

            Session.instance.CreateLocalParticipant(connectionId, "");
            Session.instance.localParticipant.Signaler.LocalPeerId = connectionId;
            if (PhotonNetwork.IsMasterClient)
            {
                ExitGames.Client.Photon.Hashtable masterConnectionId = new ExitGames.Client.Photon.Hashtable();
                masterConnectionId.Add("ConnectionId", connectionId);
                PhotonNetwork.CurrentRoom.SetCustomProperties(masterConnectionId);
            }
            //handle ice server
            string username = "";
            string password = "";
            username = result.GetField("turnUsername").stringValue;
            password = result.GetField("turnCredential").stringValue;
            if(result.HasField(JsonConstant.ICE_SERVERS))
            {
                var jsonIceServers = result.GetField(JsonConstant.ICE_SERVERS).list;
                List<ConfigurableIceServer> iceServers = new List<ConfigurableIceServer>();
                for (int i = 0; i < jsonIceServers.Count; i++)
                {
                    JSONObject jsonIceServer = jsonIceServers[i];
                    string url = "";
                    if(jsonIceServer.HasField("urls"))
                    {
                        var jsonUrls = jsonIceServer.GetField("urls").list;
                        url = jsonUrls[0].stringValue;
                    }

                    if(jsonIceServer.HasField("url"))
                    {
                       url = jsonIceServer.GetField("url").stringValue;   
                    }

                    int indexFirstColon = url.IndexOf(':');
                    if(indexFirstColon != -1)
                    {
                        string type = url.Substring(0, indexFirstColon);
                        string urlHasNotType = url.Substring(indexFirstColon + 1);

                        Debug.Log("type: " + type);
                        Debug.Log("url: " + urlHasNotType);
                        if(type.ToLower().Equals("turn"))
                        {
                            iceServers.Add(new ConfigurableIceServer()
                            {
                                Type = IceType.Turn,
                                Uri = urlHasNotType,
                            });
                        }else
                        {
                            iceServers.Add(new ConfigurableIceServer()
                            {
                                Type = IceType.Stun,
                                Uri = urlHasNotType,
                            });
                        }
                    }
                }

                Session.instance.setIceServers(iceServers);
                Session.instance.setUsername(username);
                Session.instance.setPassword(password);
            }


            //try
            //{
            //    Debug.Log("User already in room: " + result.GetField(JsonConstant.VALUE).list.Count);
            //    if (result.GetField(JsonConstant.VALUE).list.Count > 0)
            //    {
            //        addRemoteParticipantsAlreadyInRoom(result);
            //    }
            //} catch(Exception e)
            //{
            //    Debug.Log("<color=red> value is empty</color>: "+ e.Message );
            //}


        } else if(rpcId == this.ID_LEAVEROOM)
        {
            Debug.Log(" <color=cyan> LeaveRoom Call </color>");
            // Response to leaveRoom
            //disconnect WebSocket
            webSocketProcess.webSocket.Close();

        } else if(rpcId==this.ID_PUBLISHVIDEO)
        {
            Debug.Log(" <color=cyan> PublishVideo Call  </color>");
            SdpMessage sdpAnswer = new SdpMessage { Type = SdpMessageType.Answer, Content = result.GetField("sdpAnswer").stringValue };
            Session.instance.localParticipant.setStreamId(result.GetField("id").stringValue);
            Session.instance.localParticipant.Signaler.handleMessageAsync(() =>
            {
                Session.instance.localParticipant.PeerConnection.HandleConnectionMessageAsync(sdpAnswer);
            });

        } else if(IDS_PREPARERECEIVEVIDEO.ContainsKey(rpcId))
        {
            Debug.Log(" <color=cyan> PREPARERECEIVEVIDEO Call  </color>");
            // Response to prepareReceiveVideoFrom
            var participantAndStream = IDS_PREPARERECEIVEVIDEO[rpcId];
            IDS_PREPARERECEIVEVIDEO.Remove(rpcId);
            string remotePeerId = participantAndStream.Item1;
            string streamId = participantAndStream.Item2;
            Participant remoteUser = Session.instance.FindRemoteWithConnectionId(remotePeerId);
            SdpMessage remoteSdpOffer = new SdpMessage { Type = SdpMessageType.Offer, Content = result.GetField("sdpOffer").stringValue };
            if (remoteUser == null) return;

            remoteUser.Signaler.handleMessageAsync(() => {
                remoteUser.PeerConnection.HandleConnectionMessageAsync(remoteSdpOffer).ContinueWith(_ =>
                {
                    var res = remoteUser.Signaler.createAnswer();
                    Debug.Log("User with id: " + remoteUser.ConnectionId + " create answer return: " + res);
                    remoteUser.Signaler.setRemotePeerId(remotePeerId);
                    remoteUser.Signaler.setRemoteStreamId(streamId);
                });

            });

        } else if(this.IDS_IDS_RECEIVEVIDEO.ContainsKey(rpcId))
        {
            //// Response to receiveVideoFrom
            Debug.Log(" <color=cyan> RECEIVEVIDEO Call  </color>");
            try
            {
                string remoteConnectionId = IDS_IDS_RECEIVEVIDEO[rpcId];
                //IDS_IDS_RECEIVEVIDEO.Remove(rpcId);
                if(!mediaServer.Equals("kurento"))
                {
                    return;
                }
                Participant remoteUser = Session.instance.FindRemoteWithConnectionId(remoteConnectionId);
                SdpMessage sdpAnswerReceiveVideo = new SdpMessage { Type = SdpMessageType.Answer, Content = result.GetField("sdpAnswer").stringValue };
                if (remoteUser == null)
                {
                    Debug.Log("remote user null");
                    return;
                }
                remoteUser.Signaler.handleMessageAsync(() =>
                {
                    remoteUser.PeerConnection.HandleConnectionMessageAsync(sdpAnswerReceiveVideo);
                });
            } catch(Exception e)
            {
                Debug.LogError("Receivei video error: " + e.Message);
            }
        }
        else if(IDS_ONICECANDIDATE.Contains(rpcId))
        {
            // Response to onIceCandidate
            IDS_ONICECANDIDATE.Remove(rpcId);
        }
        else if(rpcId == ID_SENDMESSAGE)
        {
            Debug.Log(" <color=cyan> SENDMESSAGE Call  </color>");
            ID_SENDMESSAGE = -1;
        }
        else if(rpcId == this.ID_UNPUBLISHVIDEO)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {

                CloseOpenVoiceAndCam.instance.stopStream();
            });
        } else if(rpcId == ID_CHANGESTREAMPROPERTY)
        {

        }
        else
        {
            Debug.Log("\"Unrecognized server response: \"" + result.ToString());
        }
    }

    public void HandleServerError(JSONObject json)
    {
        JSONObject error = json.GetField(JsonConstant.ERROR);
        int errorCode = error.GetField("code").intValue;
        string errorMessage = error.GetField("message").stringValue;
        Debug.Log("Server error code " + errorCode + " : " + errorMessage);
    }

    public void HandleServerEvent(JSONObject json)
    {
        if(!json.HasField(JsonConstant.METHOD))
        {
            Debug.Log("Server event lack a field " + JsonConstant.METHOD + "; JSON: " + json.ToString());
            return;
        }
        string method = json.GetField(JsonConstant.METHOD).stringValue;

        if (!json.HasField(JsonConstant.PARAMS))
        {
            Debug.Log("Server event " + method + " lack a field " + JsonConstant.PARAMS + "; JSON: " + json.ToString());
            return;
        }

        JSONObject jsonParams = json.GetField(JsonConstant.PARAMS);

        switch(method)
        {
            case JsonConstant.ICE_CANDIDATE:
                Debug.Log("<color=cyan> iceCandidate Event </color>");
                iceCandidateEvent(jsonParams);
                break;
            case JsonConstant.PARTICIPANT_JOINED:
                Debug.Log("<color=cyan> participant joined Event </color>");
                participantJoinedEvent(jsonParams);
                break;
            case JsonConstant.PARTICIPANT_PUBLISHED:
                Debug.Log("<color=cyan> participant publish Event </color>");
                participantPublishedEvent(jsonParams);
                break;
            case JsonConstant.PARTICIPANT_LEFT:
                Debug.Log("<color=cyan> participant left Event </color>");
                participantLeftEvent(jsonParams);
                break;
            case JsonConstant.PARTICIPANT_EVICTED:
                Debug.Log("<color=cyan> participant evicted Event </color>");
                participantLeftEvent(jsonParams);
                break;
            case JsonConstant.SEND_MESSAGE:
                Debug.Log("<color=cyan> send message Event </color>");
                sendMessageEvent(jsonParams);
                break;
            case JsonConstant.UNPUBLISHVIDEO_METHOD:
                Debug.Log("<color=cyan> participant unpublish Event </color>");
                participanUnpublisedEvent(jsonParams);
                break;
            case JsonConstant.STREAM_PROPERTY_CHANGED:
                streamPropertyChanged(jsonParams);
                break;
            default:
                throw new System.Exception("Unknown server event '" + method + "'");
        }
    }
    public int sendJson(String method)
    {
        return this.sendJson(method, new Dictionary<string, string>());
    }

    public int sendJson(string method, Dictionary<string, string> param)
    {
        int id = RPC_ID++;
        JSONObject jsonObject = new JSONObject();
        JSONObject jsonParams = new JSONObject();
        foreach(var item in param) {
            jsonParams.AddField(item.Key, item.Value);
        }

        if(jsonParams.type == JSONObject.Type.Null)
        {
            jsonParams = JSONObject.emptyObject;
        }

        jsonObject.AddField("jsonrpc", JsonConstant.JSON_RPCVERSION);
        jsonObject.AddField("method", method);
        jsonObject.AddField("id", id);
        jsonObject.AddField("params", jsonParams);

        var stringJsonObject = jsonObject.ToString();
        Debug.Log("send data: " + stringJsonObject);
        webSocketProcess.getWebSocket().SendText(stringJsonObject);
        return id;
    }

    public void ping()
    {
        Dictionary<string, string> paramsJson = new Dictionary<string, string>();
        if(/*ID_PING == -1*/true)
        {
            paramsJson.Add("interval", "5000");
        }

        ID_PING = sendJson(JsonConstant.PING_METHOD, paramsJson);
    }

    public void joinRoom()
    {
#if UNITY_WSA || UNITY_WSA_10_0
        name = "hololens";
#elif UNITY_EDITOR
        name = "room_demo";
#endif
        Dictionary<string, string> joinRoomParams = new Dictionary<string, string>();
        joinRoomParams.Add(JsonConstant.METADATA, name);
        joinRoomParams.Add("secret", "");
        joinRoomParams.Add("session", webSocketProcess.Room);
        joinRoomParams.Add("recorder", "true");
        joinRoomParams.Add("platform", "Unity " + Application.version);
        joinRoomParams.Add("token", webSocketProcess.getToken());
        joinRoomParams.Add("sdkVersion", JsonConstant.SDK_VERSION);
        ID_JOINROOM = sendJson(JsonConstant.JOINROOM_METHOD, joinRoomParams);
        Debug.Log("Method: " + JsonConstant.JOINROOM_METHOD + " ID: " + ID_JOINROOM);
    }

    public void leaveRoom()
    {
        this.ID_LEAVEROOM = this.sendJson(JsonConstant.LEAVEROOM_METHOD);
        Debug.Log("Method: " + JsonConstant.LEAVEROOM_METHOD + " ID: " + ID_LEAVEROOM);
    }

    public void publishVideo(SdpMessage message)
    {
        Dictionary<string, string> publishVideoParams = new Dictionary<string, string>();
        publishVideoParams.Add("audioActive", "true");
        publishVideoParams.Add("videoActive", "true");
        publishVideoParams.Add("doLoopback", "false");
        publishVideoParams.Add("frameRate", "30");
        publishVideoParams.Add("hasAudio", "true");
        publishVideoParams.Add("hasVideo", "true");
        publishVideoParams.Add("typeOfVideo", "CAMERA");
        publishVideoParams.Add("videoDimensions", "{width:320, height:240}");
        publishVideoParams.Add("sdpOffer", message.Content);
        ID_PUBLISHVIDEO = sendJson(JsonConstant.PUBLISHVIDEO_METHOD, publishVideoParams);

        Debug.Log("Method: " + JsonConstant.PUBLISHVIDEO_METHOD + " ID: " + ID_PUBLISHVIDEO);
    }

    public void prepareReceiveVideoFrom(String remotePeerId, String streamId)
    {
        Dictionary<String, String> prepareReceiveVideoFromParams = new Dictionary<String, String>();
        prepareReceiveVideoFromParams.Add("sender", streamId);
        prepareReceiveVideoFromParams.Add("reconnect", "false");
        IDS_PREPARERECEIVEVIDEO.Add(sendJson(JsonConstant.PREPARERECEIVEVIDEO_METHOD, prepareReceiveVideoFromParams), (remotePeerId, streamId));
    }

    public void receiveVideoFrom(SdpMessage message,String remotePeerId, String streamId)
    {
        Dictionary<string, string> receiveVideoFromParams = new Dictionary<string, string>();
        receiveVideoFromParams.Add("sender", streamId);

        if(mediaServer.Equals("kurento"))
        {
            receiveVideoFromParams.Add("sdpOffer", message.Content);
        } else
        {
            receiveVideoFromParams.Add("sdpAnswer",message.Content);
        }

        IDS_IDS_RECEIVEVIDEO.Add(sendJson(JsonConstant.RECEIVEVIDEO_METHOD, receiveVideoFromParams), remotePeerId);
    }

    public void onIceCandidate(IceCandidate iceCandidate, string endpointName)
    {
        Dictionary<string, string> onIceCandidateParams = new Dictionary<string, string>();
        if (endpointName != null)
        {
            onIceCandidateParams.Add("endpointName", endpointName);
        }

        onIceCandidateParams.Add("candidate", iceCandidate.Content);
        onIceCandidateParams.Add("sdpMid", iceCandidate.SdpMid);
        onIceCandidateParams.Add("sdpMLineIndex", iceCandidate.SdpMlineIndex.ToString());
        IDS_ONICECANDIDATE.Add(sendJson(JsonConstant.ONICECANDIDATE_METHOD, onIceCandidateParams));
    }

    public void sendMessage(string message, List<string> connectionId = null) 
    {
        Debug.Log("<color=orange>Send Message</color>");
        Debug.Log(message);
        string toUser = "[]";
        //if(connectionId != null)
        //{
        //     toUser += "[";
        //    foreach (string id in connectionId)
        //    {
        //        toUser += id;
        //        toUser += ",";
        //    }
        //    toUser += "]";
        //}
        //if(toUser.IsNullOrEmpty())
        //{
        //    toUser = "[]";
        //}
        Dictionary<string, string> sendMessageParams = new Dictionary<string, string>();
        string temp = "{" + "\"to\":"+toUser+"," +
       "\"data\":\""+message+"\"," +
       "\"type\":\"signal:chat\"}";
        sendMessageParams.Add("message",  temp);
   
        ID_SENDMESSAGE = sendJson(JsonConstant.SENDMESSAGE_ROOM_METHOD, sendMessageParams);
    }

    public void unpublishedVideo()
    {
        ID_UNPUBLISHVIDEO=  sendJson(JsonConstant.UNPUBLISHVIDEO_METHOD);
        Debug.Log("Method: " + JsonConstant.UNPUBLISHVIDEO_METHOD + " ID: " + ID_UNPUBLISHVIDEO);
    }

    public void changeStreamProperty(String connectionId, String streamId, String property, bool newValue, string reason)
    {
        Dictionary<string, string> streamPropertyParams = new Dictionary<string, string>();
        streamPropertyParams.Add("connectionId", connectionId);
        streamPropertyParams.Add("newValue", newValue.ToString().ToLower());
        streamPropertyParams.Add("property", property);
        streamPropertyParams.Add("reason", reason);
        streamPropertyParams.Add("streamId", streamId);

        ID_CHANGESTREAMPROPERTY = sendJson(JsonConstant.STREAMPROPERTYCHANGED_METHOD, streamPropertyParams);
    }

    //Handle server Event

    void iceCandidateEvent(JSONObject paramsJson)
    {
        IceCandidate ic = new IceCandidate
        {
            SdpMid = paramsJson.GetField("sdpMid").stringValue,
            SdpMlineIndex = paramsJson.GetField("sdpMLineIndex").intValue,
            Content = paramsJson.GetField("candidate").stringValue
        };

        string connectionId = paramsJson.GetField("senderConnectionId").stringValue;
        bool isRemote = Session.instance.localParticipant.Signaler.LocalPeerId != connectionId;
        

        Debug.Log("<color=white>IceCandidate</color>(SdpMid=" + ic.SdpMid +
            ", SdpMlineIndex=" + ic.SdpMlineIndex +
            ", Content=" + ic.Content +
            ")");
        if(isRemote)
        {
            Participant remoteUser = Session.instance.FindRemoteWithConnectionId(connectionId);
            if (remoteUser == null) return;
            remoteUser.Signaler.addIceCandiate(ic);
        }else
        {
            Session.instance.localParticipant.Signaler.addIceCandiate(ic);
        }   
    }

    void participantJoinedEvent(JSONObject jsonParams)
    {

        string connectionId = jsonParams.GetField(JsonConstant.ID).stringValue;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("ConnectionId", out var masterConnectionId))
        {
            Debug.Log("<color=yellow>Method: participantJoinedEvent</color>" + connectionId);
            Debug.Log("<color=yellow>Method: participantJoinedEvent</color>" + PhotonNetwork.IsMasterClient);
            if ((string)masterConnectionId != connectionId && !PhotonNetwork.IsMasterClient)
            {
                return;
            }
        }
        else
        {
            if (!PhotonNetwork.IsMasterClient) { return; }
        }
        newRemoteParticipantAux(jsonParams);
    }

    void participantPublishedEvent(JSONObject paramsJson)
    {

        string remoteParticipantId = paramsJson.GetField(JsonConstant.ID).stringValue;
        string streamId = paramsJson.GetField("streams")[0].GetField("id").stringValue;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("ConnectionId", out var masterConnectionId))
        {
            if ((string)masterConnectionId != remoteParticipantId && !PhotonNetwork.IsMasterClient)
            {
                return;
            }
        }
        else
        {
            if (!PhotonNetwork.IsMasterClient) { return; }
        }

        //active again for sure 
        Debug.Log("remote id: " + remoteParticipantId);
        var remote = Session.instance.FindRemoteWithConnectionId(remoteParticipantId);
        if(remote == null)
        {
            Debug.Log("not find remote user");
        }
        //remote.getAudioObject().SetActive(true);
        //remote.getVideoRender().SetActive(true);
        remote.setStreamId(streamId);
        subscribe(remoteParticipantId, streamId);
    }

    void participantLeftEvent(JSONObject paramsJson)
    {
        string id = paramsJson.GetField("connectionId").stringValue;
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            Session.instance.RemoveParticipantWithConnectionId(id);
        });
    }

    void sendMessageEvent(JSONObject jsonParams)
    {
        if(jsonParams.GetField("from").stringValue == Session.instance.localSignaler.LocalPeerId)
        {
            return;
        }
        string data = jsonParams.GetField("data").stringValue;
        if(name == "room_demo")
        {
            try
            {
                string[] arrayValue = data.Split(",");
                MousePointData pointData = new MousePointData
                {
                    x = float.Parse(arrayValue[0]),
                    y = float.Parse(arrayValue[1]),
                    z = float.Parse(arrayValue[2]),
                    width = float.Parse(arrayValue[3]),
                    height = float.Parse(arrayValue[4])
                };
                //MousePointData pointData = JsonUtility.FromJson<MousePointData>(data);
                var point = new Vector3(pointData.x, pointData.y, pointData.z);
                Debug.Log("point: " + point);
                CalculatePointCoor.Instance.setStartCalculate(true, point, pointData.width, pointData.height);

            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError("data:" + data);
            }
        } else if (name == "hololens")
        {
            //
            string[] arrayValue = data.Split(",");
            Vector3 point = new Vector3(
                float.Parse(arrayValue[0]),
                float.Parse(arrayValue[1]),
                float.Parse(arrayValue[2]));
            CalculatePointCoor.Instance.InstanceMarker(point);
        }        
    }

    private void participanUnpublisedEvent(JSONObject paramsJson)
    {
        string id = paramsJson.GetField("connectionId").stringValue;
        Participant remoteUser =  Session.instance.FindRemoteWithConnectionId(id);
        if(remoteUser != null)
        {
            var audio = remoteUser.getAudioObject();
            var video = remoteUser.getVideoRender();
            audio.SetActive(false);
            video.SetActive(false); 
        }

    }

    private void streamPropertyChanged(JSONObject jsonParams) {
        Debug.Log("Stream property has changed");
    }

    private void subscribe(string remoteParticipantId, string streamId)
    {
        if(mediaServer.Equals("kurento"))
        {
            subscriptionInitiatedFromClient(remoteParticipantId, streamId);

        }
        else
        {
            prepareReceiveVideoFrom(remoteParticipantId, streamId);
        }
    }

    private void subscriptionInitiatedFromClient(string remoteParticipantId, string streamId)
    {
        var remoteUser = Session.instance.FindRemoteWithConnectionId(remoteParticipantId);
        remoteUser.Signaler.setRemotePeerId(remoteParticipantId);
        remoteUser.Signaler.setRemoteStreamId(streamId);
        remoteUser.Signaler.startCreateOfferWithKurento(true);
    }

    private void subscriptionInitiatedFromServer(string remotePeerId, string streamId)
    {
        createAnswerForSubscribing(remotePeerId, streamId);
    }

    private void createAnswerForSubscribing(string remotePeerId, string streamId)
    {
        Debug.Log("<color=red> createAnswer </color>");
        //var res = signaler.createAnswer();
        //remotePeerSignaler.setRemotePeerId(remotePeerId);
        //remotePeerSignaler.setRemoteStreamId(streamId);

    }

    private void addRemoteParticipantsAlreadyInRoom(JSONObject result)
    {
        foreach(JSONObject participant in result.GetField(JsonConstant.VALUE).list)
        {
            string connectionID = participant.GetField(JsonConstant.ID).stringValue;
            if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("ConnectionId", out var masterConnectionId))
            {
                if((string)masterConnectionId != connectionID)
                {
                    return;
                }
            } else
            {
                if(!PhotonNetwork.IsMasterClient) {
                    return;
                }
            }

            newRemoteParticipantAux(participant);
            List<JSONObject> streams = participant.GetField("streams").list;
            if(streams.Count > 0)
            {
                string streamId = streams[0].GetField("id").stringValue;
                subscribe(connectionID, streamId);
            }
        }
    }

    private void newRemoteParticipantAux(JSONObject jsonParams)
    {
        string connectionId = jsonParams.GetField(JsonConstant.ID).stringValue;
        string name = "";

        JSONObject metadata = jsonParams.GetField(JsonConstant.METADATA);
        Debug.Log("metadata: " + metadata.ToString());
        if (metadata != null)
        {
            name = metadata.stringValue;
            Debug.Log("Callllllllllllllll");

        } else
        {
            name = connectionId;
        }
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            Session.instance.CreateRemoteParticipant(connectionId, name);

        });
    }

    public static bool isHololensDevice()
    {
#if UNITY_WSA || UNITY_WSA_10_0
        return true;
#else
        return false;
#endif
    }
}

public class JsonConstant : MonoBehaviour {
    // RPC incoming methods (server event)
    public const  string PARTICIPANT_JOINED = "participantJoined";
    public const  string PARTICIPANT_LEFT = "participantLeft";
    public const  string PARTICIPANT_PUBLISHED = "participantPublished";
    public const  string ICE_CANDIDATE = "iceCandidate";
    public const  string PARTICIPANT_UNPUBLISHED = "participantUnpublished";
    public const  string PARTICIPANT_EVICTED = "participantEvicted";
    public const  string RECORDING_STARTED = "recordingStarted";
    public const  string RECORDING_STOPPED = "recordingStopped";
    public const  string SEND_MESSAGE = "sendMessage";
    public const  string STREAM_PROPERTY_CHANGED = "streamPropertyChanged";
    public const  string FILTER_EVENT_DISPATCHED = "filterEventDispatched";
    public const  string MEDIA_ERROR = "mediaError";

    // RPC outgoing methods
    public const  string PING_METHOD = "ping";
    public const  string JOINROOM_METHOD = "joinRoom";
    public const  string LEAVEROOM_METHOD = "leaveRoom";
    public const  string PUBLISHVIDEO_METHOD = "publishVideo";
    public const  string ONICECANDIDATE_METHOD = "onIceCandidate";
    public const  string PREPARERECEIVEVIDEO_METHOD = "prepareReceiveVideoFrom";
    public const  string RECEIVEVIDEO_METHOD = "receiveVideoFrom";
    public const  string UNSUBSCRIBEFROMVIDEO_METHOD = "unsubscribeFromVideo";
    public const  string SENDMESSAGE_ROOM_METHOD = "sendMessage";
    public const  string UNPUBLISHVIDEO_METHOD = "unpublishVideo";
    public const  string STREAMPROPERTYCHANGED_METHOD = "streamPropertyChanged";
    public const  string NETWORKQUALITYLEVELCHANGED_METHOD = "networkQualityLevelChanged";
    public const  string FORCEDISCONNECT_METHOD = "forceDisconnect";
    public const  string FORCEUNPUBLISH_METHOD = "forceUnpublish";
    public const  string APPLYFILTER_METHOD = "applyFilter";
    public const  string EXECFILTERMETHOD_METHOD = "execFilterMethod";
    public const  string REMOVEFILTER_METHOD = "removeFilter";
    public const  string ADDFILTEREVENTLISTENER_METHOD = "addFilterEventListener";
    public const  string REMOVEFILTEREVENTLISTENER_METHOD = "removeFilterEventListener";

    public const  string JSON_RPCVERSION = "2.0";
    public const string SDK_VERSION = "2.29.0";

    public const  string VALUE = "value";
    public const  string PARAMS = "params";
    public const  string METHOD = "method";
    public const  string ID = "id";
    public const  string RESULT = "result";
    public const  string ERROR = "error";
    public const  string MEDIA_SERVER = "mediaServer";

    public const  string SESSION_ID = "sessionId";
    public const  string SDP_ANSWER = "sdpAnswer";
    public const  string METADATA = "metadata";

    public const  string ICE_SERVERS = "customIceServers";
}
