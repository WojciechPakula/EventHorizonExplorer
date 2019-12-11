using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Parser : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static string WorldToString()
    {
        var walls = FindObjectsOfType<Wall>();
        var targets = FindObjectsOfType<Target>();
        var blackHoles = FindObjectsOfType<Mass>();
        var guns = FindObjectsOfType<Gun>();

        JPack jp = new JPack();
        jp.walls = new JWall[walls.Length];
        for (int i = 0; i < walls.Length; ++i)
        {
            jp.walls[i] = new JWall();
            jp.walls[i].position = walls[i].transform.position;
            jp.walls[i].rotation = walls[i].transform.rotation;
            jp.walls[i].scale = walls[i].transform.localScale;
        }
        jp.targets = new JTarget[targets.Length];
        for (int i = 0; i < targets.Length; ++i)
        {
            jp.targets[i] = new JTarget();
            jp.targets[i].position = targets[i].transform.position;
            jp.targets[i].rotation = targets[i].transform.rotation;
            jp.targets[i].scale = targets[i].transform.localScale;
        }
        jp.masses = new JMass[blackHoles.Length];
        for (int i = 0; i < blackHoles.Length; ++i)
        {
            jp.masses[i] = new JMass();
            jp.masses[i].position = blackHoles[i].transform.position;
            jp.masses[i].radius = blackHoles[i].promien;
        }
        jp.guns = new JGun[guns.Length];
        for (int i = 0; i < guns.Length; ++i)
        {
            jp.guns[i] = new JGun();
            jp.guns[i].position = guns[i].transform.position;
        }
        string js = JsonUtility.ToJson(jp);
        return js;
    }

    public static void WorldFromString(String data)
    {
        try
        {
            var JPack = JsonUtility.FromJson<JPack>(data);

            var walls = FindObjectsOfType<Wall>();
            var targets = FindObjectsOfType<Target>();
            var blackHoles = FindObjectsOfType<Mass>();
            var guns = FindObjectsOfType<Gun>();

            for (int i = 0; i < walls.Length; ++i)
            {
                Destroy(walls[i].gameObject);
            }
            for (int i = 0; i < targets.Length; ++i)
            {
                Destroy(targets[i].gameObject);
            }
            for (int i = 0; i < blackHoles.Length; ++i)
            {
                Destroy(blackHoles[i].gameObject);
            }
            for (int i = 0; i < guns.Length; ++i)
            {
                Destroy(guns[i].gameObject);
            }

            foreach (var ele in JPack.walls)
            {
                GameObject f = (GameObject)Instantiate(Resources.Load("wall"));
                f.transform.position = ele.position;
                f.transform.rotation = ele.rotation;
                f.transform.localScale = ele.scale;
                RayTracingManager._transformsToWatch.Add(f.transform);
            }
            foreach (var ele in JPack.guns)
            {
                GameObject f = (GameObject)Instantiate(Resources.Load("gun"));
                f.transform.position = ele.position;
                RayTracingManager._transformsToWatch.Add(f.transform);
            }
            foreach (var ele in JPack.masses)
            {
                GameObject f = (GameObject)Instantiate(Resources.Load("bh"));
                f.transform.position = ele.position;
                var m = f.GetComponent<Mass>();
                m.promien = ele.radius;
                RayTracingManager._transformsToWatch.Add(f.transform);
            }
            foreach (var ele in JPack.targets)
            {
                GameObject f = (GameObject)Instantiate(Resources.Load("Target"));
                f.transform.position = ele.position;
                f.transform.rotation = ele.rotation;
                f.transform.localScale = ele.scale;
                RayTracingManager._transformsToWatch.Add(f.transform);
            }

        } catch
        {

        }
    }

    public static void stringToFile(string str, string fileName)
    {
        var sr = File.CreateText(fileName);
        sr.WriteLine(str);
        sr.Close();
    }

    public static string stringFromFile(string fileName)
    {
        string lines="";
        //if (!fileName.Contains(".json")) fileName = fileName + ".json";

        using (StreamReader sr = new StreamReader(fileName))
        {
            string line;
            // Read and display lines from the file until the end of 
            // the file is reached.
            while ((line = sr.ReadLine()) != null)
            {
                lines += line + "\n";
            }
        }
        return lines;
    }

    [Serializable]
    private struct JPack
    {
        public JWall[] walls;
        public JTarget[] targets;
        public JMass[] masses;
        public JGun[] guns;
    }
    [Serializable]
    private struct JWall
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
    [Serializable]
    private struct JTarget
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
    [Serializable]
    private struct JMass
    {
        public Vector3 position;
        public float radius;
    }
    [Serializable]
    private struct JGun
    {
        public Vector3 position;
    }
}


