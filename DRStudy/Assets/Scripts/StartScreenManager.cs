using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour
{
    #region Serialized fields
    [SerializeField]
    private Button startButtonPrefab;
    /// <summary>
    /// Game objects that should only be enabled once this start button has been clicked.
    /// </summary>
    [SerializeField]
    public GameObject[] dependencies;
    #endregion

    #region Properties
    public ClickingTask ParentScript { get; set; }
    #endregion

    #region Private fields
    private Button startButtonInstance;
    #endregion
    public void LoadStartScreen(ClickingTask parentScript, Canvas parentPanel)
    { 
        foreach (GameObject dependency in dependencies)
        {
        Debug.Log($"Dependencies: {dependencies}; next dependency: {dependency}");
            if (dependency.activeInHierarchy)
                dependency.SetActive(false);
        }

        if (!startButtonPrefab.IsUnityNull())
        {
            ParentScript = parentScript;
            startButtonInstance = Instantiate(startButtonPrefab, parentPanel.transform);
            startButtonInstance.onClick.AddListener(OnStart);
            Debug.Log("[DEBUG] OnStart click event listener added to start button");
            startButtonInstance.transform.SetSiblingIndex(transform.parent.childCount - 2);
        } else
        {
            Debug.LogError("[ERROR] No start button prefab was found. Task cannot be started.");
        }
    }

    public void OnStart()
    {
        foreach (GameObject dependency in dependencies)
        {
            dependency.SetActive(true);
        }

        startButtonInstance.onClick.RemoveListener(OnStart);
        Destroy(startButtonInstance.gameObject);
        ParentScript.StartClickingTask();
    }
}
