// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Specialized;

namespace Microsoft.MixedReality.WebRTC.Unity
{

    public class OpenViduSignaler : Signaler
    {
        
        /// <summary>
        /// Automatically log all errors to the Unity console.
        /// </summary>
        [Tooltip("Automatically log all errors to the Unity console")]
        public bool AutoLogErrors = true;

        /// <summary>
        /// Unique identifier of the local peer.
        /// </summary>
        [Tooltip("Unique identifier of the local peer")]
        public string LocalPeerId;
        
        

        /// <summary>
        /// Unique identifier of the remote peer.
        /// </summary>
        [Tooltip("Unique identifier of the remote peer")]
        public string RemotePeerId;

        /// <summary>
        /// The Open vidu server to connect to
        /// </summary>
        //[Header("Server")]
        //[Tooltip("The server to connect to")]
        //public string Server = "127.0.0.1";

        //[Tooltip("The secret")]
        //public string Secret = "secret";

        //[Tooltip("The room")]
        //public string Room = "room";

        /// <summary>
        /// The interval (in ms) that the server is polled at
        /// </summary>
        [Tooltip("The interval (in ms) that the server is polled at")]
        public float PollTimeMs = 500f;



        /// <summary>
        /// Internal timing helper
        /// </summary>
        private float timeSincePollMs = 0f;

        /// <summary>
        /// Internal last poll response status flag
        /// </summary>
        private bool lastGetComplete = true;
        private HandleResponse responseHandler;

        #region ISignaler interface

        private SdpMessage sdpAnswerReceiveVideo;
        //for start publish video
        private bool startConnection = false;
        //for start receiver video from with kurento media server
        private bool startReceiveVideoWithKurento = false;
        private bool isDoReceiveVideoFrom = false;
        private string streamId = "";
        /// <inheritdoc/>
        public override Task SendMessageAsync(SdpMessage message)
        {
            if(responseHandler.mediaServer.Equals("kurento") && message.Type == SdpMessageType.Offer && isDoReceiveVideoFrom)
            {
                Debug.Log("<color=red> createOffer success for kurento</color>");
                responseHandler.receiveVideoFrom(message, LocalPeerId, streamId);
                isDoReceiveVideoFrom=false;
                var tcs1 = new TaskCompletionSource<bool>();
                tcs1.SetResult(true);

                return tcs1.Task;
            }

            Debug.Log("<color=cyan>SdpMessage</color>: " + message.Type);
            if (message.Type == SdpMessageType.Offer)
            {
#if UNITY_WSA || UNITY_WSA_10_0
#else 
                Debug.Log("<color=red> createOffer success </color>");
                responseHandler.publishVideo(message);
#endif
            }

            if (message.Type == SdpMessageType.Answer)
            {
                Debug.Log("<color=red> createAnswer success </color>");
                responseHandler.receiveVideoFrom(message, LocalPeerId, streamId);
            }


            var tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(true);
            
            return tcs.Task;
        }

        /// <inheritdoc/>
        public override Task SendMessageAsync(IceCandidate candidate)
        {    
            responseHandler.onIceCandidate(candidate, LocalPeerId);

            var tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(true);

            return tcs.Task;
        }

#endregion

#region IDataReceiver interface
        public enum OpenViduType {
            Ping,
            JoinRoom,
            PublishVideo,
            ReceiveVideoFrom,
            OnIceCandidate
        }

#region normal_method
        public void handleMessageAsync(Action item1)
        {
            _mainThreadWorkQueue.Enqueue(item1);
        }

        public void addIceCandiate(IceCandidate ic)
        {
            _nativePeer.AddIceCandidate(ic);
        }

        public void setToStartConnection(bool startConnection)
        {
            this.startConnection = startConnection;
        }

        public void setRemotePeerId(string id)
        {
            this.LocalPeerId = id;
        }

        public void setRemoteStreamId(string id)
        {
            this.streamId = id;
        }

        public bool createAnswer()
        {
            return _nativePeer.CreateAnswer();
        }

