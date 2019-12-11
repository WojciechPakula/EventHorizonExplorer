using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonEmitter : MonoBehaviour {
    static int fIndex = 0;
    public static void emitPhoton(Vector3 position, Vector3 direction)
    {
        direction.Normalize();
        GameObject f = (GameObject)Instantiate(Resources.Load("foton"));
        f.name = "foton" + fIndex++;
        var ph = f.GetComponent<Photon>();
        f.transform.position = position;
        ph.positionD = Vector3d.f_to_d(position);
        ph.momentumD = Vector3d.f_to_d(direction);
        RayTracingManager._transformsToWatch.Add(f.transform);
    }
}
