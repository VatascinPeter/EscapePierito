using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

// This file was created by Peter Vataščin, building on implementation from https://www.youtube.com/watch?v=lYDfV-GaKQA&t=3s
// 19/04/2024

// A script taking care of inputs from the players (touching the screen)
public class InputManager : MonoBehaviour
{
    // finds the main camera in the scene, this is needed for raycasting
    private Camera mainCamera;
    private void Awake()
    {
        mainCamera = Camera.main;
    }

    // subscribe the FingerDown method to the onFingerDown event
    // When the onFingerDown event is triggered, the FingerDown method is called
    private void OnEnable()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
    }

    private void OnDisable()
    {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
    }

    // the method called when the player touches the screen
    private void FingerDown(EnhancedTouch.Finger finger)
    {
        // disable multiple touch
        if (finger.index != 0) return;

        // calls a method that creates a ray from the camera from the position on the screen that has been touched
        Ray ray = mainCamera.ScreenPointToRay(finger.screenPosition);
        RaycastHit hit;

        // cast the ray and save the object that the ray collided with
        if (Physics.Raycast(ray, out hit))
        {
            // if the object has a tag "TestCube", call the Change method, which changes the rendered material
            // -- this was used for testing and is not used in the final game
            if (hit.transform.CompareTag("TestCube"))
            {
                ChangeMaterial tcs = hit.transform.GetComponent<ChangeMaterial>();
                tcs.Change();
            }

            // if the object has a tag "Button" call the ButtonClick method, which notifies the NetworkUIManager which button has been clicked
            // the following two calls to an object script could be handled directly here, but I prefer it to be clearly differentiated and have the functionality in its own scipts
            if (hit.transform.CompareTag("Button"))
            {
                ButtonBehavior button = hit.transform.GetComponent<ButtonBehavior>();
                button.ButtonClick();
            }

            // if the object has a tag "PuzzlePiece" call the ButtonClick method, which notifies the NetworkUIManager which piece of the sliding puzzle has been clicked
            if (hit.transform.CompareTag("PuzzlePiece"))
            {
                PuzzlePieceScript piece = hit.transform.GetComponent<PuzzlePieceScript>();
                piece.Clicked();
            }
        }

    }
}
