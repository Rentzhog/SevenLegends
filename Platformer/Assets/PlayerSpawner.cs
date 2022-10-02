using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    public List<GameObject> playerPrefabs = new List<GameObject>();

    private void Awake() {
        GetComponent<PlayerInputManager>().playerPrefab = playerPrefabs[0];
    }

    public void OnPlayerJoin() {
        playerPrefabs.RemoveAt(0);
        if(playerPrefabs.Count > 0) {
            GetComponent<PlayerInputManager>().playerPrefab = playerPrefabs[0];
        }

    }
}
