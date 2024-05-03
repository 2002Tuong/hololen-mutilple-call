using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionPool : MonoBehaviour
{
    public static PositionPool instance;
    public static List<Vector3> pool = new List<Vector3>();
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        pool.Add(new Vector3(0,0,-1));
        pool.Add(new Vector3(0.38f, 0, -1));
        pool.Add(new Vector3(0.76f, 0.00f, -1.00f));
        pool.Add(new Vector3(0, 0.32f, -1));
        pool.Add(new Vector3(0.38f, 0.32f, -1));
        pool.Add(new Vector3(0.76f, 0.32f, -1));
        pool.Add(new Vector3(0f, 0.64f, -1));
        pool.Add(new Vector3(0.38f, 0.64f, -1));
        pool.Add(new Vector3(0.76f, 0.64f, -1));
        pool.Add(new Vector3(0, 0.96f, -1));

    }
}
