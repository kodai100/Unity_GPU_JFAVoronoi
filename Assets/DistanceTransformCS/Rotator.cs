using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {

    public float speed = 1f;
    private float time = 0;

	// Use this for initialization
	void Start () {
        time = 0;
	}
	
	// Update is called once per frame
	void Update () {

        time += Time.deltaTime;

        transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, speed * time));
	}
}
