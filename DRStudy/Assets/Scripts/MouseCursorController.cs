using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseCursorController : MonoBehaviour
{
    private RectTransform canvasRect;
    private RectTransform cursorRect;
    private PointerEventData pointerEventData;

    /// <summary>
    /// Vector3 that defines the ratio of physical mouse movement that corresponds to the virtual mouse cursor's movement on the canvas.
    /// Usage: multiply this component-wise with the physical mouse coordinates to obtain the virtual cursor's coordinates before the change in coordinate system.
    /// Note: the physical mouse uses a bottom-left reference system, whereas Unity UI GameObjects use center reference systems.
    /// </summary>
    private Vector3 movementAdjustment;

    /// <summary>
    /// The offset that defines
    /// </summary>
    private Vector3 offset;

    /// <summary>
    /// Empty, unused game object whose sole purpose in this grim life is to disable mouse clicks for the physical mouse.
    /// </summary>
    private GameObject lastSelect;

    private void Start()
    {
        DisablePhysicalMouse();
        InitializeComponents();
        ComputeAdjustments();
    }

    private void Update()
    {
        UpdateCursorPosition();
        CheckForClick();
    }

    private void UpdateCursorPosition()
    {
        // Set the custom cursor position
        cursorRect.localPosition = Vector3.Scale(Input.mousePosition, movementAdjustment) - offset;

        cursorRect.pivot = new Vector2(0.0f, 1.0f); // Top left pivot
    }

    private void CheckForClick()
    {
        if (Input.GetMouseButtonDown((int) MouseButton.Left))
        {
            // transform.position is in world coordinates, we need to convert it to screen coordinates to be able to raycast
            pointerEventData.position = Camera.main.WorldToScreenPoint(cursorRect.position);
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            foreach (RaycastResult result in raycastResults)
            {
                if (result.gameObject.GetComponent<Button>())
                {
                    ExecuteEvents.Execute(result.gameObject, pointerEventData, ExecuteEvents.pointerClickHandler);
                    break;
                }
            }
        }
    }

    private void DisablePhysicalMouse()
    {
        Cursor.visible = false;
    }

    private void InitializeComponents()
    {
        lastSelect = new GameObject();
        if ((canvasRect = transform.parent.GetComponent<RectTransform>()).IsUnityNull())
        {
            canvasRect = transform.parent.AddComponent<RectTransform>();
        }
        if ((cursorRect = gameObject.GetComponent<RectTransform>()).IsUnityNull())
        {
            cursorRect = gameObject.AddComponent<RectTransform>();
        }
        pointerEventData = new PointerEventData(EventSystem.current);
    }

    /// <summary>
    /// Compute values for <see cref="movementAdjustment"/> and <see cref="offset"/>, which govern the movement of the virtual mouse cursor in relation to the physical mouse cursor
    /// </summary>
    private void ComputeAdjustments()
    {
        float widthRatio = canvasRect.rect.width / Screen.width;
        float heighRatio = canvasRect.rect.height / Screen.height;
        movementAdjustment = new(widthRatio, heighRatio);
        offset = new(canvasRect.rect.width / 2.0f, canvasRect.rect.height / 2.0f);
    }
}
