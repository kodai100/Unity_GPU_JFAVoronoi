using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotator : MonoBehaviour {

    public float speed = 10;

    float time;

	// Use this for initialization
	void Start () {
        time = 0;
	}
	
	// Update is called once per frame
	void Update () {
        time += Time.deltaTime * speed;
        transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, time));
	}
}
