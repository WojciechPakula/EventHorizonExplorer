using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static int shoots = 0;
    public static int hits = 0;
    public static GameManager instance;
    public static string lvlToLoad = "";
    public static string folderToLoad = "";
    public static bool isFirst = false;

    public static int unlockedLevels = 1;

    public static ComputeBuffer _textureGravity=null;
    public static int w=0;
    public static int h=0;

    public static void saveLvl()
    {
        Parser.stringToFile(unlockedLevels.ToString(), "config.dat");
    }

    private void Start()
    {
        
    }
    private void Update()
    {
        
    }
}
