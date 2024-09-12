using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode.Transports.UTP;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// This file was created by Peter Vataščin
// 19/04/2024

// Script taking care of the network communication between the two players and taking care of the UI elements
public class NetworkUIManager : NetworkBehaviour
{
    // UI Declarations --------------------------------------------------
    [SerializeField]
    private Button HostButton;
    [SerializeField]
    private Button ClientButton;
    [SerializeField]
    private TextMeshProUGUI clientIDLabel;
    [SerializeField]
    private Image MainMenu;
    [SerializeField]
    private TMP_InputField IPInput;
    [SerializeField]
    private Image EndScreen;
    [SerializeField]
    private TextMeshProUGUI endText1;
    [SerializeField]
    private TextMeshProUGUI endText2;
    [SerializeField]
    private TextMeshProUGUI IPAddressText;

    // Logic Declarations -----------------------------------------------
    [SerializeField]
    private ARSessionOrigin aRSessionOrigin;

    // stores whether the WatchBox image is being tracked by the client
    private bool seen = false;

    // stores whether the owner of the script is a host or client
    private bool isHost = false;

    // stores whether the main menu is inactive 
    private bool connected = false;

    // stores whether the second scenario has been finished
    private bool puzzle3 = false;

    // Tasks performed on Awake -----------------------------------------
    private void Awake()
    {
        // BEGINNING OF A CODE SNIPPET FOR FINDING A LOCAL IP ADDRESS
        // a slightly modified version of a code posted by the user "Pr0n"
        // https://discussions.unity.com/t/local-ip-adress-in-unet/143531
        IPHostEntry iphe = Dns.GetHostEntry(Dns.GetHostName());
        string localIP = "";
        foreach (IPAddress ip in iphe.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break;
            }
        }
        // ENDING OF A CODE SNIPPET FOR FINDING A LOCAL IP ADDRESS

        IPAddressText.text = localIP;

        // when the host button is clicked, the game starts as a host
        HostButton.onClick.AddListener(() =>
        {
            if (!string.IsNullOrWhiteSpace(IPInput.text)) NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(IPInput.text, (ushort)7778);
            aRSessionOrigin.GetComponent<ImageRecognitionScript>().SetIsHost(true);
            NetworkManager.Singleton.StartHost();
            isHost = true;
            changeUI();
        });

