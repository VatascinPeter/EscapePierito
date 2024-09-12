using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// This file was created by Peter Vataščin
// 19/04/2024

// This script was used to test network communication and is not used in the final game, because of its inefficiency
// it has been reworked and a similar functionality is in the script "WatchBoxScript.cs"
public class ChangeMaterial : NetworkBehaviour
{
    [SerializeField]
    private Material mat1;

    [SerializeField]
    private Material mat2;

    // a local variable whether the first or the second material should be rendered
    private bool _first = true;

    // the same but as a network variable
    private NetworkVariable<bool> first = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // start with the first material 
    private void Awake()
    {
        GetComponent<Renderer>().material = mat1;
    }

    // change the material to the other one
    public void Change()
    {
        first.Value = !first.Value;
    }

    // Keeps asking for the value inside the network variable, if it changed from last time it asked, set the other material
    // This is inefficient and has been changed in the final game
    private void FixedUpdate()
    {
        if (_first != first.Value)
        {
            if (!first.Value) GetComponent<Renderer>().material = mat2;
            else GetComponent<Renderer>().material = mat1;
            _first = !_first;
        }
    }
}
