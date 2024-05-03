using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GeneratePosition : MonoBehaviour
{
    public float Distance = 0.01f;
    public int limitedItemInRow = 3;

    private int indexRow = 0;
    private int indexCol = 0;
    private List<Vector3> RemoteMediaPositions;
    public GameObject LocalMedia;
    public GameObject PrefabRemote;
    public Camera Camera;
    private float HorizontalFOV;
    private float VerticalFOV;
    private float NearZ;
    private float FarZ;
    private float prefabWidth;
    private float prefabHeight;
    private float WitdthOfGenArea;
    private float HeightOfGenArea;
    // Start is called before the first frame update
    void Start()
    {
        RemoteMediaPositions = new List<Vector3>();
        HorizontalFOV = Camera.fieldOfView;
        VerticalFOV = Camera.HorizontalToVerticalFieldOfView(HorizontalFOV, Camera.aspect);
        NearZ = Camera.nearClipPlane;
        FarZ = Camera.farClipPlane;

        float DistanceFromCameraToLocalMediaByZ = Mathf.Abs(Camera.transform.position.z - LocalMedia.transform.position.z);
        float DistanceFromNearPlaneToFar = Mathf.Abs(FarZ - NearZ);


        float HorizontalFOVInRads = HorizontalFOV * Mathf.Deg2Rad;
        float VerticalFOVInRads = VerticalFOV * Mathf.Deg2Rad;
        WitdthOfGenArea = (DistanceFromCameraToLocalMediaByZ / Mathf.Cos(VerticalFOVInRads / 2)) * Mathf.Tan(HorizontalFOVInRads / 2) * 2; 
        HeightOfGenArea = (DistanceFromCameraToLocalMediaByZ / Mathf.Cos(HorizontalFOVInRads / 2)) * Mathf.Tan(VerticalFOVInRads / 2) * 2;

        GameObject BackPlate = PrefabRemote.transform.Find("RemoteVideoPlayer").Find("BackPlate").gameObject;
        Vector3 min = BackPlate.GetComponent<MeshRenderer>().bounds.min;
        Vector3 max = BackPlate.GetComponent<MeshRenderer>().bounds.max;

        //Vector3 screenMin = Camera.WorldToScreenPoint(min);
        //Vector3 screenMax = Camera.WorldToScreenPoint(max);

        prefabWidth = BackPlate.GetComponent<MeshRenderer>().bounds.size.x;
        prefabHeight = BackPlate.GetComponent<MeshRenderer>().bounds.size.y;

        GenListPosition();
    }

    private bool CheckLegalPos(Vector3 newPosition)
    {
        float DistanceInWidth = prefabWidth;
        float DistanceInHeight = prefabHeight;

        // check with other Prefab. Make sure not overlay
        foreach (Vector3 pos in RemoteMediaPositions)
        {
            if (Mathf.Abs(pos.x - newPosition.x) < DistanceInWidth && Mathf.Abs(pos.y - newPosition.y) < DistanceInHeight)
            {
                return false;
            }
        }

        // check to make sure. Not out of screen
        Vector3 left = newPosition + new Vector3(-prefabWidth/2 , 0 , 0);
        Vector3 right = newPosition + new Vector3(prefabWidth / 2, 0, 0);
        Vector3 top = newPosition + new Vector3(0, prefabHeight / 2, 0);
        Vector3 bottom = newPosition + new Vector3(0, -prefabHeight / 2, 0);

        Vector3 leftLimit = Camera.transform.position + new Vector3(-WitdthOfGenArea /2, 0 ,0);
        Vector3 rightLimit = Camera.transform.position + new Vector3(WitdthOfGenArea / 2, 0, 0);
        Vector3 topLimit = Camera.transform.position + new Vector3(0, HeightOfGenArea / 2, 0);
        Vector3 bottomLimit = Camera.transform.position + new Vector3(0, -HeightOfGenArea / 2, 0);

        //if (left.x - leftLimit.x < 0 || rightLimit.x - right.x < 0 || topLimit.y - top.y < 0 || bottom.y - bottomLimit.y < 0)
        //{
        //    return false;
        //}
        return true;
    }

    private Vector3? GenPosition(int row, int col)
    {
        //Debug.Log("size: " + RemoteMediaPositions.Count);
        if(RemoteMediaPositions.Count <= 0)
        {
            return new Vector3(0,0,-1);
        }

        float DistanceInWidth = prefabWidth + Distance;
        float DistanceInHeight = prefabHeight + Distance;

        for(int i = col * limitedItemInRow + row; i <= RemoteMediaPositions.Count && row < limitedItemInRow; i++)
        {
            Vector3 pos = RemoteMediaPositions[i-1];
            Vector3 newPos1 = pos + new Vector3(DistanceInWidth, 0, 0);
            //Debug.Log("newPos1: " + newPos1);
            if (CheckLegalPos(newPos1))
            {
                return newPos1;
            }
        }
        var position = RemoteMediaPositions[col * limitedItemInRow + row - 1];
        Vector3 newPos2 = position + new Vector3(0, DistanceInHeight, 0);
        //Debug.Log("newPos2: " + newPos2);
        if (CheckLegalPos(newPos2))
        {
            return newPos2;
        }
        //foreach (Vector3 position in RemoteMediaPositions)
        //{
        //    Vector3 newPos1 = position + new Vector3(DistanceInWidth, 0, 0);
        //    Debug.Log("newPos1: " + newPos1);
        //    if(CheckLegalPos(newPos1))
        //    {
        //        return newPos1;
        //    }
        //}

        //foreach (Vector3 position in RemoteMediaPositions)
        //{
        //    Vector3 newPos2 = position + new Vector3(0, DistanceInHeight, 0);
        //    Debug.Log("newPos2: " + newPos2);
        //    if (CheckLegalPos(newPos2))
        //    {
        //        return newPos2;
        //    }
        //}

        return null;
    }

    public void GenListPosition()
    {
        for (int i = 0; i < 10; i++)
        {
            
            if(indexRow == limitedItemInRow)
            {
                indexRow = 0;
                indexCol++;
            }
            Vector3? position = GenPosition(indexRow, indexCol);
            indexRow++;
            if (position != null)
            {
                //Debug.Log("Pos: " + i + " | position: " + position.Value);
                AddRemoteMediaPos((Vector3)position);
            }
        }
    }

    public void AddRemoteMediaPos(Vector3 position)
    {
        RemoteMediaPositions.Add(position);
    }

    public void SetListPos(List<Vector3> ListPos)
    {
        RemoteMediaPositions.Clear();
        RemoteMediaPositions = ListPos;
    }

    public int AmountPlayer()
    {
        return RemoteMediaPositions.Count;
    }

    public Vector3 GetAt(int index)
    {
        return RemoteMediaPositions[index];
    }

}



