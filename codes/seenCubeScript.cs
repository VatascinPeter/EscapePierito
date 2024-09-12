using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This file was created by Peter Vataščin
// 19/04/2024

// Prototype of the first scenarion, not used in the final game, final verion is in "WatchBoxScript.cs"
public class seenCubeScript : MonoBehaviour
{
    private NetworkUIManager nium;
    private bool localActivity;

    private void Awake()
    {
        nium = FindFirstObjectByType<NetworkUIManager>();

        // finds out if the WatchBox image is currently tracked by the client
        localActivity = nium.IsSeen();
    }

    private void FixedUpdate()
    {
        // gets the value from NetworkUIManager continually - inefficient - better implementation in WatchBox.cs
        bool activity = nium.IsSeen();

        // if the value has changed, set active a different child of the game object
        if (localActivity != activity)
        {
            localActivity = activity;
            this.transform.GetChild(0).gameObject.SetActive(activity);
            this.transform.GetChild(1).gameObject.SetActive(!activity);
        }
    }
}
