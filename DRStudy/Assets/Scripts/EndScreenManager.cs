using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EndScreenManager : MonoBehaviour
{
    #region Serialized fields
    [SerializeField]
    private TextMeshProUGUI endTextPrefab;
    /// <summary>
    /// Game objects that should only be enabled once this start button has been clicked.
    /// </summary>
    [SerializeField]
    private GameObject[] dependencies;
    #endregion

    #region Properties
    public ClickingTask ParentScript { get; set; }
    #endregion

    public void LoadEndScreen(Canvas parentPanel)
    {
        foreach (GameObject dependency in dependencies)
        {
            if (dependency.activeInHierarchy)
                dependency.SetActive(false);
        }

        if (!endTextPrefab.IsUnityNull())
        {
            Instantiate(endTextPrefab, parentPanel.transform);
        } else
        {
            Debug.LogError("[ERROR] No end text prefab was found. Task cannot be started.");
        }
    }
}
