using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This file was created by Peter Vataščin
// 19/04/2024

// Script taking care of the behaviour of pieces of the sliding puzzle
public class PuzzlePieceScript : MonoBehaviour
{
    // <2, 9>
    private int id;

    // position of the piece
    private float x;
    private float y;

    // not set in editor, because there is too many pieces to be set individually
    private float moveSpeed = 0.0001f;
    private void Awake()
    {
        // find the piece's ID
        id = System.Int32.Parse(transform.name.Substring(transform.name.Length - 1));

        // get the piece's position
        x = transform.localPosition.x;
        y = transform.localPosition.y;
    }

    // animation of the piece sliding
    private void FixedUpdate()
    {
        // get the piece's current position
        float curX = transform.localPosition.x;
        float curY = transform.localPosition.y;

        // if it is not equal to the desired destination update it
        if (curX > x)
        {
            curX -= moveSpeed;
        }

        if (curX < x)
        {
            curX += moveSpeed;
            if (curX > x) curX = x;
        }

        if (curY > y)
        {
            curY -= moveSpeed;
        }

        if (curY < y)
        {
            curY += moveSpeed;
            if (curY > y) curY = y;
        }

        // update the position
        transform.localPosition = new Vector3(curX, curY, transform.localPosition.z);
    }

    // this method is called from the InputManager.cs when the player clicked on the puzzle piece
    public void Clicked()
    {
        // Debug.Log("Piece " + id + " clicked!"); //debugging

        // call the puzzle box's method notifying it which piece was clicked
        transform.parent.GetComponent<PuzzleBoxScript>().PieceClicked(id);
    }

    // move the piece given distance in a given direction 
    public void Move(PuzzleBoxScript.Directions dir, float distance)
    {
        switch (dir)
        {
            case PuzzleBoxScript.Directions.Up:
                x += distance;
                break;
            case PuzzleBoxScript.Directions.Down:
                x -= distance;
                break;
            case PuzzleBoxScript.Directions.Left:
                y -= distance;
                break;
            case PuzzleBoxScript.Directions.Right:
                y += distance;
                break;
        }
    }
}
