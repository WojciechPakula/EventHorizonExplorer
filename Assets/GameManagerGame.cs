using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameManagerGame : GameManager
{
    public GameObject menuEdit;
    public GameObject menuEdit2;

    public Button nextLvlBtn;

    float positionX = -204.05f-50;
    float positionY = 253.62f+50;
    float positionStep = 253.62f - 198.2f;

    public Text statText;

    int statLife = 0;
    float statOpacity = 0;
    float statOpacityPivot = 0;
    
    int statTime = 60 * 3;
    public void statUp(string text)
    {
        statLife = statTime;
        statText.text = text;
        //popText.text = text;
    }

    void dodajPrzycisk(float r)
    {
        GameObject f = (GameObject)Instantiate(Resources.Load("BhButton"));
        f.transform.parent = menuEdit2.transform;
        f.transform.localPosition = new Vector3(positionX, positionY, 0);

        var comp = f.GetComponent<BhButton>();
        comp.r = r;
        positionY -= positionStep;
    }

    // Start is called before the first frame update
    void Start()
    {
        hits = 0;
        shoots = 0;
        instance = this;
        ptrMeshRenderer = pointer.GetComponent<MeshRenderer>();
        /*dodajPrzycisk(5);
        dodajPrzycisk(10);
        dodajPrzycisk(15);*/
        loadWorld(GameManager.folderToLoad+"/"+GameManager.lvlToLoad+".json");

        var obs = FindObjectsOfType<Mass>();
        for (int i = 0; i < obs.Length; ++i)
        {
            var o = obs[i];
            dodajPrzycisk(o.promien);
            Destroy(o.gameObject);
        }
        widokStrzelca();
    }



    // Update is called once per frame
    void Update()
    {
        string statstr = "trafienia/strzały: "+GameManager.hits+"/"+GameManager.shoots;
        if (statstr != statText.text)
        {
            statUp(statstr);
        }

        if (hits > 0) {
            nextLvlBtn.interactable = true;
            popUp("Cel osiągnięty! Następny poziom");
        }
        else
            nextLvlBtn.interactable = false;
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


        if (selected != null)
        {
            selected.transform.position = new Vector3(cam.transform.position.x, 0, cam.transform.position.z);
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

        if (statLife > 0)
        {
            statLife--;
            statOpacity = 1;
            statOpacityPivot = Mathf.Lerp(statOpacityPivot, 1, Time.deltaTime * 5);
        }
        else
        {
            statOpacity = 0;
            statOpacityPivot = Mathf.Lerp(statOpacityPivot, 0, Time.deltaTime * 5);
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

        Color zm2 = statText.color;
        zm2.a = statOpacityPivot;
        statText.color = zm2;

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
            }

        }
    }

    public void odznacz()
    {
        selected = null;
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
            var mas = selected.GetComponent<Mass>();

            var go = selected.gameObject;

            var ms = FindObjectsOfType<BhButton>();
            foreach (var m in ms) {
                if (m.locked == true && m.m == mas)
                {
                    m.locked = false;
                    break;
                }
            }

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

    public void next()
    {
        if (GameManager.isFirst) {
            unlockedLevels++;
            GameManager.saveLvl();
        }
        SceneManager.LoadScene("Menu");
    }

    public void quit()
    {
        SceneManager.LoadScene("Menu");
    }


    /*public void parserTest()
    {
        //saveWorld();
        loadWorld("SavedMaps/2019-9-19-0-3-22.json");
    }*/

    /*public void saveWorld()
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


    }*/

    string lastSaved = "";

    /*public void load()
    {
        string p = "SavedMaps/" + nazwaWczytania.text;
        if (!p.Contains(".json"))
        {
            p += ".json";
        }
        loadWorld(p);
    }*/

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


    public void help()
    {
        string p = "";
        p+=("Pomoc\n");
        p+=("Celem gry jest trafienie w tarczę, która znajduje się na planszy, z działa które strzela cząsteczkami światła.\n");
        p+=("Mapy zbudowane są tak, że nie da się trafić w tarczę w lini prostej. Zadaniem gracza jest ustawienie czarnych dziur na mapie w taki sposób, żeby strumień światła zagiął się i poleciał prosto w tarczę.\n");
        p += ("Czarną dziurę można postawić w trybie widoku z góry, przyciskami znajdującymi się w lewym górnym rogu ekranu.\n");
        popUp(p);
    }

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
