using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour {

    Rigidbody rBody = null;
    bool stop;
    float speed;

	// Use this for initialization
	void Start () {
        rBody = GetComponent<Rigidbody>();
        stop = false;
        speed = 1;
	}
	
	// Update is called once per frame
	void Update () {
        if (stop)
        {
            speed = Mathf.Lerp(speed, 0, Time.deltaTime);
            rBody.velocity = rBody.velocity * speed;
            if (speed < 0.5)
            {
                stop = false;
                speed = 1;
            }
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>())
        {
            rBody.velocity = other.transform.GetComponent<Rigidbody>().velocity;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        stop = true;
    }
}
