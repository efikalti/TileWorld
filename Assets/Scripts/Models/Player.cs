using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour {
    // RigidBody component
    Rigidbody rigidBody;

    // Agent speed
    private float Speed = 1.5f;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }
    
    public void Move(Vector3 target)
    {
        if (rigidBody == null) return;
        if (target.x >= transform.position.x + 0.1)
        {
            rigidBody.velocity = new Vector3(1, 0, 0) * Speed;
        }
        else if (target.x <= transform.position.x - 0.1)
        {
            rigidBody.velocity = new Vector3(-1, 0, 0) * Speed;
        }
        else if (target.z <= transform.position.z + 0.1)
        {
            rigidBody.velocity = new Vector3(0, 0, -1) * Speed;
        }
        else if (target.z >= transform.position.z - 0.1)
        {
            rigidBody.velocity = new Vector3(0, 0, 1) * Speed;
        }
    }

    public void Stop()
    {
        if (rigidBody == null) return;
        rigidBody.velocity = Vector3.zero;
    }
}
