using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This file was created by Peter Vataščin
// 19/04/2024

// A script for the three buttons for entering a letter code on the wheel box / code machine
public class ButtonBehavior : MonoBehaviour
{
    private NetworkUIManager nium;

    // variable for button animation
    private bool isClicking = false;
    private bool isReturning = false;

    [SerializeField]
    private float buttonMovementRange = 0.003f;

    [SerializeField]
    private float speed;

    private Vector3 originalPosition;

    private int id;

    // if the correct code is entered, the players can't interact with the buttons anymore
    private bool locked = false;

    private void Awake()
    {
        nium = FindFirstObjectByType<NetworkUIManager>();
        originalPosition = transform.localPosition;

        // saving the button ID in a local variable
        switch (transform.name)
        {
            case "Button1":
                id = 1;
                break;
            case "Button2":
                id = 2;
                break;
            case "Button3":
                id = 3;
                break;
        }
    }

    // code for the button click animation
    private void FixedUpdate()
    {
        // if a button has been clicked
        if (isClicking)
        {
            // if it reached the lowest point and now is returning to the original position
            if (isReturning)
            {
                // if the original position has been reached, stop animation
                if (transform.localPosition.z + speed >= originalPosition.z)
                {
                    transform.localPosition = originalPosition;
                    isReturning = false;
                    isClicking = false;
                } else
                {
                    // move the button up
                    transform.Translate(0, 0, speed);
                }
            } else
            {
                // if the button reached the lowest point of the animation
                if (transform.localPosition.z - speed <= originalPosition.z - buttonMovementRange)
                {
                    // start returning to the original position
                    transform.localPosition = new Vector3(originalPosition.x, originalPosition.y, originalPosition.z - buttonMovementRange);
                    isReturning = true;
                } else
                {
                    // move the button down
                    transform.Translate(0, 0, -speed);
                }
            }
            
        }
    }

    // stop the button from being clickable
    public void Lock()
    {
        locked = false;
    }

    public void ButtonClick()
    {
        // if the button animation os running or if the button has been locked, return
        if (isClicking || locked) return;

        // start button animation
        isClicking = true;

        // call NetworkUIManager.cs's method and pass it the button's ID
        nium.ButtonClick(id);
    }
}
