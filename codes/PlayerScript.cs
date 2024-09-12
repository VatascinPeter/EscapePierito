using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// This file was created by Peter Vataščin, building on implementation from https://www.youtube.com/watch?v=GfS72wqKQ_g&t=13s
// 20/5/2024

// This script was used to test the network communication and is not used in the final game
// It lets the players controle each their own player object prefab and spawns a cube which changes material and the change is synchronised between players
public class PlayerScript : NetworkBehaviour
{
    // prefab of a network cube spawned by the host
    [SerializeField]
    private Transform CubePrefab;
    private Transform spawnedCube;
    
    // the movement speed of the player object
    [SerializeField]
    private float speed;
    private FixedJoystick joystick;
    private Rigidbody body;

    // the two materials for the player prefab
    [SerializeField]
    private Material original;
    [SerializeField]
    private Material updated;

    // local image of the isOriginal value
    private bool _value = true;

    // stores, which material this instance of the player object should show
    private NetworkVariable<bool> isOriginal = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void OnEnable()
    {
        joystick = FindObjectOfType<FixedJoystick>();
        body = gameObject.GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsHost) return;

        // when the player is spawned in the scene, if the owner is a host, instantiate a cube, which material is being synchronised between the client and the host
        spawnedCube = Instantiate(CubePrefab);
        spawnedCube.GetComponent<NetworkObject>().Spawn(true);
    }

    private void FixedUpdate()
    {
        // if the network variable of this object changed, change the material of the player object on all sides
        if (isOriginal.Value != _value) { 
            if (isOriginal.Value) GetComponent<Renderer>().material = original;
            else GetComponent<Renderer>().material = updated;
            _value = !_value;
        }

        // change the material of the spawned cube by pressing a key, this is done for easier testing on the computer
        if (Input.GetKeyDown(KeyCode.O) && spawnedCube != null) spawnedCube.GetComponent<ChangeMaterial>().Change();

        // control of the player only by the owner
        if (!IsOwner) return;
        // move the player object by the joystick, don't move vertically
        body.velocity = new Vector3(joystick.Horizontal, 0, joystick.Vertical) * speed;
        
        // change a material of the player object
        if (Input.GetKeyDown(KeyCode.T)) isOriginal.Value = !isOriginal.Value;
        
    }
}
