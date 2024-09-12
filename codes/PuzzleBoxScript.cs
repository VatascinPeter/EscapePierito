using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This file was created by Peter Vataščin
// 19/04/2024

// A script for the sliding puzzle
public class PuzzleBoxScript : MonoBehaviour
{
    // values specifying how the puzzle box cover should open - terminal angle, current angle (closed), opening speed (angular speed)
    [SerializeField]
    private float openAngle;
    private float currentAngle = 0f;
    [SerializeField]
    private float openingSpeed;

    // variable stating whether the box is open
    private bool open = false;

    // alternative materials for all the puzzle bo parts
    [SerializeField]
    private Material glassCoverMaterial;

    [SerializeField]
    private Material pieceMaterial2;
    [SerializeField]
    private Material pieceMaterial3;
    [SerializeField]
    private Material pieceMaterial4;
    [SerializeField]
    private Material pieceMaterial5;
    [SerializeField]
    private Material pieceMaterial6;
    [SerializeField]
    private Material pieceMaterial7;
    [SerializeField]
    private Material pieceMaterial8;
    [SerializeField]
    private Material pieceMaterial9;

    // saved reference to every sliding puzzle piece
    private GameObject piece2;
    private GameObject piece3;
    private GameObject piece4;
    private GameObject piece5;
    private GameObject piece6;
    private GameObject piece7;
    private GameObject piece8;
    private GameObject piece9;

    // references to the point around which the cover should open and the cover window
    private GameObject coverPivot;
    private GameObject coverWindow;

    private NetworkUIManager nium;

    // maximum absolute offset of pieces from the centre - how much a piece should move
    private float pieceOffset = 0.00105f;

    // variable stating whether the puzzle pieces can be moved
    private bool movable = false;

    // position of each piece gameBoard[0] = position of the empty space, gameBoard[1] = position of the piece 2 etc.
    private int[] gameBoard = { 0, 3, 5, 2, 8, 7, 4, 1, 6 }; 
    
    //                      |1|8|4|    |1|2|3|
    // - initial position = |2|7|3| -> |4|5|6| = final position
    //                      |9|6|5|    |7|8|9|

    // enumerates for directions to make the code clearer
    public enum Directions
    {
        Up,
        Down,
        Left,
        Right,
        Invalid
    }
    private void Awake()
    {
        nium = FindFirstObjectByType<NetworkUIManager>();

        piece2 = transform.Find("Piece2").gameObject;
        piece3 = transform.Find("Piece3").gameObject;
        piece4 = transform.Find("Piece4").gameObject;
        piece5 = transform.Find("Piece5").gameObject;
        piece6 = transform.Find("Piece6").gameObject;
        piece7 = transform.Find("Piece7").gameObject;
        piece8 = transform.Find("Piece8").gameObject;
        piece9 = transform.Find("Piece9").gameObject;

        coverPivot = transform.Find("CoverPivot").gameObject;
        coverWindow = transform.Find("CoverPivot").Find("PuzzleCover").Find("PuzzleCoverWindow").gameObject;

        // if the second scenario has been finished before the puzzle box has been instantiated
        if (nium.IsPuzzle3())
        {
            if (nium.Host())
            {
                // open the box and let the player move the pieces
                movable = true;
                OpenBox();
            } else
            {
                // make the cover transparent and let the puzzle pieces show the scrambled image
                MakeWindow();
                FillPieces();
            }
        }

        // TESTING
        /*
        Invoke("OpenBox", 0);
        Invoke("MakeWindow", 3);
        Invoke("FillPieces", 4);

        int[] solution = { 8, 4, 3, 5, 6, 7, 2, 8, 4, 2, 7, 9, 8, 7, 5, 6, 9, 8, 7, 4 };

        for (int i = 0; i < solution.Length; i++)
        {
            Invoke("Mov" + solution[i], 5 + i);
        }

        Invoke("Mov2", 5 + solution.Length);
        */
        // TESTING END
    }