        public void startCreateOfferWithKurento(bool start)
        {
            startReceiveVideoWithKurento = start;
            isDoReceiveVideoFrom = true;

            
        }
#endregion

#endregion




        /// <summary>
        /// Unity Engine Start() hook
        /// </summary>
        /// <remarks>
        /// https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html
        /// </remarks>
        private void Start()
        {
            
            
            responseHandler = FindAnyObjectByType<HandleResponse>();

            // If not explicitly set, default local ID to some unique ID generated by Unity
            if (string.IsNullOrEmpty(LocalPeerId))
            {
                LocalPeerId = SystemInfo.deviceName;
            }
         
        }

        private void Connection_IceStateChanged(IceConnectionState newState)
        {
            Debug.LogWarning("IceGatheringStateChanged");
        }

        private void Connection_IceGatheringStateChanged(IceGatheringState newState)
        {
            Debug.LogWarning("IceGatheringStateChanged");

        }

        private void Connection_RenegotiationNeeded()
        {
            Debug.LogWarning("RenegotiationNEeded");
        }

        public void OnInitialized()
        {
            Debug.Log("<color=pink>OnInitialized</color>");
        }

        public void OnShutdown()
        {
            Debug.Log("<color=pink>OnShutdown</color>");
        }

        public void OnError(string s)
        {
            Debug.Log("<color=pink>OnError </color>" + s);
        }


        /// <summary>
        /// Internal coroutine helper for receiving HTTP data from the DSS server using GET
        /// and processing it as needed
        /// </summary>
        /// <returns>the message</returns>

        /// <inheritdoc/>
        protected override void Update()
        {
            // Do not forget to call the base class Update(), which processes events from background
            // threads to fire the callbacks implemented in this class.
            base.Update();
            //start connection to publish video
            if (startConnection)
            {
                Debug.Log("<color=red> createOffer  </color>");
                PeerConnection.StartConnection();
                
                Debug.Log("Start connection call");
                _nativePeer.RenegotiationNeeded += Connection_RenegotiationNeeded;
                _nativePeer.IceGatheringStateChanged += Connection_IceGatheringStateChanged;
                _nativePeer.IceStateChanged += Connection_IceStateChanged;

                startConnection = false;
            }

            if(startReceiveVideoWithKurento && _nativePeer.Initialized)
            {
                PeerConnection.StartConnection();
                startReceiveVideoWithKurento = false;
            }

            // If we have not reached our PollTimeMs value...
            if (timeSincePollMs <= PollTimeMs)
            {
                // ...then we keep incrementing our local counter until we do.
                timeSincePollMs += Time.deltaTime * 1000.0f;
                return;
            }

            // If we have a pending request still going, don't queue another yet.
            if (!lastGetComplete)
            {
                return;
            }

            // When we have reached our PollTimeMs value...
            timeSincePollMs = 0f;

            // ...begin the poll and process.
            lastGetComplete = false;
        }

        private IEnumerator SdpAnswer()
        {
            yield return new WaitForSeconds(1f);
            _mainThreadWorkQueue.Enqueue(() =>
            {

                PeerConnection.HandleConnectionMessageAsync(sdpAnswerReceiveVideo).ContinueWith(_ =>
                {
                    _nativePeer.CreateAnswer();
                    sdpAnswerReceiveVideo = null;

                }, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.RunContinuationsAsynchronously);
            });
            
        }

        private void DebugLogLong(string str)
        {
#if UNITY_ANDROID
            // On Android, logcat truncates to ~1000 characters, so split manually instead.
            const int maxLineSize = 1000;
            int totalLength = str.Length;
            int numLines = (totalLength + maxLineSize - 1) / maxLineSize;
            for (int i = 0; i < numLines; ++i)
            {
                int start = i * maxLineSize;
                int length = Math.Min(start + maxLineSize, totalLength) - start;
                Debug.Log(str.Substring(start, length));
            }
#else
            Debug.Log(str);
#endif
        }
    
    }
}
