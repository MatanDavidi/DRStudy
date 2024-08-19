using UnityEngine;

public class MovementController : MonoBehaviour
{
    public float movementDelta;
    public float acceleratedMovementDelta;
    public float rotationDelta;
    public float acceleratedRotationDelta;

    public OVRInput.Button accelerationButton = OVRInput.Button.SecondaryIndexTrigger;
    public OVRInput.Button rotationButton = OVRInput.Button.SecondaryHandTrigger;

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Up, OVRInput.Controller.LTouch))
        {
            MoveOrRotate(Vector3.up);
        } else if (OVRInput.GetDown(OVRInput.Button.Down, OVRInput.Controller.LTouch))
        {
            MoveOrRotate(Vector3.down);
        } else if (OVRInput.GetDown(OVRInput.Button.Up, OVRInput.Controller.RTouch))
        {
            MoveOrRotate(Vector3.forward);
        } else if (OVRInput.GetDown(OVRInput.Button.Down, OVRInput.Controller.RTouch))
        {
            MoveOrRotate(Vector3.back);
        } else if (OVRInput.GetDown(OVRInput.Button.Left))
        {
            MoveOrRotate(Vector3.left);
        } else if (OVRInput.GetDown(OVRInput.Button.Right))
        {
            MoveOrRotate(Vector3.right);
        }
    }

    private void MoveOrRotate(Vector3 movementDirection)
    {
        if (OVRInput.Get(rotationButton))
        {
            RotateRotation(movementDirection);

        } else
        {
            MovePosition(movementDirection);
        }
    }

    private void MovePosition(Vector3 movementDirection)
    {
        if (OVRInput.Get(accelerationButton))
        {
            transform.position += movementDirection * acceleratedMovementDelta;
        } else
        {
            transform.position += movementDirection * movementDelta;
        }
    }

    private void RotateRotation(Vector3 movementDirection)
    {
        if (OVRInput.Get(accelerationButton))
        {
            transform.Rotate(movementDirection * acceleratedRotationDelta);
        }
        else
        {
            transform.Rotate(movementDirection * rotationDelta);
        }
    }
}
