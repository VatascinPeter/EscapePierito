using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This file was created by Peter Vataščin
// 19/04/2024

// The final version of the script that changes the material of the object based on whether the WatchBox image is being tracked by the client
public class WatchBoxScript : MonoBehaviour
{
    private NetworkUIManager nium;

    // material of this object when the image is being tracked
    [SerializeField]
    private Material seenMaterial;

    // material of this object when the image is not being tracked
    [SerializeField]
    private Material unseenMaterial;

    private void Awake()
    {
        nium = FindFirstObjectByType<NetworkUIManager>();
        // gets the current value ofthe image tracking
        ChangeSeen(nium.IsSeen());
    }

    // changes a material based on passed variable
    public void ChangeSeen(bool newSeen)
    {
        if (newSeen) transform.GetComponent<Renderer>().material = seenMaterial;
        else transform.GetComponent<Renderer>().material = unseenMaterial;
    }
}
