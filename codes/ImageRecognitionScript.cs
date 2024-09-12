using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;

// This file was created by Peter Vataščin, building on implementation from https://www.youtube.com/watch?v=gpaq5bAjya8&t=1356s
// 19/04/2024

// This script takes care of instantiating the correct objects when an image is recognised
[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageRecognitionScript : MonoBehaviour
{
    private ARTrackedImageManager manager;

    // array of game objects to be instantiated in the host's game
    [SerializeField]
    private GameObject[] hostPrefabs;
    // array of game objects to be instantiated in the client's game
    [SerializeField]
    private GameObject[] clientPrefabs;

    // dictionary of names and game objects that have been instantiated
    private readonly Dictionary<string, GameObject> placed = new Dictionary<string, GameObject>();
    [SerializeField]
    private NetworkUIManager nuim;

    // variable used for the first scenario so this script does not keep calling the NetworkUIManager every frame
    private bool watchBoxSeen = false;

    // a variable stating which array of prefabs is to be used
    private bool isHost = false;

    // setter for isHost
    public void SetIsHost(bool b)
    {
        isHost = b;
    }

    private void Awake()
    {
        manager = GetComponent<ARTrackedImageManager>();
    }

    // subscribes and unsubscribes to the trackedImagesChangedEvent
    // This means that when the event occurs, the method ImagesChanged is called
    private void OnEnable()
    {
        manager.trackedImagesChanged += ImagesChanged;
    }

    private void OnDisable()
    {
        manager.trackedImagesChanged -= ImagesChanged;
    }

    // A method taking in ARTrackedImagesChangedEventArgs argument which contains three arrays of ARTrackedImage
    private void ImagesChanged(ARTrackedImagesChangedEventArgs obj)
    {
        // loops through the ARTrackedImages that are newly found
        foreach (ARTrackedImage image in obj.added)
        {
            // saves the name of the given ARTrackedImage - it is the same as the name of the prefab that is to be instantiated on the image
            string name = image.referenceImage.name;

            // decides where to find the corresponding game object to the tracked image
            if (isHost)
            {
                // loops through the prefabs and finds the one with the same name as the image
                foreach (GameObject prefab in hostPrefabs)
                {
                    if (string.Compare(prefab.name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        // instantiate the game object in the same position as the image, add the object to the placed dictionary
                        GameObject newObject = Instantiate(prefab, image.transform);
                        placed[name] = newObject;
                        //break;
                    }
                }
            } else
            {
                // loops through the prefabs and finds the one with the same name as the image
                foreach (GameObject prefab in clientPrefabs)
                {
                    if (string.Compare(prefab.name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        // if the "WatchBox" image is not being tracked and is newly found, notify NetworkUIManager and remember it is being tracked
                        if (!watchBoxSeen && string.Compare(name, "WatchBox", StringComparison.OrdinalIgnoreCase) == 0) {
                            nuim.SeenChanger(true);
                            watchBoxSeen = true;
                        }

                        // instantiate the game object in the same position as the image, add the object to the placed dictionary
                        GameObject newObject = Instantiate(prefab, image.transform);
                        placed[name] = newObject;
                        //break;
                    }
                }
            }
        }

        // loop through the images which status has been updated
        // this could mean the image is just lost, found again after being lost for a bit
        foreach (ARTrackedImage image in obj.updated)
        {
            // update the status of the WatchBox image being tracked
            if (!isHost && string.Compare(image.referenceImage.name, "WatchBox", StringComparison.OrdinalIgnoreCase) == 0)
            {
                bool isSeen = image.trackingState == TrackingState.Tracking;
                // don't notify the NetworkUIManager if the new state is the same as the previous one
                if (isSeen != watchBoxSeen) nuim.SeenChanger(isSeen);
                watchBoxSeen = isSeen;
            }
        }

        // loop through the images that are lost in the scene and removed from the list
        foreach (ARTrackedImage image in obj.removed)
        {
            // if the WatchBox image is lost notify the NetworkUIManager, unless it wasn't being tracked before
            if (!isHost && string.Compare(image.referenceImage.name, "WatchBox", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (watchBoxSeen) nuim.SeenChanger(false);
                watchBoxSeen = false;
            }
        }
    }
}
