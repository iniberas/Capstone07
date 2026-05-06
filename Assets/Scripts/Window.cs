using System;
using UnityEngine;

public class Window : MonoBehaviour
{
    private float xTimeFactor = .5f;
    private float yTimeFactor = .2f;
    private float xMoveFactor = .1f;
    private float yMoveFactor = .1f;

    private Vector3 startPos;

    void Start() {
        startPos = transform.position;
    }

    void Update() {
        float x = xMoveFactor * Mathf.Cos(Time.time * xTimeFactor);
        float y = yMoveFactor * Mathf.Sin(Time.time * yTimeFactor);

        transform.position = startPos + new Vector3(x, y, 0f);
    }
}