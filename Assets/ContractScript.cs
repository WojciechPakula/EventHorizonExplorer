using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContractScript : MonoBehaviour
{
    public float time;

    Vector3 s;

    float totaltime;

    // Start is called before the first frame update
    void Start()
    {
        s = this.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        totaltime += Time.deltaTime;
        float wsp = (time - totaltime) / time;
        wsp = Mathf.Clamp(wsp, 0, 1);
        this.transform.localScale = s*wsp;
    }
}
