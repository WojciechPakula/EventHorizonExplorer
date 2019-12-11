using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BhButton : MonoBehaviour
{
    Button btn;

    public bool locked = false;
    public float r = 0;

    public Mass m = null;

    GameManagerGame gmg;

    // Start is called before the first frame update
    void Start()
    {
        btn = this.GetComponent<Button>();
        gmg = (GameManagerGame)GameManager.instance;


        btn.onClick.AddListener(click);
    }

    // Update is called once per frame
    void Update()
    {
        if (locked)
        {
            btn.interactable = false;
        }
        else
        {
            btn.interactable = true;
            m = null;
        }
    }

    void click()
    {
        if (locked)
        {
        } else
        {
            gmg.odznacz();
            GameObject f = (GameObject)Instantiate(Resources.Load("bh"));
            m = f.GetComponent<Mass>();
            m.promien = r;
            gmg.zaznacz(f);
            RayTracingManager._transformsToWatch.Add(f.transform);
            locked = true;
        }
    }
}
