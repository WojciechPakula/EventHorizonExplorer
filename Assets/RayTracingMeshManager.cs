using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class RayTracingMeshManager : MonoBehaviour {

    // Use this for initialization
    void Start() {
        //QualitySettings.vSyncCount = 1;
        
        //Application.targetFrameRate = 10;
        Stopwatch stopwatch = Stopwatch.StartNew();
        //MeshDataPack.loadTreesSet();
        //loadTextures();
        stopwatch.Stop();
        killChilds();
        UnityEngine.Debug.Log("RayTracingMeshManager init: " + stopwatch.ElapsedMilliseconds+" ms");
    }

    //public Dictionary<int, Texture> textures = new Dictionary<int, Texture>();

    //xd
    void killChilds()
    {
        var chc = transform.childCount;
        for (int i = 0; i < chc; ++i)
        {
            var child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }
	
	// Update is called once per frame
	void Update () {
        /*Stopwatch stopwatch = Stopwatch.StartNew();
        var stat = RayTracingMeshRenderer.getStaticMeshes();//
        stopwatch.Stop();
        UnityEngine.Debug.Log("Stat: " + stat.Count+"\tczas: "+ stopwatch.ElapsedMilliseconds);

        Stopwatch stopwatch2 = Stopwatch.StartNew();
        var stat2 = RayTracingMeshRenderer.getDynamicMeshes();//
        stopwatch2.Stop();
        UnityEngine.Debug.Log("Dyn: " + stat2.Count + "\tczas: " + stopwatch2.ElapsedMilliseconds);

        Stopwatch stopwatch3 = Stopwatch.StartNew();
        var ren = RayTracingMeshRenderer.getObjectsToRender();
        stopwatch3.Stop();
        UnityEngine.Debug.Log("Ren: " + ren.Count + "\tczas: " + stopwatch3.ElapsedMilliseconds);*/
        
    }

    

    /*void testBuffers()
    {

        ComputeBuffer _SUB2 = new ComputeBuffer(1000 * 1000, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float)));
    }*/
}