    // TESTING


    private void Mov2()
    {
        PieceClicked(2);
    }

    private void Mov3()
    {
        PieceClicked(3);
    }

    private void Mov4()
    {
        PieceClicked(4);
    }

    private void Mov5()
    {
        PieceClicked(5);
    }

    private void Mov6()
    {
        PieceClicked(6);
    }

    private void Mov7()
    {
        PieceClicked(7);
    }

    private void Mov8()
    {
        PieceClicked(8);
    }

    private void Mov9()
    {
        PieceClicked(9);
    }

    // TESTING END

    private void FixedUpdate()
    {
        // Opening Animation
        if (open && currentAngle < openAngle)
        {
            currentAngle += openingSpeed;
            if (currentAngle > openAngle) currentAngle = openAngle;

            coverPivot.transform.Rotate(new Vector3(0, 0, -openingSpeed));
        }
    }

    // start the opening animation and let the puzzle pieces be moved by the player
    public void OpenBox()
    {
        open = true;
        movable = true;
    }

    // make the box cover transparent
    public void MakeWindow()
    {
        coverWindow.GetComponent<Renderer>().material = glassCoverMaterial;
    }

    // render the puzzle pieces displaying the scrambled image
    public void FillPieces()
    {
        piece2.GetComponent<Renderer>().material = pieceMaterial2;
        piece3.GetComponent<Renderer>().material = pieceMaterial3;
        piece4.GetComponent<Renderer>().material = pieceMaterial4;
        piece5.GetComponent<Renderer>().material = pieceMaterial5;
        piece6.GetComponent<Renderer>().material = pieceMaterial6;
        piece7.GetComponent<Renderer>().material = pieceMaterial7;
        piece8.GetComponent<Renderer>().material = pieceMaterial8;
        piece9.GetComponent<Renderer>().materials = new Material[] { pieceMaterial9, pieceMaterial9 };
    }

    // a method called by a puzzle piece when it is clicked passing it the piece's ID
    public void PieceClicked(int stringId) 
    {
        if (!movable) return;
        
        // convert id [1-9] to [0-8]
        int id = stringId - 1;
        // check if the piece that was clicked is adjacent to the empty place, if yes, return in which direction the piece can move
        Directions adjacency = IsAdjacent(gameBoard[0], gameBoard[id]);
        // if the piece is not adjacent to an empty space, return
        if (adjacency == Directions.Invalid) return;

        // move piece in the direction of the empty place and notify the NetworkUIManager
        MovePiece(stringId, adjacency);
        nium.PieceMoved(stringId, adjacency);

        // update the local representation of the puzzle
        int tmp = gameBoard[0];
        gameBoard[0] = gameBoard[id];
        gameBoard[id] = tmp;

        // check if the puzzle is solved, if yes stop the pieces from moving again and notify the NetworkUIManager
        if (IsFinished())
        {
            nium.GameWon();
            movable = false;
        }
    }

    // check if all pieces of the puzzle are in their correct spot
    private bool IsFinished()
    {
        for (int i = 0; i < 9; i++)
        {
            if (gameBoard[i] != i) return false;
        }
        return true;
    }

    // returns the relation between two pieces of the puzzle
    private Directions IsAdjacent(int empty, int piece)
    {
        if (empty - 1 == piece && empty % 3 != 0) return Directions.Right;
        if (empty + 1 == piece && empty % 3 != 2) return Directions.Left;
        if (empty + 3 == piece) return Directions.Up;
        if (empty - 3 == piece) return Directions.Down;
        return Directions.Invalid;
    }

    // move a piece without any other logic
    public void MovePiece(int pieceNumber, Directions dir)
    {
        // get the correct piece
        Transform currentPiece = transform.Find("Piece" + pieceNumber);
        if (!currentPiece) return;
        currentPiece.gameObject.GetComponent<PuzzlePieceScript>().Move(dir, pieceOffset);
    }
}
