using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This file was created by Peter Vataščin
// 19/04/2024

// script for the wheel box / code machine for entering a secret code
public class WheelBox : MonoBehaviour
{
    // material of the wheel box that is rendered when the players enter the right code
    [SerializeField]
    private Material solvedMaterial;

    // speed of rotation of the letter wheels
    [SerializeField]
    private float rotationSpeed;

    // is the correct code entered
    private bool solved = false;

    // references to the three letter wheels
    private Transform wheel1;
    private Transform wheel2;
    private Transform wheel3;

    // number of letters on each wheel
    private readonly int MaxInt = 10;

    // current value (letter) of each wheel
    private int code1 = 0;
    private int code2 = 0;
    private int code3 = 0;

    // desired angle of each wheel
    private float angle1 = 0f;
    private float angle2 = 0f;
    private float angle3 = 0f;

    // angle difference between two letters
    private float stepAngle;

    // the correct code
    private readonly int[] correctCode = {8, 2, 5 }; // 0...9 = A...J -> 8 = I, 2 = C, 5 = F

    private NetworkUIManager nium;

    private void Awake()
    {
        nium = FindFirstObjectByType<NetworkUIManager>();
        stepAngle = 360f / (float)MaxInt;
        wheel1 = transform.Find("Wheel1");
        wheel2 = transform.Find("Wheel2");
        wheel3 = transform.Find("Wheel3");

        // TEST

        // InvokeRepeating("Inc1", 0, 1);
        // InvokeRepeating("Inc2", 1, 2);
        // InvokeRepeating("Inc3", 0, 5);

        // TEST END
    }

    // TEST
    private void Inc1()
    {
        RotateWheel(1);
    }

    private void Inc2()
    {
        RotateWheel(2);
    }

    private void Inc3()
    {
        RotateWheel(3);
    }
    // TEST END


    private void FixedUpdate()
    {
        // Animation of the rotation of the first wheel
        if (angle1 != code1 * stepAngle)
        {
            float nextAngle = (angle1 + rotationSpeed) % 360f;
            if (nextAngle >= code1 * stepAngle && nextAngle <= (code1 + 1) * stepAngle) angle1 = code1 * stepAngle;
            else angle1 = nextAngle;

            if (angle1 >= 360) angle1 = angle1 % 360f;

            wheel1.localRotation = Quaternion.Euler(90, 0, angle1);
        }

        // Animation of the rotation of the second wheel
        if (angle2 != code2 * stepAngle)
        {
            float nextAngle = (angle2 + rotationSpeed) % 360f;
            if (nextAngle >= code2 * stepAngle && nextAngle <= (code2 + 1) * stepAngle) angle2 = code2 * stepAngle;
            else angle2 = nextAngle;

            if (angle2 >= 360) angle2 = angle2 % 360f;

            wheel2.localRotation = Quaternion.Euler(90, 0, angle2);
        }

        // Animation of the rotation of the third wheel
        if (angle3 != code3 * stepAngle)
        {
            float nextAngle = (angle3 + rotationSpeed) % 360f;
            if (nextAngle >= code3 * stepAngle && nextAngle <= (code3 + 1) * stepAngle) angle3 = code3 * stepAngle;
            else angle3 = nextAngle;

            if (angle3 >= 360) angle3 = angle3 % 360f;

            wheel3.localRotation = Quaternion.Euler(90, 0, angle3);
        }
    }

    // called by NetworkUIManager when a button is clicked
    public void RotateWheel(int wheelNum)
    {
        // dont move wheels if the correct code is entered
        if (solved) return;

        // update the entered code and start wheel rotation animation
        switch (wheelNum)
        {
            case 1:
                code1 += 1;
                while (code1 >= MaxInt) code1 = code1 % MaxInt;
                break;
            case 2:
                code2 += 1;
                while (code2 >= MaxInt) code2 = code2 % MaxInt;
                break;
            case 3:
                code3 += 1;
                while (code3 >= MaxInt) code3 = code3 % MaxInt;
                break;
        }

        // if the correct code is entered, mark solved
        if (code1 == correctCode[0] && code2 == correctCode[1] && code3 == correctCode[2])
        {
            Solved();
        }
    }

    // if the correct code is entered notify the NetworkUIManager and change material
    public void Solved()
    {
        solved = true;
        transform.GetComponent<Renderer>().material = solvedMaterial;
        nium.Task2Solved();
    }
}
