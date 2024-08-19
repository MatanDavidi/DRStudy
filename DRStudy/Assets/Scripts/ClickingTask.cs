using Assets.Scripts;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ClickingTask : MonoBehaviour
{
    #region Serialized field
    [SerializeField]
    private Canvas mainCanvas;
    /// <summary>
    /// Panel on which to run the entire task.
    /// </summary>
    [SerializeField]
    private GridLayoutGroup buttonsPanel;
    /// <summary>
    /// GameObject prefab to instantiate and add to <see cref="buttonsPanel"/>
    /// </summary>
    [SerializeField]
    private GameObject buttonPrefab;
    [SerializeField]
    private CustomTimerScript timer;
    /// <summary>
    /// RectTransform component of the main panel's container.
    /// </summary>
    public RectTransform parentTransform;
    /// <summary>
    /// RectTransform component of the main panel's sibling.
    /// </summary>
    [SerializeField]
    private RectTransform siblingTransform;
    /// <summary>
    /// TextMeshPro UI Text component to provide instructions to the participant.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI instructionsText;
    [SerializeField]
    private GameObject virtualCursor;
    #endregion

    #region Private fields
    /// <summary>
    /// Array of buttons added to the panel <see cref="buttonsPanel"/>
    /// </summary>
    private readonly ClickableButton[] buttons = new ClickableButton[BUTTONS_NUMBER];
    /// <summary>
    /// StreamWriter used to write to output file.
    /// </summary>
    private StreamWriter sw;
    /// <summary>
    /// List containing all labels to be assigned to the buttons.
    /// </summary>
    private List<int> buttonLabels;
    /// <summary>
    /// List containing the indices of the buttons to activate in activation order.
    /// </summary>
    private List<int> targetIndices;
    /// <summary>
    /// Counter to keep track of the number of the target that needs to be clicked.
    /// </summary>
    private int targetCounter = 1;
    /// <summary>
    /// Participant's ID defined in the Participant Data Canvas UI.
    /// </summary>
    private int participantId;
    /// <summary>
    /// Participant's condition defined in the Participant Data Canvas UI.
    /// </summary>
    private StudyConditions participantCondition;
    #endregion

    #region Constants
    /// <summary>
    /// Timeout between when the user clicks on the START button and when the first button activates.
    /// </summary>
    private const float START_TIMEOUT = 5.0f;
    /// <summary>
    /// Number of buttons to generate.
    /// </summary>
    public const int BUTTONS_NUMBER = 100;
    /// <summary>
    /// Number of buttons to activate for the user to click.
    /// </summary>
    private const int TARGETS_NUMBER = 100;
    /// <summary>
    /// Number of targets for which only a single button activates at a time (compared to activating <see cref="SECOND_PART_ADDITIONAL_BUTTONS_TO_ACTIVATE"/> buttons)
    /// </summary>
    private const int SINGLE_BUTTON_TARGETS_NUMBER = 50;
    /// <summary>
    /// Number of buttons to activate on the second part ALONGSIDE the main one, when multiple buttons get activated simultaneously and only one of them should be clicked.
    /// </summary>
    public const int SECOND_PART_ADDITIONAL_BUTTONS_TO_ACTIVATE = 1;
    /// <summary>
    /// Number of columns by which to constrain grid layout (in a layman's terms, the buttons grid will always have exactly <see cref="GRID_COLUMNS_NUMBER"/> columns).
    /// </summary>
    private const int GRID_COLUMNS_NUMBER = 10;
    /// <summary>
    /// Directory in which the output file is found, relative to project base directory.
    /// </summary>
    private string OUTPUT_FILE_PATH;
    /// <summary>
    /// Header (i.e. first line) of the output CSV file.
    /// </summary>
    private const string OUTPUT_FILE_HEADER = "participantId,condition,timeout,misclick,mouseX,mouseY,targetX,targetY,shadow1X,shadow1Y,targetWidth,targetHeight,targetLabel,enableTimestamp,disableTimestamp";
    /// <summary>
    /// Text to be displayed to give instructions to the participant.
    /// </summary>
    private const string INSTRUCTIONS_TEXT = "Please press button ##BUTTON_TEXT##";
    /// <summary>
    /// Height of the instructions text.
    /// </summary>
    private const float SIBLING_TEXT_HEIGHT = 80.44f;
    #endregion

    public void SetParticipantData(int id, StudyConditions condition)
    {
        participantId = id;
        participantCondition = condition;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (mainCanvas.IsUnityNull())
        {
            Debug.LogError("[ERROR] Could not find main canvas. Terminating execution.");
            return;
        }
        // Initialize OUTPUT_FILE_PATH with persistent data path directory + test_output.csv
        OUTPUT_FILE_PATH = Path.Combine(Application.persistentDataPath, "test_output.csv");

        StartScreenManager ssl = GetComponent<StartScreenManager>();
        if (ssl.IsUnityNull())
        {
            ssl = gameObject.AddComponent<StartScreenManager>();
            ssl.dependencies = new GameObject[0];
        }

        ssl.LoadStartScreen(this, mainCanvas);
    }

    public void StartClickingTask()
    {
        InitializeComponents();
        InitializeFileWriter();
        InitializeButtons();
    }

    private void InitializeComponents()
    {
        // Buttons panel
        Debug.Log("[DEBUG] Initializing buttons panel");
        if (buttonsPanel.IsUnityNull())
        {
            Debug.LogError("[ERROR] Could not find GridLayoutGroup GameObject. Terminating execution.");
            return;
        }
        buttonsPanel.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        buttonsPanel.constraintCount = GRID_COLUMNS_NUMBER;
        int rowsNumber = BUTTONS_NUMBER / GRID_COLUMNS_NUMBER;
        buttonsPanel.cellSize = new(parentTransform.sizeDelta.x / GRID_COLUMNS_NUMBER, (parentTransform.sizeDelta.y - SIBLING_TEXT_HEIGHT) / rowsNumber);
        Debug.Log("[DEBUG] Buttons panel initialized");

        // Custom timer script
        Debug.Log("[DEBUG] Initializing custom timer script");
        if (timer.IsUnityNull())
        {
            timer = gameObject.AddComponent<CustomTimerScript>();
        }
        Debug.Log("[DEBUG] Custom timer script initialized");

        // List of labels (random, without dupes)
        InitializeLabelsList();
        // List of indices of buttons to activate (random, without dupes)
        InitializeTargetIndicesList();
    }

    private void InitializeLabelsList()
    {
        buttonLabels = UtilityMethods.CreateAndInitializeList(0, BUTTONS_NUMBER);
        buttonLabels = UtilityMethods.ShuffleAndFilterList(buttonLabels, BUTTONS_NUMBER);
    }

    private void InitializeTargetIndicesList()
    {
        targetIndices = UtilityMethods.CreateAndInitializeList(0, BUTTONS_NUMBER);
        targetIndices = UtilityMethods.ShuffleAndFilterList(buttonLabels, TARGETS_NUMBER);
    }

    private void InitializeFileWriter()
    {
        // If the output file does not exist, create it and write the header.
        if (!File.Exists(OUTPUT_FILE_PATH))
        {
            UtilityMethods.AddFirstLine(OUTPUT_FILE_HEADER, OUTPUT_FILE_PATH);
        }
        else
        {
            // If the first line of the file is not the header, write the header
            string[] outputLines = File.ReadAllLines(OUTPUT_FILE_PATH);
            if (outputLines.Length == 0 ||
                !File.ReadAllLines(OUTPUT_FILE_PATH)[0].Equals(OUTPUT_FILE_HEADER))
            {
                UtilityMethods.AddFirstLine(OUTPUT_FILE_HEADER, OUTPUT_FILE_PATH);
            }
        }
        // Open file writing stream on output file.
        sw = File.AppendText(OUTPUT_FILE_PATH);
    }

    private void InitializeButtons()
    {
        if (!buttonPrefab.IsUnityNull())
        {
            for (int i = 0; i < BUTTONS_NUMBER; i++)
            {
                // Instantiate base button
                GameObject newButton = Instantiate(buttonPrefab, buttonsPanel.transform);
                // Add newly-instantiated button to array.
                buttons[i] = new ClickableButton(newButton, participantId, participantCondition, UtilityMethods.ReadAndRemoveHead(buttonLabels).ToString("00"), sw, timer, this, virtualCursor);
            }
            // Create and start timer to activate the buttons
            Invoke(nameof(ActivateNextButtons), START_TIMEOUT);
        }
        else
        {
            Debug.LogError("[ERROR] Could not find buttons prefab or virtual cursor. No buttons will be added.");
        }
    }

    public void ActivateNextButtons()
    {
        if (targetIndices.Count > 0)
        {
            ClickableButton[] shadows = new ClickableButton[SECOND_PART_ADDITIONAL_BUTTONS_TO_ACTIVATE]; ;
            if (targetCounter > SINGLE_BUTTON_TARGETS_NUMBER)
            {
                for (int i = 0; i < SECOND_PART_ADDITIONAL_BUTTONS_TO_ACTIVATE; i++)
                {
                    int nextShadowIndex = UtilityMethods.ReadAndRemoveHead(targetIndices);
                    buttons[nextShadowIndex].SoftActivate();
                    shadows[i] = buttons[nextShadowIndex];
                }
            }
            // Get next button, and activate it.
            int nextButtonIndex = UtilityMethods.ReadAndRemoveHead(targetIndices);
            buttons[nextButtonIndex].Activate(shadows);
            string newInstructions = INSTRUCTIONS_TEXT.Replace("##BUTTON_TEXT##", buttons[nextButtonIndex].Text);
            instructionsText.text = newInstructions;
            ++targetCounter;
        } else
        {
            Debug.Log("[DEBUG] All targets have been started, shutting down.");
            sw.Close();
            GetComponent<EndScreenManager>().LoadEndScreen(mainCanvas);
            Debug.Log("[DEBUG] Stream writer has been closed.");

        }
    }
}

public enum StudyConditions
{
    Physical,
    PhysicalDiminishedDistractions,
    XR,
    XRDiminishedDistractions
}