        // when the client button is clicked the game connects to the IP address given as a client
        ClientButton.onClick.AddListener(() =>
        {
            if (!string.IsNullOrWhiteSpace(IPInput.text)) NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(IPInput.text, (ushort)7778);
            aRSessionOrigin.GetComponent<ImageRecognitionScript>().SetIsHost(false);
            NetworkManager.Singleton.StartClient();
            isHost = false;
            changeUI();
        });
    }

    // UI Methods -------------------------------------------------------
        // activates and deactivates the main menu
    private void changeUI()
    {
        HostButton.gameObject.SetActive(connected);
        ClientButton.gameObject.SetActive(connected);
        MainMenu.gameObject.SetActive(connected);
        IPInput.gameObject.SetActive(connected);

        connected = !connected;
    }

    // fades in the end screen
    private void EndScreenFadeIn()
    {
        EndScreen.gameObject.SetActive(true);
        EndScreen.color += new Color(0f, 0f, 0f, 0.1f);
        EndScreen.CrossFadeAlpha(255f, 5f, true);
        endText1.color += new Color(0f, 0f, 0f, 0.1f);
        endText1.CrossFadeAlpha(255f, 5f, true);
        endText2.color += new Color(0f, 0f, 0f, 0.1f);
        endText2.CrossFadeAlpha(255f, 5f, true);
        NetworkManager.Singleton.Shutdown();
    }

    // Task synchronisation calls ---------------------------------------
        // getter for seen variable
    public bool IsSeen()
    {
        return seen;
    }

    // getter for puzzle3 variable
    public bool IsPuzzle3()
    {
        return puzzle3;
    }

    // getter for isHost variable
    public bool Host()
    {
        return isHost;
    }

    // a method called when the WatchBox becomes tracked or has been lost
    public void SeenChanger(bool isSeen)
    {
        // always called on the client side

        // updates local variable
        seen = isSeen;
        // finds the first object with the WatchBoxScript component and calls a method saying which material is to be used
        WatchBoxScript wbox = FindObjectOfType<WatchBoxScript>();
        if (wbox) wbox.ChangeSeen(isSeen);

        // calls for the same to be done on the host side
        ChangeSeenServerRpc(isSeen);
    }

    // called when a button is clicked, passing it the ID
    public void ButtonClick(int buttonNumber)
    {
        //always called on the client side
        ButtonClickServerRpc(buttonNumber);
    }

    // called to notify the game that the second scenario is finished (the correct code was entered to the wheel box)
    public void Task2Solved()
    {
        // always called on the host side
        Task2SolvedClientRPC();
        puzzle3 = true;
        
        // finds the puzzle box in the scene and opens it
        PuzzleBoxScript puzzleBox = FindObjectOfType<PuzzleBoxScript>();
        if (!puzzleBox) return;

        puzzleBox.OpenBox();
    }

    // called to notify the client that a piece in the puzzle box has been moved
    public void PieceMoved(int id, PuzzleBoxScript.Directions dir)
    {
        // always called on the host side
        PieceMovedClientRPC(id, dir);
    }

    // called to notify that the game is finished
    public void GameWon()
    {
        // always called on the host side (if not uncomment the bottom 2 lines)

        GameWonClientRPC();
        // EndScreenFadeIn();
        // GameWonServerRPC();
    }

    // Server RPCs ------------------------------------------------------

      // finds the wheel box in the game scene and calls a method to rotate the correct letter wheel
    [ServerRpc(RequireOwnership = false)]
    private void ButtonClickServerRpc(int buttonNumber)
    {
        WheelBox wbox = FindObjectOfType<WheelBox>();
        if (!wbox) return;

        wbox.RotateWheel(buttonNumber);
    }

    // calls a method of an object with the WatchBoxScript component that changes the material to the correct one
    [ServerRpc(RequireOwnership = false)]
    private void ChangeSeenServerRpc(bool isSeen)
    {
        // TESTING
        // if (isSeen) clientIDLabel.text = "Seen";
        // else clientIDLabel.text = "Unseen";
        // TESTING
        seen = isSeen;
        WatchBoxScript wbox = FindObjectOfType<WatchBoxScript>();
        if (!wbox) return;

        wbox.ChangeSeen(isSeen);
    }

    // called when the game is finished, calls a method to show the end screen
    [ServerRpc(RequireOwnership = false)]
    private void GameWonServerRPC()
    {
        EndScreenFadeIn();
    }

    // Client RPCs ------------------------------------------------------

    // notifies the client the second scenario is finished
    [ClientRpc]
    private void Task2SolvedClientRPC()
    {
        // a host is also a client so if the owner is a host, return
        if (isHost) return;
        puzzle3 = true;
        PuzzleBoxScript puzzleBox = FindObjectOfType<PuzzleBoxScript>();
        if (!puzzleBox) return;

        // make the puzzle box pieces show the scrambled image and make the box cover transparent
        puzzleBox.MakeWindow();
        puzzleBox.FillPieces();
    }

    // notifies the client a sliding puzzle piece has been moved, calls an appropriate method of the PuzzleBoxScript
    [ClientRpc]
    private void PieceMovedClientRPC(int id, PuzzleBoxScript.Directions dir)
    {
        if (isHost) return;

        PuzzleBoxScript puzzleBox = FindObjectOfType<PuzzleBoxScript>();
        if (!puzzleBox) return;

        puzzleBox.MovePiece(id, dir);
    }

    // notifies both sides the game is finished
    [ClientRpc]
    private void GameWonClientRPC()
    {
        // no if because it is meant to be performed on both sides
        EndScreenFadeIn();
    }
}
