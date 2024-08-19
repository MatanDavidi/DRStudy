using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ParticipantDataHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject clickerTaskCanvas;
    [SerializeField]
    private ClickingTask taskHandler;
    [SerializeField]
    private TMP_InputField participantIdInputField;
    [SerializeField]
    private TMP_Dropdown participantConditionDropdown;
    [SerializeField]
    private Button submitButton;

    // Start is called before the first frame update
    void Start()
    {
        if (!clickerTaskCanvas.IsUnityNull() &&
            !taskHandler.IsUnityNull() &&
            !participantIdInputField.IsUnityNull() &&
            !participantConditionDropdown.IsUnityNull())
        {
            submitButton.onClick.AddListener(() => {
                string participantIdString = participantIdInputField.text;
                int participantId = -1;
                if (participantIdString.Length > 0)
                {
                    participantId = Convert.ToInt32(participantIdString);
                }

                string conditionString = participantConditionDropdown.options[participantConditionDropdown.value].text;
                var participantCondition = conditionString switch
                {
                    "Physical" => StudyConditions.Physical,
                    "XR" => StudyConditions.XR,
                    "PhysicalDiminishedDistractions" => StudyConditions.PhysicalDiminishedDistractions,
                    _ => StudyConditions.XRDiminishedDistractions,
                };
                Debug.Log($"Submitting for main task following values: id = {participantId}, condition = {participantCondition}");
                clickerTaskCanvas.SetActive(true);
                taskHandler.SetParticipantData(participantId, participantCondition);
                gameObject.SetActive(false);
            });
        }
    }
}
