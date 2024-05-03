using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestXample : MonoBehaviour
{
    public float distance = -1.0F;
    public GameObject prefab;
    void OnDrawGizmosSelected()
    {
        //Matrix4x4 m = Camera.main.cameraToWorldMatrix;
        //Vector3 p = m.MultiplyPoint(new Vector3(0, 0, distance));
        
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(p, 0.2F);
    }

    private void Update()
    {

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Matrix4x4 m = Camera.main.cameraToWorldMatrix;
            Vector3 p = m.MultiplyPoint(new Vector3(0, 0, distance));
            Object.Instantiate(prefab, p, Quaternion.identity);

            
        }

        if(Input.GetKeyDown(KeyCode.J)) {
            prefab.transform.position = new Vector3(0, 0, distance);
        }    

    }
}
