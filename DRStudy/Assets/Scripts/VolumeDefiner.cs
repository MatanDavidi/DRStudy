using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class VolumeDefiner : MonoBehaviour
{
    #region Serialized members
    [SerializeField] private float vertexPrefabWidth = 0.01f;
    [SerializeField] private GameObject vertexPrefab;
    [SerializeField] private VolumePrefab[] volumePrefabs;
    [SerializeField] private Transform parent;
    #endregion

    #region Private members
    private Vector3[] vertices;
    private List<GameObject> setupGameObjects;
    private int currentVertexIndex = -1;
    private readonly Vector3 OVR_CONTROLLER_RADIUS = new(0.0f, 0.0f, 0.03f);
    private int currentPrefabIndex = 0;
    #endregion

    #region Constants
    private const float EPSILON = 1e-05f;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        vertices = new Vector3[4];
        if (!volumePrefabs.IsUnityNull())
        {
            setupGameObjects = new List<GameObject>(volumePrefabs.Length * 4 * 3); // For each VolumePrefab we have 4 vertices and 3 lines.
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (volumePrefabs.Length > 0)
            if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            {
                Debug.Log($"Detecting vertex at controller position. Spawning vertex.");
                if (currentVertexIndex < vertices.Length - 1)
                {
                    vertices[++currentVertexIndex] = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch) + OVR_CONTROLLER_RADIUS;

                    if (!vertexPrefab.IsUnityNull())
                    {
                        GameObject tempVertexPrefab = Instantiate(vertexPrefab, vertices[currentVertexIndex], Quaternion.identity);
                        tempVertexPrefab.transform.localScale = new Vector3(vertexPrefabWidth, vertexPrefabWidth, vertexPrefabWidth);
                        setupGameObjects.Add(tempVertexPrefab);
                        Debug.Log($"Add volume vertex at coords: {vertices[currentVertexIndex]}");
                    }

                    if (currentVertexIndex > 0)
                    {
                        // Initialize new line's LineRenderer
                        GameObject line = new GameObject("Line");
                        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
                        lineRenderer.startColor = lineRenderer.endColor = Color.black/*vertexPrefab.GetComponent<Renderer>().material.color*/;
                        lineRenderer.startWidth = lineRenderer.endWidth = vertexPrefabWidth;
                        lineRenderer.positionCount = 2;
                        lineRenderer.useWorldSpace = true;
                        setupGameObjects.Add(line);

                        if (currentVertexIndex == vertices.Length - 1)
                        {
                            lineRenderer.SetPosition(0, vertices[currentVertexIndex - 2]);  // x, y and z position of the starting point of the line
                        }
                        else
                        {
                            lineRenderer.SetPosition(0, vertices[currentVertexIndex - 1]);  // x, y and z position of the starting point of the line
                        }

                        lineRenderer.SetPosition(1, vertices[currentVertexIndex]);          // x, y and z position of the end point of the line

                        if (currentVertexIndex == vertices.Length - 1)
                        {
                            InstantiateVolumePrefab();
                            BehaviourExecuted();
                        }
                    }
                }
            }
    }

    private void InstantiateVolumePrefab()
    {
        // Calculate the size of the volume
        float width = Vector3.Distance(vertices[0], vertices[1]);
        float height = Vector3.Distance(vertices[1], vertices[2]);
        float depth = Vector3.Distance(vertices[1], vertices[3]);

        GameObject targetArea = new GameObject($"VolumePrefab{currentPrefabIndex}");
        setupGameObjects.Add(targetArea);
        BoxCollider targetCollider = targetArea.AddComponent<BoxCollider>();
        targetCollider.size = new Vector3(width, height, depth);

        // Instantiate the prefab and position it in the center of the volume
        if (!volumePrefabs[currentPrefabIndex].IsUnityNull())
        {
            // If the parent was specified, instantiate the prefab as child of that parent.
            GameObject instance;
            Vector3 forwardFaceMidPoint = Vector3.Lerp(vertices[0], vertices[2], 0.5f);
            Vector3 depthMidPoint = Vector3.Lerp(vertices[1], vertices[3], 0.5f);
            Vector3 depthMidPointDifference = depthMidPoint - vertices[1];
            Vector3 volumeCenter = forwardFaceMidPoint + depthMidPointDifference;

            //GameObject tempForward = Instantiate(vertexPrefab, forwardFaceMidPoint, Quaternion.identity);
            //tempForward.transform.localScale = new(vertexPrefabWidth, vertexPrefabWidth, vertexPrefabWidth);

            //GameObject tempDepth = Instantiate(vertexPrefab, depthMidPoint, Quaternion.identity);
            //tempDepth.transform.localScale = new(vertexPrefabWidth, vertexPrefabWidth, vertexPrefabWidth);

            //GameObject tempCenter = Instantiate(vertexPrefab, volumeCenter, Quaternion.identity);
            //tempCenter.transform.localScale = new(vertexPrefabWidth, vertexPrefabWidth, vertexPrefabWidth);

            if (volumePrefabs[currentPrefabIndex].AlreadyInstantiated)
            {
                volumePrefabs[currentPrefabIndex].UnderlyingPrefab.transform.position = volumeCenter;
                instance = volumePrefabs[currentPrefabIndex].UnderlyingPrefab;
            } else
            {
                if (!parent.IsUnityNull())
                {
                    instance = Instantiate(volumePrefabs[currentPrefabIndex].UnderlyingPrefab, parent, true);
                }
                else
                {
                    instance = Instantiate(volumePrefabs[currentPrefabIndex].UnderlyingPrefab, transform, true);
                }
                instance.transform.position = volumeCenter;
                instance.transform.rotation = Quaternion.identity;
            }
            // Scale the prefab to fit the volume
            Vector3 targetScale = targetCollider.bounds.size;

            Vector3 modelScale;
            RectTransform instanceRect;
            if (!(instanceRect = instance.GetComponentInChildren<RectTransform>()).IsUnityNull())
            {
                modelScale = instanceRect.rect.size;
            } else
            {
                modelScale = instance.GetComponent<MeshRenderer>().bounds.size;
            }

            Debug.Log($"[DEBUG] target scale: {targetScale}; model scale: {modelScale}");

            if (volumePrefabs[currentPrefabIndex].ShouldRotate)
            {
                // Calculate normal vector of the plane defined by the first three vertices
                Vector3 edge0 = vertices[1] - vertices[0];
                Vector3 edge1 = vertices[2] - vertices[1];

                Vector3 normal = Vector3.Cross(edge0, edge1).normalized;

                // Make instance look at center of forward-facing surface
                //instance.transform.LookAt(Vector3.Lerp(vertices[0], vertices[2], 0.5f));
                instance.transform.rotation = Quaternion.LookRotation(normal);
            }

            ScalePrefab(instance, modelScale, targetScale, volumePrefabs[currentPrefabIndex].scaleMultiplier);
        }
        else
        {
            Debug.LogError($"Could not instantiate volume prefab number {currentPrefabIndex} because it is null. Moving on to next prefab.");
        }
    }

    private void ScalePrefab(GameObject model, Vector3 modelScale, Vector3 targetScale, float scale)
    {
        var xFraction = modelScale.x / targetScale.x;
        var yFraction = modelScale.y / targetScale.y;
        var zFraction = modelScale.z / targetScale.z;

        Vector3 fraction = new Vector3(xFraction, yFraction, zFraction);
        // Check if any of the fraction's features is "zero", if so avoid division (because dividing by 0 is bad m'kay?)
        if (fraction.x < EPSILON)
        {
            fraction.x = 1.0f;
        }
        if (fraction.y < EPSILON)
        {
            fraction.y = 1.0f;
        }
        if (fraction.z < EPSILON)
        {
            fraction.z = 1.0f;
        }

        model.transform.localScale = UtilityMethods.Divide(model.transform.localScale, fraction) * scale;
    }

    public void BehaviourExecuted()
    {
        if (++currentPrefabIndex < volumePrefabs.Length)
        {
            ResetVolume();
        }
        else
        {
            Debug.Log($"Destroying {setupGameObjects.Count} setup objects");
            foreach (GameObject setupObject in setupGameObjects)
            {
                Destroy(setupObject);
            }
        }
    }

    private void ResetVolume()
    {
        vertices = new Vector3[4];
        currentVertexIndex = -1;
    }
}
