using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ImageTargetScaler : MonoBehaviour {

    private static readonly float PanSpeed = 40f;
    private static readonly float scaleSpeedTouch = 2.5f;
    private static readonly float scaleSpeedMouse = 20f;

    //private static readonly float[] BoundsX = new float[] { 500f, 1500f };
    //private static readonly float[] BoundsZ = new float[] { -500f, 500f };

    private Vector3 lastPanPosition;

    private int panFingerId; // Touch mode only
    private bool wasZoomingLastFrame; // Touch mode only
    private Vector2[] lastZoomPositions; // Touch mode only

    void Update()
    {
        if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
        {
            HandleTouch();
        }
        else
        {
            HandleMouse();
        }
    }

    void HandleTouch()
    {
        switch (Input.touchCount)
        {
            /* Turned off panning until character transform is fixed.
            case 1:
                wasZoomingLastFrame = false;

                // If the touch began, capture its position and its finger ID.
                // Otherwise, if the finger ID of the touch doesn't match, skip it.
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    lastPanPosition = touch.position;
                    panFingerId = touch.fingerId;
                }
                else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
                {
                    PanDungeon(touch.position);
                }
                break;
                */
            case 2:
                Vector2[] newPositions = new Vector2[] { Input.GetTouch(0).position, Input.GetTouch(1).position };
                if (!wasZoomingLastFrame)
                {
                    lastZoomPositions = newPositions;
                    wasZoomingLastFrame = true;
                }
                else
                {
                    // Zoom based on the distance between the new positions compared to the 
                    // distance between the previous positions.
                    float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                    float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                    float offset = newDistance - oldDistance;

                    ScaleImageTarget(offset, scaleSpeedTouch);

                    lastZoomPositions = newPositions;
                }
                break;
            default:
                wasZoomingLastFrame = false;
                break;
        }
    }

    void HandleMouse()
    {
        // On mouse down, capture it's position.
        // Otherwise, if the mouse is still down, pan the camera.
        /*
        if (Input.GetMouseButtonDown(0))
        {
            lastPanPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            PanDungeon(Input.mousePosition);
        }
        */

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        ScaleImageTarget(scroll, scaleSpeedMouse);
    }

    void PanDungeon(Vector3 newPanPosition)
    {
        // Determine how much to move the camera
        Vector3 offset = Camera.main.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        Vector3 move = new Vector3(offset.x * PanSpeed, 0, offset.y * PanSpeed);

        // Perform the movement
        transform.Translate(move * -1, Space.World);

        // Ensure the camera remains within bounds.
        //Vector3 pos = transform.position;
        //pos.x = Mathf.Clamp(transform.position.x, BoundsX[0], BoundsX[1]);
        //pos.z = Mathf.Clamp(transform.position.z, BoundsZ[0], BoundsZ[1]);
        //transform.position = pos;

        // Cache the position
        lastPanPosition = newPanPosition;
    }

    void ScaleImageTarget(float offset, float speed)
    {
        if (offset == 0)
        {
            return;
        }

        float xyz = (.1f * offset) * speed;
        if (transform.localScale.x >= .5f && transform.localScale.x <= 15f)
        {
            transform.localScale += new Vector3(xyz, xyz, xyz);
            if(transform.localScale.x < .5f)
            {
                transform.localScale = new Vector3(.5f, .5f, .5f);
            }else if (transform.localScale.x > 15f)
            {
                transform.localScale = new Vector3(15f, 15f, 15f);
            }
        }
    }

}