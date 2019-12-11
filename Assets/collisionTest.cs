using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collisionTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        //var tmp = other.GetComponent<Wall>();
        Debug.Log("Collision");
        /*if (tmp != null)
        {

        }*/
    }
}
