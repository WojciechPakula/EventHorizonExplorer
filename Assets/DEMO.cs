using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEMO : MonoBehaviour
{
    public GameObject dsc;
    public GameObject dscfe;
    public GameObject fe1;
    public GameObject fe2;
    public GameObject gl1;
    public GameObject gl2;
    public GameObject lb;
    public GameObject caps;
    public GameObject doub;
    public GameObject bh;

    // Start is called before the first frame update
    void Start()
    {
        //res();
        //bh.SetActive(true);
    }

    void res()
    {
        dsc.SetActive(false);
        dscfe.SetActive(false);
        fe1.SetActive(false);
        fe2.SetActive(false);
        gl1.SetActive(false);
        gl2.SetActive(false);
        lb.SetActive(false);
        caps.SetActive(false);
        doub.SetActive(false);
        bh.SetActive(false);
    }



    // Update is called once per frame
    void Update()
    {
        int k = -1;
        if (Input.GetKeyDown(KeyCode.Alpha1)) k = 1;
        if (Input.GetKeyDown(KeyCode.Alpha2)) k = 2;
        if (Input.GetKeyDown(KeyCode.Alpha3)) k = 3;
        if (Input.GetKeyDown(KeyCode.Alpha4)) k = 4;
        if (Input.GetKeyDown(KeyCode.Alpha5)) k = 5;
        if (Input.GetKeyDown(KeyCode.Alpha6)) k = 6;
        if (Input.GetKeyDown(KeyCode.Alpha7)) k = 7;
        if (Input.GetKeyDown(KeyCode.Alpha8)) k = 8;
        if (Input.GetKeyDown(KeyCode.Alpha9)) k = 9;
        if (Input.GetKeyDown(KeyCode.Alpha0)) k = 0;

        if (k > 0 && k < 9)
        {
            doub.SetActive(false);
            bh.SetActive(true);
        }

        switch (k) {
            case 0:
                res();
                bh.SetActive(true);
                break;
            case 1:
                dsc.active = !dsc.active;
                if (dsc.active && dscfe.active) dscfe.active = false;
                break;
            case 2:
                dscfe.active = !dscfe.active;
                if (dscfe.active && dsc.active) dsc.active = false;
                break;
            case 3:
                fe1.active = !fe1.active;
                
                break;
            case 4:
                fe2.active = !fe2.active;
                break;
            case 5:
                gl1.active = !gl1.active;
                break;
            case 6:
                gl2.active = !gl2.active;
                break;
            case 7:
                lb.active = !lb.active;
                break;
            case 8:
                caps.active = !caps.active;
                break;
            case 9:
                res();
                doub.SetActive(true);
                bh.SetActive(false);
                break;
        }

    }
}
