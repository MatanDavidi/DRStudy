using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerAimer : MonoBehaviour
{
    Vector3 storedControllerPosition;
    // Start is called before the first frame update
    void Start()
    {
        storedControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Any))
        {
            storedControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        }
        transform.LookAt(storedControllerPosition);
    }
}
