﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerEdit : GameManager
{
    public GameObject menuEdit;
    public GameObject menuEdit2;

    public Button usunButton;

    public GameObject menuWall;
    public Slider dlugosc;
    public Slider grubosc;

    public GameObject menuBh;
    public Slider promien;

    public GameObject menuTar;
    public Slider rozmiar;

    public Text stats;
    public InputField nazwaWczytania;

    // Start is called before the first frame update
    void Start()
    {
        hits = 0;
        shoots = 0;
        instance = this;
        ptrMeshRenderer = pointer.GetComponent<MeshRenderer>();
    }

    public void quit()
    {
        SceneManager.LoadScene("Menu0");
    }

    // Update is called once per frame
    void Update()
    {
        stats.text = "trafienia/strzały: " + hits + "/" + shoots;
       
        if (cam.mode == 1)
        {
            menuEdit.SetActive(true);
            menuEdit2.SetActive(true);
        }
        else
        {
            menuEdit.SetActive(false);
            menuEdit2.SetActive(false);
        }

        var walls = FindObjectsOfType<Wall>();
        var targets = FindObjectsOfType<Target>();
        var blackHoles = FindObjectsOfType<Mass>();
        var guns = FindObjectsOfType<Gun>();

        minDistance = float.PositiveInfinity;
        nearest = null;
        foreach (var ele in blackHoles)
        {
            processElement(ele.gameObject);
        }
        foreach (var ele in walls)
        {
            processElement(ele.gameObject);
        }
        foreach (var ele in targets)
        {
            processElement(ele.gameObject);
        }

        foreach (var ele in guns)
        {
            processElement(ele.gameObject);
        }


        if (selected != null)
        {
            usunButton.interactable = true;
            selected.transform.position = new Vector3(cam.transform.position.x, 0, cam.transform.position.z);
            if (selected.GetComponent<Mass>() != null)
            {
                var m = selected.GetComponent<Mass>();
                m.promien = promien.value;
            }
            if (selected.GetComponent<Wall>() != null)
            {
                var s = new Vector3(grubosc.value, selected.transform.localScale.y, dlugosc.value);
                selected.transform.localScale = s;
            }

            if (selected.GetComponent<Target>() != null)
            {
                var m = selected.GetComponent<Target>();
                m.transform.localScale = new Vector3(rozmiar.value, 0.23f, rozmiar.value);
            }
            if (selected.GetComponent<Gun>() != null)
            {
                usunButton.interactable = false;
            }
        }

        if (popUpLife > 0)
        {
            popUpLife--;
            popOpacity = 1;
        }
        else
        {
            popOpacity = 0;
        }

        if (textBuffer != popText.text)
        {
            popUpLife = popTime;
            popOpacityPivot = Mathf.Lerp(popOpacityPivot, 0, Time.deltaTime * 5);
            if (popOpacityPivot < 0.05f) popText.text = textBuffer;
        }
        else
        {
            popOpacityPivot = Mathf.Lerp(popOpacityPivot, popOpacity, Time.deltaTime * 5);
        }

        Color zm = popText.color;
        zm.a = popOpacityPivot;
        popText.color = zm;

        if (cam.mode == 1)
        {
            ptrMeshRenderer.enabled = true;
        }
        else
        {
            ptrMeshRenderer.enabled = false;
        }

        if (cam.mode == 1 || cam.mode == 2)
        {
            kursor.SetActive(true);
        }
        else
        {
            kursor.SetActive(false);
        }
    }

    public void zaznacz()
    {
        shoots = 0;
        hits = 0;
        odznacz();
        selected = nearest;
        if (selected != null)
        {
            cam.transform.position = selected.transform.position;
            if (selected.GetComponent<Mass>() != null)
            {
                var m = selected.GetComponent<Mass>();
                menuBh.SetActive(true);
                promien.value = m.rs;
            }
            if (selected.GetComponent<Wall>() != null)
            {
                menuWall.SetActive(true);
                grubosc.value = selected.transform.localScale.x;
                dlugosc.value = selected.transform.localScale.z;
            }

            if (selected.GetComponent<Target>() != null)
            {
                var m = selected.GetComponent<Target>();
                menuTar.SetActive(true);

                rozmiar.value = m.transform.localScale.x;
            }
        }
    }
    public void zaznacz(GameObject go)
    {
        shoots = 0;
        hits = 0;
        odznacz();
        selected = go;
        if (selected != null)
        {
            //cam.transform.position = selected.transform.position;
            if (selected.GetComponent<Mass>() != null)
            {
                var m = selected.GetComponent<Mass>();
                menuBh.SetActive(true);
                promien.value = m.rs;
            }
            if (selected.GetComponent<Wall>() != null)
            {
                menuWall.SetActive(true);
                grubosc.value = selected.transform.localScale.x;
                dlugosc.value = selected.transform.localScale.z;
            }

            if (selected.GetComponent<Target>() != null)
            {
                var m = selected.GetComponent<Target>();
                menuTar.SetActive(true);

                rozmiar.value = m.transform.localScale.x;
            }

        }
    }

    public void odznacz()
    {
        selected = null;
        if (menuWall != null) menuWall.SetActive(false);
        if (menuBh != null) menuBh.SetActive(false);
        if (menuTar != null) menuTar.SetActive(false);
    }

    public void obrocL()
    {
        if (selected != null)
        {
            selected.transform.Rotate(new Vector3(0, -22.5f, 0), Space.World);
        }
    }
    public void obrocP()
    {
        if (selected != null)
        {
            selected.transform.Rotate(new Vector3(0, 22.5f, 0), Space.World);
        }
    }
    public void Usun()
    {
        if (selected != null)
        {
            Destroy(selected.gameObject);
            odznacz();
        }
    }

    float minDistance = float.PositiveInfinity;
    GameObject nearest = null;
    GameObject selected = null;
    void processElement(GameObject go)
    {
        var mag = (go.transform.position - pointer.transform.position).magnitude;
        if (minDistance > mag)
        {
            minDistance = mag;
            nearest = go;
        }
    }


    public void putBH()
    {
        odznacz();
        GameObject f = (GameObject)Instantiate(Resources.Load("bh"));
        zaznacz(f);
        RayTracingManager._transformsToWatch.Add(f.transform);
    }
    public void putTar()
    {
        odznacz();
        GameObject f = (GameObject)Instantiate(Resources.Load("Target"));
        zaznacz(f);
        RayTracingManager._transformsToWatch.Add(f.transform);
    }
    public void putWall()
    {
        odznacz();
        GameObject f = (GameObject)Instantiate(Resources.Load("wall"));
        zaznacz(f);
        RayTracingManager._transformsToWatch.Add(f.transform);
    }

    

    /*public void parserTest()
    {
        //saveWorld();
        loadWorld("SavedMaps/2019-9-19-0-3-22.json");
    }*/

    public void saveWorld()
    {
        if (hits > 0)
        {
            var js = Parser.WorldToString();
            DateTime t = DateTime.Now;
            string FileName = t.Year + "-" + t.Month + "-" + t.Day + "-" + t.Hour + "-" + t.Minute + "-" + t.Second;
            System.IO.Directory.CreateDirectory("SavedMaps");
            string path = "SavedMaps/" + FileName + ".json";
            Parser.stringToFile(js, path);
            lastSaved = path;
            popUp(("Zapisano mapę: " + path));
            nazwaWczytania.text = FileName;
        }
        else
        {
            popUp("Musisz udowodnić, że mapę da się przejść. \nAby zapisać mapę musisz przynajmniej raz trafić w tarczę.");
        }


    }

    string lastSaved = "";

    public void load()
    {
        /* var js = Parser.WorldToFile();
         Parser.stringToFile(js, "JsonUtility_test.json");*/
        //saveWorld();
        /*if (lastSaved == "")
            loadWorld("SavedMaps/2019-9-19-0-3-22.json");
        else
            loadWorld(lastSaved);*/
        string p = "SavedMaps/" + nazwaWczytania.text;
        if (!p.Contains(".json"))
        {
            p += ".json";
        }
        loadWorld(p);
    }

    public void loadWorld(string path)
    {
        try
        {
            var js = Parser.stringFromFile(path);
            Parser.WorldFromString(js);
            popUp(("Wczytano mapę: " + path));
        }
        catch
        {
            popUp(("Błąd podczas wczytywania: " + path));
        }
    }

    public RayTracingManager RTM;
    public cameraScript cam;
    public GameObject pointer;

    public Text popText;

    public GameObject kursor;
    // Start is called before the first frame update

    // Update is called once per frame
    MeshRenderer ptrMeshRenderer;
    



    public void toogleFx()
    {
        RTM.fx = !RTM.fx;

        string t = "";

        int r = UnityEngine.Random.Range(0, 6);

        t += "Ciekawostka:";
        t += "\n";
        switch (r)
        {
            case 0:
                t += "Cień czarnej dziury jest 2.6 razy większy od jej horyzontu zdarzeń";
                t += "\n";
                break;
            case 1:
                t += "Obraz krawędzi czarnej dziury jest obszarem w którym światło może ją orbitować";
                t += "\n";
                break;
            case 2:
                t += "Obserwator mógłby zobaczyć samego siebie, gdyby obserwował krawędź czarnej dziury przez teleskop";
                t += "\n";
                break;
            case 3:
                t += "Obraz gwiazdy znajdującej się dokładnie za czarną dziurą tworzy pierścień dookoła niej (pierścień Einsteina).";
                t += "\n";
                break;
            case 4:
                t += "Wewnątrz czarnej dziury, obraz świata zewnętrznego, przypomina zmniejszającą się kulę.";
                t += "\n";
                break;
            case 5:
                t += "Dylatacja czasu w pobliżu horyzontu zdarzeń jest tak duża, że zewnętrzny obserwator nigdy nie zarejestruje obiektu wpadającego do środka.";
                t += "\n";
                break;
        }
        popUp(t);
    }

    public void camUP()
    {
        RTM.fx = !RTM.fx;
    }


    public void widokSwobodny()
    {
        cam.mode = 0;
        selected = null;

        string t = "";
        t += "PPM - obrót kamery";
        t += "\n";
        t += "WASD RF - przód, lewo, tył, prawo, góra, dół";
        t += "\n";
        t += "Scroll - prędkość";
        t += "\n";
        t += "Spacja - strzał";
        popUp(t);
    }

    public void widokZGory()
    {
        cam.mode = 1;
        selected = null;
        string t = "";
        t += "PPM - ruch kamery";
        t += "\n";
        t += "Scroll - przybliżenie";
        t += "\n";
        t += "Zaznaczenie dotyczy najbliższego obiektu";

        popUp(t);
    }

    public void widokStrzelca()
    {
        var guns = FindObjectsOfType<Gun>();
        if (guns.Length > 0)
        {
            cam.mode = 2;
            cam.gun = guns[0].gameObject;
            cam.transform.rotation = cam.gun.transform.rotation;
        }
        selected = null;

        string t = "";
        t += "PPM - obrót kamery";
        t += "\n";
        t += "Spacja - strzał";
        popUp(t);
    }

    int popUpLife = 0;
    float popOpacity = 0;
    float popOpacityPivot = 0;

    string textBuffer = "";
    int popTime = 60 * 5;
    public void popUp(string text)
    {
        popUpLife = popTime;
        textBuffer = text;
        //popText.text = text;
    }
}
