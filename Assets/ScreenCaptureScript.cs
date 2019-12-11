using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenCaptureScript : MonoBehaviour
{
    string FolderName = "captureFolder";
    // Start is called before the first frame update
    void Start()
    {
        DateTime t = DateTime.Now;
        FolderName += t.Day+"-"+t.Month+"-"+t.Year+"-"+t.Hour+"-"+t.Minute+"-"+t.Second;
        Time.captureFramerate = 60;
        Time.fixedDeltaTime = 1.0f / 60.0f;
        System.IO.Directory.CreateDirectory(FolderName);
    }

    // Update is called once per frame
    void Update()
    {
        // Append filename to folder name (format is '0005 shot.png"')
        string name = string.Format("{0}/{1:D04} shot.png", FolderName, Time.frameCount);

        // Capture the screenshot to the specified file.
        UnityEngine.ScreenCapture.CaptureScreenshot(name);
        Debug.Log(Time.frameCount);
    }
}
