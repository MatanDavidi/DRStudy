using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ClickableButton
    {
        #region Constants
        /// <summary>
        /// Timeout before a button click is considered failed.
        /// </summary>
        private const float BUTTON_TIMEOUT = 3.0f;
        /// <summary>
        /// Color of an enabled button's text.
        /// </summary>
        private readonly Color ENABLED_TEXT_COLOR = Color.green;
        /// <summary>
        /// Background color of an enabled button.
        /// </summary>
        private readonly Color ENABLED_BACKGROUND_COLOR = new Color(.9215686274509804f, .2509803921568627f, .203921568627451f);
        /// <summary>
        /// Color of a disabled button's text.
        /// </summary>
        private readonly Color DISABLED_TEXT_COLOR = new Color(0.196f, 0.196f, 0.196f);
        /// <summary>
        /// Background color of a disabled button.
        /// </summary>
        private readonly Color DISABLED_BACKGROUND_COLOR = Color.white;
        #endregion

        #region Private fields
        /// <summary>
        /// Timestamp in Unix time of when the button was activated.
        /// </summary>
        private long activationTimestamp;
        /// <summary>
        /// Mouse position at the time of activation of this button.
        /// </summary>
        private Vector3 mousePositionOnActivation;
        /// <summary>
        /// StreamWriter that allows to write to the file.
        /// </summary>
        private StreamWriter StreamWriter { get; set; }
        /// <summary>
        /// Button component attached to baseButton GameObject.
        /// </summary>
        private Button buttonComponent;
        /// <summary>
        /// RectTransform component attached to baseButton GameObject.
        /// </summary>
        private RectTransform buttonRect;
        /// <summary>
        /// Image component attached to baseButton GameObject.
        /// </summary>
        private Image buttonImage;
        /// <summary>
        /// TextMeshPro Text component attached to the button's child "Text" GameObject.
        /// </summary>
        private TextMeshProUGUI textComponent;
        /// <summary>
        /// RectTransform component attached to the button's GameObject.
        /// </summary>
        private RectTransform rectTransformComponent;
        /// <summary>
        /// Boolean flag to keep track of whether the button is active or not (for misclicks and other internal logic).
        /// </summary>
        private bool active = false;
        /// <summary>
        /// String containing the text contents of a row that corresponds to a data report of a button without shadows.
        /// </summary>
        private StringBuilder emptyShadowsString;
        #endregion

        #region Properties
        public string Text { get; set; }
        public GameObject BaseButton { get; set; }
        public int ParticipantId { get; set; }
        public StudyConditions StudyCondition { get; set; }
        private CustomTimerScript CustomTimer { get; set; }
        private ClickingTask MainTask { get; set; }
        public GameObject VirtualCursor { get; set; }
        /// <summary>
        /// Other buttons that were soft activated along the proper activation of this one (if any -- may be empty)
        /// </summary>
        public ClickableButton[] Shadows { get; set; }
        #endregion

        #region Constructors
        public ClickableButton(GameObject baseButton, int participantId, StudyConditions studyCondition, string buttonLabel, StreamWriter streamWriter, CustomTimerScript timer, ClickingTask mainTask, GameObject virtualCursor)
        {
            Text = buttonLabel;
            BaseButton = baseButton;
            StudyCondition = studyCondition;
            ParticipantId = participantId;
            ParticipantId = participantId;
            StreamWriter = streamWriter;
            CustomTimer = timer;
            MainTask = mainTask;
            VirtualCursor = virtualCursor;
            InitializeButton(buttonLabel);

            emptyShadowsString = new();
            for (int i = 0; i < ClickingTask.SECOND_PART_ADDITIONAL_BUTTONS_TO_ACTIVATE; i++)
            {
                if (i < ClickingTask.SECOND_PART_ADDITIONAL_BUTTONS_TO_ACTIVATE - 1)
                    emptyShadowsString.Append($"{float.NaN},{float.NaN},");
                else
                    emptyShadowsString.Append($"{float.NaN},{float.NaN}");
            }
        }
        #endregion

        private void InitializeButton(string buttonLabel)
        {
            Debug.Log($"[DEBUG] Initializing button {buttonLabel}");
            // Get TextMeshProUGUI component from button
            textComponent = BaseButton.GetComponentInChildren<TextMeshProUGUI>();
            // Get RectTransform component from button
            rectTransformComponent = BaseButton.GetComponentInChildren<RectTransform>();
            // Change button's text to random 2-digit number
            textComponent.text = buttonLabel;
            // Get button's UI button component
            buttonComponent = BaseButton.GetComponent<Button>();
            // Get button's RectTransform component
            buttonRect = BaseButton.GetComponent<RectTransform>();
            // Get button's Image component
            buttonImage = BaseButton.GetComponent<Image>();
            // Add on click event listener to button, redirecting to method <see href="OnButtonClick">OnButtonClick</see>
            buttonComponent.onClick.AddListener(OnButtonClick);
            Debug.Log($"[DEBUG] Button {textComponent.text} initialized");
        }

        public void Activate(ClickableButton[] shadows)
        {
            Shadows = shadows;

            // Collecting data for future writing to file
            if (VirtualCursor.IsUnityNull())
            {
                // VirtualCursor is null => use physical mouse
                RectTransformUtility.ScreenPointToLocalPointInRectangle(MainTask.parentTransform, Input.mousePosition, Camera.main, out Vector2 mousePosOnCanvas);
                mousePositionOnActivation = mousePosOnCanvas;
            }
            else
            {
                // VirtualCursor is not null => use virtual cursor
                mousePositionOnActivation = VirtualCursor.transform.position;
            }
            activationTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            Debug.Log($"[DEBUG] Activating button {textComponent.text} for user click");
            active = true;
            Debug.Log($"[DEBUG] Changing text {textComponent.text}'s color to {ENABLED_BACKGROUND_COLOR}");
            BaseButton.GetComponentInChildren<TextMeshProUGUI>().color = ENABLED_TEXT_COLOR;
            buttonImage.color = ENABLED_BACKGROUND_COLOR;
            // Timeout timer before button is enabled
            Debug.Log($"[DEBUG - {textComponent.text}] Creating and starting new timer");
            CustomTimer.StartCoroutine(ButtonTimeout());
        }

        public void SoftActivate()
        {
            Debug.Log($"[DEBUG] Changing text {textComponent.text}'s color to {ENABLED_BACKGROUND_COLOR}");
            BaseButton.GetComponentInChildren<TextMeshProUGUI>().color = ENABLED_TEXT_COLOR;
            buttonImage.color = ENABLED_BACKGROUND_COLOR;
        }

        public void Disable(bool timeout = false)
        {
            Debug.Log($"[DEBUG] Disabling button {textComponent.text}");
            active = false;
            BaseButton.GetComponentInChildren<TextMeshProUGUI>().color = DISABLED_TEXT_COLOR;
            buttonImage.color = DISABLED_BACKGROUND_COLOR;
            Debug.Log($"[DEBUG] Changing text {textComponent.text}'s color to {DISABLED_BACKGROUND_COLOR}");

            // Disable shadows as well, and get their position to write in file
            StringBuilder shadowPositionsStringBuilder = new(string.Empty);
            for (int i = 0; i < Shadows.Length; i++)
            {
                Vector2 shadowPosition;
                if (Shadows[i] != null)
                {
                    shadowPosition = Shadows[i].SoftDisable();
                } else
                {
                    shadowPosition = new(float.NaN, float.NaN);
                }

                if (i < Shadows.Length - 1)
                {
                    shadowPositionsStringBuilder.Append($"{shadowPosition.x},{shadowPosition.y},");
                }
                else
                {
                    shadowPositionsStringBuilder.Append($"{shadowPosition.x},{shadowPosition.y}");
                }
            }

            // Log this click as either a correct click or a timeout
            long clickTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            WriteData(timeout, false, mousePositionOnActivation, shadowPositionsStringBuilder.ToString(), activationTimestamp, clickTime);

            Debug.Log($"[DEBUG] Button {textComponent.text} disabled. Measured data: {ParticipantId},{timeout},{false},{mousePositionOnActivation.x:0.00000},{mousePositionOnActivation.y:0.00000},{BaseButton.transform.position.x},{BaseButton.transform.position.y},{shadowPositionsStringBuilder},{rectTransformComponent.sizeDelta.x:0.00000},{rectTransformComponent.sizeDelta.y:0.00000},{textComponent.text},{activationTimestamp},{clickTime}");
            MainTask.ActivateNextButtons();
        }

        public Vector2 SoftDisable()
        {
            BaseButton.GetComponentInChildren<TextMeshProUGUI>().color = DISABLED_TEXT_COLOR;
            buttonImage.color = DISABLED_BACKGROUND_COLOR;

            return BaseButton.transform.position;
        }

        private void OnButtonClick()
        {
            // If the virtual cursor is inside of the button's surface
            if (VirtualCursor.IsUnityNull() || RectTransformUtility.RectangleContainsScreenPoint(buttonRect, VirtualCursor.transform.position))
            {
                if (active)
                {
                    Debug.Log($"[DEBUG] Button {textComponent.text} was clicked BEFORE timeout");
                    Disable();
                }
                else
                {
                    // Log this click as a mistake
                    WriteData(false, true, new Vector2(float.NaN, float.NaN), emptyShadowsString.ToString(), -1, DateTimeOffset.Now.ToUnixTimeMilliseconds());
                }
            }
        }

        private IEnumerator ButtonTimeout()
        {
            yield return new WaitForSeconds(BUTTON_TIMEOUT);
            if (active)
            {
                Debug.Log($"[DEBUG] Button {textComponent.text} was NOT clicked and timed out");
                Disable(true);
            }
        }

        /// <summary>
        /// Writes given measured data to file as specified by given property <see cref="StreamWriter">StreamWriter</see>.
        /// </summary>
        /// <param name="timeout">true if the button was disabled due to a timeout, false otherwise.</param>
        /// <param name="misclick">true if the button was clicked but was not active, false otherwise.</param>
        /// <param name="mousePosition">the position of the mouse when the button was activated.</param>
        /// <param name="disableTimestamp">The timestamp at which the button was disabled</param>
        private void WriteData(bool timeout, bool misclick, Vector2 mousePosition, string shadowsPosition, long activationTimestamp, long disableTimestamp)
        {
            Vector2 targetPosition = BaseButton.transform.position;
            Vector2 targetDimensions = rectTransformComponent.sizeDelta;
            string targetLabel = textComponent.text;

            Debug.Log($"[DEBUG] Writing button {textComponent.text}'s data to file");
            string newLine = $"{ParticipantId},{StudyCondition},{timeout},{misclick},{mousePosition.x:0.00000},{mousePosition.y:0.00000},{targetPosition.x},{targetPosition.y},{shadowsPosition},{targetDimensions.x:0.00000},{targetDimensions.y:0.00000},{targetLabel},{activationTimestamp},{disableTimestamp}";
            // Write collected data to file
            StreamWriter.WriteLine(newLine);
            Debug.Log($"[DEBUG] Button {textComponent.text}'s data written to file");
        }
    }
}