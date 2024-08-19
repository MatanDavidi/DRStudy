using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePrefabSpawner : MonoBehaviour
{
    public GameObject prefab;
    public GameObject previewPrefab;
    private GameObject currentPreview;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Instantiating preview prefab");
        currentPreview = Instantiate(previewPrefab);

        Debug.Log("Preview prefab instantiated");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Getting controller's position and rotation vectors");
        //Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        //Quaternion controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
        //Vector3 controllerRotationVector = controllerRotation * Vector3.forward;
        Debug.Log("Casting ray from controller");
        Ray ray = new Ray(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch), OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("Raycasting has hit");
            currentPreview.transform.position = hit.point;
            currentPreview.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Debug.Log("Preview prefab position and rotation updated");
            if (OVRInput.GetDown(OVRInput.Button.One))
            {
                Debug.Log($"Instantiating actual prefab at point {hit.point}");
                GameObject newObject = Instantiate(prefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                // Rotate table to be facing user
                newObject.transform.Rotate(Vector3.up * 180.0f);
                Debug.Log($"Actual prefab instantiated at point {hit.point}");
                Destroy(currentPreview);
            }
        }
    }
}
