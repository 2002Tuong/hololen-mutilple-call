using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Mouse3D : MonoBehaviour
{
    public static Mouse3D Instance { get; private set; }

    [SerializeField] private LayerMask mouseColliderLayerMask = new LayerMask();

    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Mouse position: " + Input.mousePosition);
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, mouseColliderLayerMask))
        //{
        //    transform.position = raycastHit.point;
        //    Debug.Log(raycastHit.point);
        //}
    }

    public static Vector3 GetMouseWorldPosition() => Instance.GetMouseWorldPosition_Instance();
    public static Vector3 GetWorldPosition(Vector3 point) => Instance.GetWorldPosition_Instance(point);
    private Vector3 GetMouseWorldPosition_Instance()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, mouseColliderLayerMask))
        {
           return raycastHit.point;
        } else
        {
            return Vector3.zero;
        }
    }

    private Vector3 GetWorldPosition_Instance(Vector3 point)
    {
        Ray ray = Camera.main.ScreenPointToRay(point);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, mouseColliderLayerMask))
        {
            return raycastHit.point;
        }
        else
        {
            return Vector3.zero;
        }
    }
}
