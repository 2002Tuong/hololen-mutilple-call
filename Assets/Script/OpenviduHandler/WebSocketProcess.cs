using Defective.JSON;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity3dAzure.WebSockets;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;
using UnityEngine.Networking;
using System.Text;
using Microsoft.MixedReality.WebRTC.Unity;
using OpenVidu;
using System.Net;

public class WebSocketProcess : MonoBehaviour, IDataReceiver
{

    /// <summary>
    /// The Open vidu server to connect to
    /// </summary>
    [Header("Server")]
    [Tooltip("The server to connect to")]
    public string Server = "127.0.0.1";

    [Tooltip("The secret")]
    public string Secret = "secret";

    [Tooltip("The room")]
    public string Room = "room";


    public HandleResponse responseHandler;
    public UnityWebSocket webSocket;

    private string EncodedSecret;
    private string token = "";

    private bool webSocketConnect = false;
    private bool firstTimeCall = true;
    public void OnReceivedData(object sender, EventArgs args)
    {
        if (args == null)
        {
            return;
        }

        // return early if wrong type of EventArgs
        var myArgs = args as TextEventArgs;
        if (myArgs == null)
        {

            Debug.Log("Got somethin elseg from ws:" + args.ToString());
            return;
        }

        var json = myArgs.Text;

        JSONObject responseJson = new JSONObject(json);
        Debug.Log("OnReceiveData: " + responseJson.ToString());
        if (responseJson.HasField(JsonConstant.RESULT))
        {
            Debug.Log("call handle Response");
            responseHandler.HandleServerResponse(responseJson);
        }
        else if (responseJson.HasField(JsonConstant.ERROR))
        {
            Debug.Log("call handle Error");
            responseHandler.HandleServerError(responseJson);
        }
        else
        {
            Debug.Log("call handle Event");
            responseHandler.HandleServerEvent(responseJson);
        }
    }

    private void OnDisable()
    {
        DataHandler.OnReceivedData -= OnReceivedData;
    }

    private void OnEnable()
    {
        DataHandler.OnReceivedData += OnReceivedData;
    }

    public IEnumerator PingSchedule()
    {
        while (true)
        {
            responseHandler.ping();
            yield return new WaitForSeconds(2);
        }
    }

    public void setIsWebsocketConnect(bool isConnect)
    {
        webSocketConnect = isConnect;
    }

    public string getToken() => token;
    public UnityWebSocket getWebSocket() => webSocket;

    public void StartConnect()
    {
        StartCoroutine(Connect());
    }

    private IEnumerator Connect()
    {
        //retrive a session if exists
        Debug.Log("Server: " + Server);
        var cerHandler = new CertificateWhore();
        ServicePointManager.ServerCertificateValidationCallback +=
            (sender, certificate, chain, sslPolicyErrors) => true;
        //var www = UnityWebRequest.Get($"http://localhost:4443/openvidu/api/sessions/" + Room);
        var www = UnityWebRequest.Get($"https://{Server}/openvidu/api/sessions/" + Room);
        www.SetRequestHeader("Authorization", "Basic " + EncodedSecret);
        www.certificateHandler = cerHandler;
        yield return www.SendWebRequest();
        bool sessionOk = false;
        if (www.isNetworkError)
        {
            Debug.Log("Error While Sending: " + www.error);
        }
        else
        {
            Debug.Log($"Received{www.responseCode}: {www.downloadHandler.text}");
            //session = JsonConvert.DeserializeObject<OpenViduSessionInfo>(www.downloadHandler.text);
            sessionOk = true;
        }

        if (www.responseCode == 404)
        {
            Debug.Log("Creating Session");

            www = new UnityWebRequest($"https://{Server}/api/sessions", "POST");
            www.certificateHandler = cerHandler;
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes("{\"customSessionId\": \"" + Room + "\"}");
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);

            www.SetRequestHeader("Authorization", "Basic " + EncodedSecret);
            www.SetRequestHeader("Content-Type", "application/json");
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log("Error While Sending: " + www.error);
            }
            else
            {
                Debug.Log($"Received{www.responseCode}: {www.downloadHandler.text}");
                sessionOk = true;
            }
        }

        if (sessionOk)
        {
            Debug.Log("Asking for a token");
            www = new UnityWebRequest($"https://{Server}/api/sessions/{Room}/connections", "POST");
            www.certificateHandler = cerHandler;
            //byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes();// default to publisher
            //byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes("{\"role\": \"PUBLISHER\"}");

            //www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            www.SetRequestHeader("Authorization", "Basic " + EncodedSecret);
            www.SetRequestHeader("Content-Type", "application/json");
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log("Error While Sending: " + www.error);
            }
            else
            {
                Debug.Log($"Received2 {www.responseCode}: {www.downloadHandler.text}");
                var t = new OpenViduToken(www.downloadHandler.text); //JsonConvert.DeserializeObject<OpenViduToken>(www.downloadHandler.text);
                token = t.token;
                Debug.Log($"Token: {token}");
            }
        }

        //connect Websocket
        webSocket.Connect();
    }

    // Start is called before the first frame update
    void Start()
    {
        byte[] bytesToEncode = Encoding.UTF8.GetBytes("OPENVIDUAPP:" + Secret);
        EncodedSecret = Convert.ToBase64String(bytesToEncode);
    }

    // Update is called once per frame
    void Update()
    {
        if (webSocketConnect && firstTimeCall)
        {
            Debug.Log("Call method joinRoom");
            responseHandler.joinRoom();
            Debug.Log("Call method joinRoom complete");
            StartCoroutine(PingSchedule());
            firstTimeCall = false;
        }
    }
}


public class CertificateWhore : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}