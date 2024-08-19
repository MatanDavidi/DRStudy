using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

public class DebugDisplay : MonoBehaviour
{
    private Dictionary<string, string> debugLogs = new Dictionary<string, string>();
    [SerializeField] private TextMeshProUGUI display;
    private const int MAX_DEBUG_MESSAGES = 5;

    private void Update()
    {
        // Check for user console clears
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            debugLogs.Clear();
        }
    }

    private void OnEnable()
    {
        debugLogs.Clear();
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        UpdateLogs(logString, type);
        UpdateDisplay();
    }

    private void UpdateLogs(string logString, LogType type)
    {
        /*
        if (type == LogType.Log)
        {
        */
            string[] splitString = logString.Split(':');
            string debugKey = splitString[0];
            string debugValue = splitString.Length > 1 ? splitString[1] : string.Empty;

            if (debugLogs.ContainsKey(debugKey))
            {
                debugLogs[debugKey] = debugValue;
            }
            else
            {
                if (debugLogs.Count == MAX_DEBUG_MESSAGES)
                    debugLogs.Remove(debugLogs.Keys.FirstOrDefault());
                debugLogs.Add(debugKey, debugValue);
            }
        //}
    }

    private void UpdateDisplay()
    {
        StringBuilder sb = new();
        foreach (var log in debugLogs)
        {
            if (log.Value.Trim().Length == 0)
            {
                sb.Append(log).Append("\n");
            } else
            {
                sb.Append(log.Key).Append(":").Append(log.Value).Append("\n");
            }
        }
        display.text = sb.ToString();
    }
}
