using Photon.Pun;
using PimDeWitte.UnityMainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class CalculatePointCoor : MonoBehaviourPunCallbacks
{
    public Transform captureScreen;
    public GameObject prefab;

    public static CalculatePointCoor Instance;
    private bool startCalculatePoint = false;
    private Vector3 pointInRemoteScreen;
    private float oldHeight;
    private float oldWidth;
    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("height: " + HeightOfCaptureScreen());
        //Debug.Log("width: "+ WidthOfCaptureScreen());

        //Debug.Log(("MousePosition: " + Input.mousePosition));
        
        if(Input.GetMouseButtonDown(0))
        {
            var screenCaptureWidth = WidthOfCaptureScreen();
            var screenCaptureHeight = HeightOfCaptureScreen();

            var point = Input.mousePosition;
            var pointRelative2CaptureScreenCoordinate = CaclulatePointRelative2CaptureScreenCoordinate(point);

            Debug.Log("Screen width: " + screenCaptureWidth);
            Debug.Log("Screen height: " + screenCaptureHeight);
            Debug.Log("point in screen coor: "+ pointRelative2CaptureScreenCoordinate);
            //CallRPC(pointRelative2CaptureScreenCoordinate, screenCaptureWidth, screenCaptureHeigth);
            var mousePointData = new MousePointData
            {
                x=pointRelative2CaptureScreenCoordinate.x,
                y= pointRelative2CaptureScreenCoordinate.y,
                z= pointRelative2CaptureScreenCoordinate.z,
                width = screenCaptureWidth,
                height = screenCaptureHeight
            };
            string message = mousePointData.x + "," + mousePointData.y + "," + mousePointData.z + "," + mousePointData.width + "," + mousePointData.height;

            List<string> ids = new List<string>();
            if(!Session.instance.roomDemoId.IsNullOrEmpty())
            {
                ids.Add(Session.instance.roomDemoId);
            }

            if(ids.Count > 0)
            {
                FindObjectOfType<HandleResponse>().sendMessage(message, ids);
            } else
            {
                FindObjectOfType<HandleResponse>().sendMessage(message);
            }
            //CalculatePointInWorldSpace(pointRelative2CaptureScreenCoordinate, Screen.width, Screen.height);
        }

        if(startCalculatePoint)
        {
            Debug.Log("calculte new point");
            CalculatePointInWorldSpace(pointInRemoteScreen, oldWidth, oldHeight);
            startCalculatePoint = false;
        }
    }

    public void setStartCalculate(bool start, Vector3 point , float oldWidth,float oldHeight)
    {
        startCalculatePoint = start;
        pointInRemoteScreen = point;
        this.oldHeight = oldHeight;
        this.oldWidth = oldWidth;
    } 

    //Return point in world space (world  coordinate system)
    Vector3 CalculateBottomLeft()
    {
        var bottomLeft = new Vector3();
        bottomLeft.x = captureScreen.position.x - captureScreen.localScale.x / 2;
        bottomLeft.y = captureScreen.position.y - captureScreen.localScale.y / 2;
        bottomLeft.z = captureScreen.position.z;
        return bottomLeft;
    }

    //Return point in world space (world  coordinate system)
    Vector3 CalculateTopRight()
    {
        Vector3 topRight = new Vector3();
        topRight.x = captureScreen.position.x + captureScreen.localScale.x / 2;
        topRight.y = captureScreen.position.y + captureScreen.localScale.y / 2;
        topRight.z = captureScreen.position.z;
        return topRight;
    }


    //Height in pixel
    float HeightOfCaptureScreen()
    {
        //convert world space to screen space
        var bottomLeft = CalculateBottomLeft();
        var topRight = CalculateTopRight();

        var bottomLeftInScreenSpace = Camera.main.WorldToScreenPoint(bottomLeft);
        var topRightInScreenSpace = Camera.main.WorldToScreenPoint(topRight);

        return topRightInScreenSpace.y - bottomLeftInScreenSpace.y;
    }

    //Width in pixel
    float WidthOfCaptureScreen()
    {
        var bottomLeft = CalculateBottomLeft();
        var topRight = CalculateTopRight();

        var bottomLeftInScreenSpace = Camera.main.WorldToScreenPoint(bottomLeft);
        var topRightInScreenSpace = Camera.main.WorldToScreenPoint(topRight);

        return topRightInScreenSpace.x - bottomLeftInScreenSpace.x;
    }

    //input will be mouse position
    Vector3 CaclulatePointRelative2CaptureScreenCoordinate(Vector3 point)
    {
        //this point in world space
        var originOfCaptureScreen = CalculateBottomLeft();
        //this point in screen space
        var originOfCaptureScreenInScreenSpace  = Camera.main.WorldToScreenPoint(originOfCaptureScreen);

        return point - originOfCaptureScreenInScreenSpace;
    }

    //translate this point to new screen resolution
    //return will be in screen space
    Vector3 CalculatePointInNewResolution(Vector3 point, float oldWidth, float oldHeight)
    {

        var newWidth = Screen.width;
        var newHeight = Screen.height;
        var newPoint = new Vector3();
        newPoint.x = point.x * newWidth / oldWidth;
        newPoint.y = point.y * newHeight / oldHeight;
        newPoint.z = Input.mousePosition.z;
        return newPoint;
    }

    [PunRPC]

    public void CalculatePointInWorldSpace(Vector3 point, float oldWidth, float oldHeight)
    {
        Debug.Log("Call to this function");
        Debug.Log("Old point: " + point);
        var newPointInScreenSpace = CalculatePointInNewResolution(point, oldWidth, oldHeight);
        Debug.Log("New Point: " + newPointInScreenSpace);
        point = Mouse3D.GetWorldPosition(newPointInScreenSpace);
        Debug.Log("Point in world space: " + point);
        if(point != Vector3.zero)
        {
            //create object at this point
            Instantiate(prefab, point, Quaternion.identity);
            //send this point to hololens 2
            string message = string.Format("{0},{1},{2}",point.x, point.y, point.z);
            if(!Session.instance.hololensDeviceId.IsNullOrEmpty())
            {
                List<string> ids = new List<string> { Session.instance.hololensDeviceId };
                FindAnyObjectByType<HandleResponse>().sendMessage(message, ids);
            }
           
        } else
        {
            Debug.Log("<color=red>Point is zero</color>");
        }
    }

    public void InstanceMarker(Vector3 point)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            Instantiate(prefab, point, Quaternion.identity);
        });  
    }

    void CallRPC(Vector3 point, float oldWidth, float oldHeight)
    {
        PhotonNetwork.RemoveBufferedRPCs(photonView.ViewID, "CalculatePointInWorldSpace");
        photonView.RPC("CalculatePointInWorldSpace", RpcTarget.Others, point, oldWidth, oldHeight);
        PhotonNetwork.SendAllOutgoingCommands();
    }
}
/*
 * GetMouse position by Input.mousePosition
 * Convert to coordinate system relate to CaptureScreen by CaclulatePointRelative2CaptureScreenCoordinate()
 * give this point to remote camera with capture screen width, height
 * calculate this point in new screen by CalculatePointInNewResolution()
 * Mouse3D scripts to calute this point in world space of camera scene
 */

[Serializable]
public class MousePointData
{
    [SerializeField]
    public float x;
    public float y;
    public float z;
    public float width;
    public float height;
}

public class SendMessageData
{
    public string to = "[]";
    public string data;
    public string type = "signal:chat";
}