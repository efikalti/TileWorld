using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Box>())
        {
            Destroy(this.gameObject);
            Destroy(other.gameObject);
        }
    }
}
