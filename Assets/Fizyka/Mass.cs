using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mass : MonoBehaviour {
    public float mass;
    public float _promien;

    public float rs { get; private set; }
    public float rf { get; private set; }

    public float promien
    {
        get { return _promien; }
        set
        {
            _promien = value;
            rs = _promien;
            rf = _promien * 3.0f / 2.0f;
            mass = rs/(2.0f*(float)PhotonPhysics.G)*((float)(PhotonPhysics.c * PhotonPhysics.c));
        }
    }

    // Use this for initialization
    void Awake () {
        mass = rs / (2.0f * (float)PhotonPhysics.G) * ((float)(PhotonPhysics.c * PhotonPhysics.c));//return;
    }

	// Update is called once per frame
	void Update () {
        promien = _promien;
        this.transform.localScale = new Vector3(promien*2, promien*2, promien*2);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float radius = (float)(rf);
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color = Color.red;
        radius = (float)(rs);        
        //Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.DrawWireSphere(transform.position, radius);
        /*Gizmos.color = Color.yellow;
        radius = (float)((1.0 * Photon.G * mass) / (Photon.c * Photon.c));
        Gizmos.DrawWireSphere(transform.position, radius);
        promien = radius;
        radius *= 2.0f/3.0f;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);*/
    }
}
