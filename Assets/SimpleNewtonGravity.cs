using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleNewtonGravity : MonoBehaviour
{
    public GameObject mass;
    public float M;
    public float G;

    //public Vector3 velocity;

    Rigidbody rb;

    //public int it = 0;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        rb.velocity = new Vector3(0,0,-7);
        //rb.solverIterations = 120;
    }

    // Update is called once per frame
    void Update()
    {
        float r = (transform.position - mass.transform.position).magnitude;
        Vector3 a = G * M / (r * r) * (mass.transform.position - transform.position).normalized;

        float dt = Time.deltaTime;
        float dt2 = Time.fixedDeltaTime;
        transform.position += rb.velocity * dt;
        rb.velocity += a * dt;
        
        if (rb.velocity.magnitude > 300)
        {
            rb.velocity = rb.velocity.normalized * 300;
        }

        //rb.solverIterations = it;
        //rb.solverVelocityIterations = it;
        //rb.sleepThreshold = 0.0000001f;
    }
}
