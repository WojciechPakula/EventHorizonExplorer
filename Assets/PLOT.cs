using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PLOT : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void clearPoints()
    {
        var chc = transform.childCount;
        for (int i = 0; i < chc; ++i)
        {
            var child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    public void putPoint(float x, float y)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale *= 2;
        cube.transform.parent = this.transform;

        cube.transform.position = new Vector3(x, 0, y+300);
    }
}
