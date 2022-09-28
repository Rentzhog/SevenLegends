using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMover : MonoBehaviour
{

    public float speed;

    // Update is called once per frame
    void Update()
    {
        foreach(Transform child in transform.GetComponentsInChildren<Transform>()) {
            child.Translate(Vector2.right * Time.deltaTime * speed * Random.Range(0.8f, 1f));
        }
    }
}
