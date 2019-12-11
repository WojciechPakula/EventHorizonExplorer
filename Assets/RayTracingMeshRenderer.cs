using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class RayTracingMeshRenderer : MonoBehaviour {
    MeshRenderer mr;
    SkinnedMeshRenderer ms;
    Mesh m;
    public bool dynamicMesh;
    public MeshDataPack data;

	// Use this for initialization
	void Awake () {
        try
        {
            var mf = GetComponent<MeshFilter>();
            mr = GetComponent<MeshRenderer>();

            if (mr == null)
            {
                ms = GetComponent<SkinnedMeshRenderer>();
                m = ms.sharedMesh;
                data = new MeshDataPack(m, ms, this.GetInstanceID(), dynamicMesh);
            }
            else
            {
                m = mf.mesh;
                data = new MeshDataPack(m, mr, this.GetInstanceID(), dynamicMesh);
            }
        } catch (Exception e)
        {
            UnityEngine.Debug.Log("RayTracingMeshRenderer Awake error: "+e);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    /*public string getMeshChecksum()
    {
        if (meshCheckSum != null) return meshCheckSum;
        meshCheckSum = BVH3.getMeshChecksum(m);
        return meshCheckSum;
    }*/

    //static
    static ComputeBuffer _static_V = null;
    static ComputeBuffer _static_N = null;
    static ComputeBuffer _static_UV = null;
    static ComputeBuffer _static_T = null;
    static ComputeBuffer _static_B = null;
    static ComputeBuffer _static_bvh = null;
    static ComputeBuffer _static_SUB2 = null;
    static ComputeBuffer _static_IND = null;

    public static void pushStaticMeshes(ComputeShader RayTracingShader)
    {
        //To robiłem 15.04.2019
        Dictionary<int, MeshDataPack> stat = RayTracingMeshRenderer.getStaticMeshes();//

        var INDlist = new List<int>();

        var Vlist = new List<Vector3>();
        var Nlist = new List<Vector3>();
        var UVlist = new List<Vector2>();
        var Tlist = new List<Vector3Int>();
        var Blist = new List<Vector3>();
        var BVHlist = new List<Vector3Int>();
        var SUB2list = new List<subMeshMaterial>();


        foreach (var pair in stat)
        {
            MeshDataPack MDP = pair.Value;

            INDlist.Add(MDP.id);
            INDlist.Add(BVHlist.Count);
            INDlist.Add(Blist.Count);
            INDlist.Add(Tlist.Count);
            INDlist.Add(Vlist.Count);
            INDlist.Add(SUB2list.Count);

            Vlist.AddRange(MDP.V);
            Nlist.AddRange(MDP.N);
            UVlist.AddRange(MDP.UV);
            Tlist.AddRange(MDP.T);
            Blist.AddRange(MDP.B);
            BVHlist.AddRange(MDP.bvh);
            SUB2list.AddRange(MDP.SUB2);
        }
         _static_V = new ComputeBuffer(Vlist.Count, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector3)));
         _static_N = new ComputeBuffer(Nlist.Count, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector3)));
         _static_UV = new ComputeBuffer(UVlist.Count, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector2)));
         _static_T = new ComputeBuffer(Tlist.Count, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector3Int)));
         _static_B = new ComputeBuffer(Blist.Count, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector3)));
         _static_bvh = new ComputeBuffer(BVHlist.Count, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector3Int)));
        int s = System.Runtime.InteropServices.Marshal.SizeOf(typeof(subMeshMaterial));
         _static_SUB2 = new ComputeBuffer(SUB2list.Count, s);
         _static_IND = new ComputeBuffer(INDlist.Count, System.Runtime.InteropServices.Marshal.SizeOf(typeof(int)));

        _static_V.SetData(Vlist);
        _static_N.SetData(Nlist);
        _static_UV.SetData(UVlist);
        _static_T.SetData(Tlist);
        _static_B.SetData(Blist);
        _static_bvh.SetData(BVHlist);
        _static_SUB2.SetData(SUB2list);
        _static_IND.SetData(INDlist);

        RayTracingShader.SetBuffer(0, "_static_V", _static_V); //_static_V.Release();
        RayTracingShader.SetBuffer(0, "_static_N", _static_N); //_static_N.Release();
        RayTracingShader.SetBuffer(0, "_static_UV", _static_UV); //_static_UV.Release();
        RayTracingShader.SetBuffer(0, "_static_T", _static_T); //_static_T.Release();
        RayTracingShader.SetBuffer(0, "_static_B", _static_B); //_static_B.Release();
        RayTracingShader.SetBuffer(0, "_static_bvh", _static_bvh); //_static_bvh.Release();
        RayTracingShader.SetBuffer(0, "_static_SUB2", _static_SUB2); //_static_SUB2.Release();
        RayTracingShader.SetBuffer(0, "_static_IND", _static_IND); //_static_IND.Release();
    }

    public static void dispose()
    {
        if (_static_V != null) _static_V.Release();
        if (_static_N != null) _static_N.Release();
        if (_static_UV != null) _static_UV.Release();
        if (_static_T != null) _static_T.Release();
        if (_static_B != null) _static_B.Release();
        if (_static_bvh != null) _static_bvh.Release();
        if (_static_SUB2 != null) _static_SUB2.Release();
        if (_static_IND != null) _static_IND.Release();
        if (_static_V != null) _static_V.Release();

        if (_static_transforms != null) _static_transforms.Release();

        if (_static_Textures != null) _static_Textures.Release();
        if (_static_Textures_IND != null) _static_Textures_IND.Release();
    }


    static ComputeBuffer _static_transforms = null;
    public static void pushGameObjects(ComputeShader RayTracingShader)
    {
        var renders = RayTracingMeshRenderer.getObjectsToRender();

        var packs = new List<transformPack>();

        foreach (var obj in renders)
        {
            transformPack pack = new transformPack(obj.data.id, obj.transform.position, obj.transform.rotation, obj.transform.localScale);
            packs.Add(pack);
        }
        int s = System.Runtime.InteropServices.Marshal.SizeOf(typeof(transformPack));
        if (_static_transforms!=null) _static_transforms.Release();
        //_static_transforms = new ComputeBuffer(0, s);
        if (packs.Count > 0)
        {
            _static_transforms = new ComputeBuffer(packs.Count, s);
            _static_transforms.SetData(packs);
            RayTracingShader.SetBuffer(0, "_static_transforms", _static_transforms);
        }
    }

    /*private void OnDisable()
    {
        if (_static_transforms != null)
            _static_transforms.Release();
    }*/

    public static Dictionary<int, MeshDataPack> getStaticMeshes()
    {
        var objects = UnityEngine.Object.FindObjectsOfType<RayTracingMeshRenderer>();

        //List<MeshDataPack> PAC = new List<MeshDataPack>();
        Dictionary<int, MeshDataPack> PAC = new Dictionary<int, MeshDataPack>();
        foreach (var ob in objects)
        {
            if (ob.dynamicMesh) continue;
            var id = ob.data.id;
            if (!PAC.ContainsKey(id))
            {
                PAC.Add(id, ob.data);
            }
        }
        return PAC;
    }

    public static Dictionary<int, MeshDataPack> getDynamicMeshes()
    {
        var objects = UnityEngine.Object.FindObjectsOfType<RayTracingMeshRenderer>();

        //List<MeshDataPack> PAC = new List<MeshDataPack>();
        Dictionary<int, MeshDataPack> PAC = new Dictionary<int, MeshDataPack>();
        foreach (var ob in objects)
        {
            if (!ob.dynamicMesh) continue;
            var id = ob.data.id;
            if (!PAC.ContainsKey(id))
            {
                PAC.Add(id, ob.data);
            }
        }
        return PAC;
    }

    public static List<RayTracingMeshRenderer> getObjectsToRender()
    {
        var objects = UnityEngine.Object.FindObjectsOfType<RayTracingMeshRenderer>();
        List<RayTracingMeshRenderer> lista = new List<RayTracingMeshRenderer>();
        foreach (var ele in objects)
        {
            if (ele.isActiveAndEnabled) lista.Add(ele);
        }
        return lista;
    }
    static ComputeBuffer _static_Textures =null;
    static ComputeBuffer _static_Textures_IND =null;
    public static void pushTextures(ComputeShader RayTracingShader)
    {
        var texs = MeshDataPack.getTextures();
        
        List<Color> cs = new List<Color>();
        List<Vector3Int> indexes = new List<Vector3Int>();
        foreach (var tex in texs)
        {
            var tmp = (Texture2D)tex;
            var carr = tmp.GetPixels();
            indexes.Add(new Vector3Int(cs.Count, tmp.width, tmp.height));
            cs.AddRange(carr);
        }
        _static_Textures = new ComputeBuffer(cs.Count, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Color)));
        _static_Textures_IND = new ComputeBuffer(indexes.Count, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector3Int)));

        _static_Textures.SetData(cs);
        _static_Textures_IND.SetData(indexes);

        RayTracingShader.SetBuffer(0, "_static_Textures", _static_Textures); //_static_Textures.Release();
        RayTracingShader.SetBuffer(0, "_static_Textures_IND", _static_Textures_IND); //_static_Textures_IND.Release();
    }
}
