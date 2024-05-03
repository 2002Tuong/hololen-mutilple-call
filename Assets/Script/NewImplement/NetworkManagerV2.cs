using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Chat;
using Photon.Pun;
using System;
using Photon.Realtime;
using System.Net;
using System.Linq;
using Microsoft.MixedReality.WebRTC.Unity;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class NetworkManagerV2 : MonoBehaviourPunCallbacks
{
    public static NetworkManagerV2 Instance;
    private bool _isMobilePlatform;
    // -------------- Android --------------
#if UNITY_ANDROID
    private List<string> permissions = new List<string>();
#endif
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
        PhotonNetwork.EnableCloseConnection = true;
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _isMobilePlatform = true;
        }
    }

    private void Start()
    {
        Instance = this;
        if (_isMobilePlatform)
        {
#if UNITY_ANDROID
            permissions.Add(Permission.Microphone);
            permissions.Add(Permission.Camera);

            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
            callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
            callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;

            Permission.RequestUserPermissions(permissions.ToArray(), callbacks);

            void PermissionCallbacks_PermissionGranted(string permissionName)
            {
                Connect();
            }

            void PermissionCallbacks_PermissionDenied(string permissionName)
            {
                Application.Quit();

            }

            void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
            {
                Application.Quit();
            }
#endif
        }
        else
        {
            Connect();
        }
    }

    private void OnApplicationQuit()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.EmptyRoomTtl = 0;
            PhotonNetwork.CurrentRoom.PlayerTtl = 0;

            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (!player.IsMasterClient)
                {
                    PhotonNetwork.CloseConnection(player);
                    PhotonNetwork.SendAllOutgoingCommands();
                }

            }
        }

        LeaveRoom();
        Disconnect();
    }

    #region PUN Callbacks
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connect success");
        JoinRoom();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        Application.Quit();
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Application.Quit();
    }
    #endregion

    #region Private Callbacks

    public void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("call ConnectUsingSettings");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    private void Disconnect()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    private void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    private void JoinRoom()
    {
        if (!PhotonNetwork.InRoom)
        {
            PhotonNetwork.JoinOrCreateRoom(
                "360VRCollocated",
                new RoomOptions
                {
                    IsOpen = true,
                    IsVisible = true,
                    MaxPlayers = 0
                },
                null,
                null);
        }
    }

    private string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
        .AddressList.First(
        f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        .ToString();
    }

    private string RandomId()
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[8];
        var random = new System.Random();

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        var finalString = new String(stringChars);
        return finalString;
    }
    //Format: http://ip:port/
    private string getServerAddress()
    {
        string address = "http://";
        address += GetLocalIPv4();
        address.Remove(address.Length - 1);
        address += ":3000/";
        return address;
    }
    #endregion
}
