using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    public GameObject ptr;
    float PI = 3.14159265f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var tmp = this.transform.forward;
        //ptr.transform.position = this.transform.position + this.transform.forward*10;
        var inter = planeIntersection(Vector3.zero,new Vector3(0,1,0),this.transform.position, this.transform.forward);
        if (inter.magnitude < 1000)
            ptr.transform.position = inter;
    }

    float length(Vector3 v)
    {
        return v.magnitude;
    }
    Vector3 normalize(Vector3 v)
    {
        return v.normalized;
    }
    Vector3 projectOnPlane(Vector3 v, Vector3 planeNormal)
    {
        return v - planeNormal * Vector3.Dot(v, planeNormal);
    }
    float angle(Vector3 a, Vector3 b)
    {
        return Mathf.Acos(Mathf.Clamp(Vector3.Dot(normalize(a), normalize(b)), -1.0f, 1.0f));
    }

    Vector3 planeIntersection(Vector3 pointt, Vector3 normal, Vector3 origin, Vector3 direction)
    {
        direction = normalize(direction);
        normal = normalize(normal);
        Vector3 normal2 = -1.0f * normal;

        Vector3 tmp = normal; //wybrac lepszy normal

        float kat = angle(direction, normal);
        if (kat <= PI / 2.0f)
            tmp = normal;
        else
            tmp = normal2;

        float alpha = PI / 2.0f - angle(origin - pointt, tmp);
        float beta = PI / 2.0f - angle(direction, tmp);

        float d = length(pointt - origin);
        float x = -d * Mathf.Sin(alpha) / Mathf.Sin(beta);

        return origin + direction * x;
    }
}
