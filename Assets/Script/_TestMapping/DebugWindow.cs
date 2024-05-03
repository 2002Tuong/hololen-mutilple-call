using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugWindow : MonoBehaviour
{
    TextMesh textMesh;
    public GameObject GameObject;
    // Start is called before the first frame update
    void Start()
    {
        textMesh = gameObject.GetComponentInChildren<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
        var x = GameObject.transform.position.x;
        var y = GameObject.transform.position.y;
        var z = GameObject.transform.position.z;

        LogMessage("(" + x + ", " + y + ", " + z + " )", "", LogType.Log);
    }

    void OnEnable()
    {
        Application.logMessageReceived += LogMessage;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= LogMessage;
    }

    public void LogMessage(string message, string stackTrace, LogType type)
    {
        if (textMesh.text.Length > 300)
        {
            textMesh.text = message + "\n";
        }
        else
        {
            textMesh.text += message + "\n";
        }
    }
}
