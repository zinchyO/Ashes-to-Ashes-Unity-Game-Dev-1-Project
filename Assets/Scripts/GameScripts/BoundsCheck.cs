using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsCheck : MonoBehaviour
{
    // Enum to represent various screen locations
    [System.Flags]
    public enum eScreenLocations 
    {
        onScreen = 0,
        offRight = 1,
        offLeft = 2,
        offUp = 4,
        offDown = 8
    }

    // Enum to represent the type of bounds checking
    public enum eType {center, inset, outset}

    [Header("Inscribed")]
    // Variable to set the type of bounds checking
    public eType boundsType = eType.center;
    // Radius for bounds checking, adjust this to match half the width of the attached game object
    public float radius = 0.5f;
    // Variable to decide if the object should be kept on screen
    public bool keepOnScreen = true;

    [Header("Dynamic")]
    // Variable to store the current screen location
    public eScreenLocations screenLocs = eScreenLocations.onScreen;
    // Variables to store the camera width and height
    public float camWidth;
    public float camHeight;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Set the camera height and width
        camHeight = Camera.main.orthographicSize;
        camWidth = camHeight * Camera.main.aspect;
    }

    // LateUpdate is called every frame, if the Behaviour is enabled
    private void LateUpdate()
    {
        float checkRadius = 0;
        // Set the checkRadius based on the boundsType
        if (boundsType == eType.inset) checkRadius = -radius;
        if (boundsType == eType.outset) checkRadius = radius;

        Vector3 pos = transform.position;
        screenLocs = eScreenLocations.onScreen;

        // Check if the object is off the right or left of the screen
        if (pos.x > camWidth + checkRadius) { pos.x = camWidth + checkRadius; screenLocs |= eScreenLocations.offRight; }
        if (pos.x < -camWidth - checkRadius) { pos.x = -camWidth - checkRadius; screenLocs |= eScreenLocations.offLeft; }

        // Check if the object is off the top or bottom of the screen
        if (pos.y > camHeight + checkRadius) { pos.y = camHeight + checkRadius; screenLocs |= eScreenLocations.offUp; }
        if (pos.y < -camHeight - checkRadius) { pos.y = -camHeight - checkRadius; screenLocs |= eScreenLocations.offDown; }

        // If the object should be kept on screen and it's not, adjust its position
        if (keepOnScreen && !isOnScreen) 
        {
            transform.position = pos;
            screenLocs = eScreenLocations.onScreen;
        }
    }

    // Property to check if the object is on screen
    public bool isOnScreen 
    {
        get { return (screenLocs == eScreenLocations.onScreen); }
    }

    // Method to check if the object is at a specific screen location
    public bool LocIs(eScreenLocations checkLoc) 
    {
        if (checkLoc == eScreenLocations.onScreen) return isOnScreen;
        return screenLocs == checkLoc;
    }
}