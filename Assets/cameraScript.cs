using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraScript : MonoBehaviour {

    public float ScrollSensitvity = 2;
    public float ScrollMultiplier = 1.2f;
    public float speedUp = 1;
    float cameraSpeed = 25.0f; //podstawowa prędkość ruchu kamery - 25, ewentualnie dać mniej, nie więcej
	float cameraSensitivity = 0.25f; //czułość kamery przy użyciu myszki
    float cameraSensitivityUp = 5f; //czułość kamery przy użyciu myszki
    private bool isRotating;
	private Vector3 lastMousePos = new Vector3(255,255,255);

    public Vector3 pivot;
    private Vector3 pivotMouse;

    public int mode = 1;

    public GameObject gun;

    float defoultUpDistance = 500;
    public float modeZoom = 1;

    

    void Start () {
        pivot = this.transform.position;
    }
	
	void Update () {
        //if (gun == null && mode==2)
        //    GameManager.instance.widokSwobodny();
        
        pivotMouse = Vector3.Lerp(pivotMouse, Input.mousePosition, Time.deltaTime * 10f);       
        if (Input.GetMouseButtonDown(1)) {
			lastMousePos = pivotMouse;
			isRotating = true;
		}
		
		if (!Input.GetMouseButton(1)) isRotating = false;

        if (mode == 1)
        {
            var po = this.transform.position;
            po.y = defoultUpDistance * modeZoom;
            pivot = po;
            this.transform.rotation = Quaternion.Euler(90, 0, 90);
        }
        if (mode == 2)
        {
            var po = this.transform.position;
            po.y = defoultUpDistance * modeZoom;

            pivot = gun.transform.position + Vector3.up*7;
        }

        //obsługa myszki
        if (isRotating) {
			//lastMousePos = new Vector3(transform.eulerAngles.x + lastMousePos.x, transform.eulerAngles.y + lastMousePos.y, 0);
            //var tmp = Mathf.Clamp(transform.eulerAngles.x + lastMousePos.x, -89, 89);
            if (mode == 0)
            {
                lastMousePos = pivotMouse - lastMousePos;
                //Debug.Log(lastMousePos);
                lastMousePos = new Vector3(-lastMousePos.y * cameraSensitivity, lastMousePos.x * cameraSensitivity, 0);
                var tmp = (transform.eulerAngles.x + lastMousePos.x);
                float limiter = 0.01f;
                if (tmp >= 90 - limiter && tmp < 180)
                {
                    tmp = 90 - limiter;
                }
                else if (tmp < 270 + limiter && tmp >= 180)
                {
                    tmp = 270 + limiter;
                }
                lastMousePos = new Vector3(tmp, transform.eulerAngles.y + lastMousePos.y, 0);
                transform.eulerAngles = lastMousePos;
            }
            if (mode == 1)
            {
                lastMousePos = pivotMouse - lastMousePos;
                //Debug.Log(lastMousePos);
                lastMousePos = new Vector3(-lastMousePos.y * cameraSensitivityUp, 0, lastMousePos.x * cameraSensitivityUp);

                var po = this.transform.position;
                po.y = defoultUpDistance*modeZoom;
                po -= lastMousePos*modeZoom;
                pivot = po;
            }
            if (mode == 2)
            {
                lastMousePos = pivotMouse - lastMousePos;
                //Debug.Log(lastMousePos);
                lastMousePos = new Vector3(-lastMousePos.y * cameraSensitivity, lastMousePos.x * cameraSensitivity, 0);
                var tmp = (transform.eulerAngles.x + lastMousePos.x);
                float limiter = 0.01f;
                if (tmp >= 90 - limiter && tmp < 180)
                {
                    tmp = 90 - limiter;
                }
                else if (tmp < 270 + limiter && tmp >= 180)
                {
                    tmp = 270 + limiter;
                }
                lastMousePos = new Vector3(0, transform.eulerAngles.y + lastMousePos.y, 0);
                transform.eulerAngles = lastMousePos;
                gun.transform.rotation = this.transform.rotation;
            }
            lastMousePos = pivotMouse;
        }


		//obsługa klawiatury
		Vector3 p = GetBaseInput();
        
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            if (mode == 0)
            {
                float ScrollAmount = Input.GetAxis("Mouse ScrollWheel") * ScrollSensitvity;
                if (ScrollAmount > 0)
                {
                    //mnożenie
                    speedUp *= ScrollMultiplier;
                }
                else
                {
                    //dzielenie
                    speedUp /= ScrollMultiplier;
                }
            }
            if (mode == 1)
            {
                float ScrollAmount = Input.GetAxis("Mouse ScrollWheel") * ScrollSensitvity;
                if (ScrollAmount > 0)
                {
                    //mnożenie
                    modeZoom *= ScrollMultiplier;
                }
                else
                {
                    //dzielenie
                    modeZoom /= ScrollMultiplier;
                }
            }

            
        }
        p = p * cameraSpeed * speedUp;
		p = p * Time.deltaTime;
        p = this.transform.rotation * p;
        pivot += p;
        this.transform.position = Vector3.Lerp(this.transform.position, pivot, Time.deltaTime*10f);
    }

    private Vector3 GetBaseInput() {
		Vector3 p_Velocity = new Vector3();
		if (Input.GetKey (KeyCode.W)) {
			p_Velocity += new Vector3(0, 0, 1);
		}
		if (Input.GetKey (KeyCode.S)) {
			p_Velocity += new Vector3(0, 0, -1);
		}
		if (Input.GetKey (KeyCode.A)) {
			p_Velocity += new Vector3(-1, 0, 0);
		}
		if (Input.GetKey (KeyCode.D)) {
			p_Velocity += new Vector3(1, 0, 0);
		}
        if (Input.GetKey(KeyCode.R))
        {
            p_Velocity += new Vector3(0, 1, 0);
        }
        if (Input.GetKey(KeyCode.F))
        {
            p_Velocity += new Vector3(0, -1, 0);
        }
        return p_Velocity;
	}
}
