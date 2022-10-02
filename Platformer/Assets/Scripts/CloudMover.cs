using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMover : MonoBehaviour
{

    public GameObject[] cloudPrefabs;

    public Color cloudTint;

    public int clouds;
    public float speed;

    private void Start() {

    }

    // Update is called once per frame
    void Update()
    {
        foreach(Transform child in transform.GetComponentsInChildren<Transform>()) {
            child.Translate(Vector2.right * Time.deltaTime * speed * Random.Range(0.8f, 1f));
        }
    }

    private void OnValidate() {
        foreach (SpriteRenderer child in transform.GetComponentsInChildren<SpriteRenderer>()) {
            child.color = cloudTint;
        }
    }
}
