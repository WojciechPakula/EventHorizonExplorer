using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class MeshDataPack {
    public List<Vector3Int> bvh;  //Drzewo - B_localIndex, BVH_1 (lewy węzeł), BVH_2 (prawy węzeł), oznacza pudełko i 2 węzły lub T_localIndex, 0, 0 oznacza, liść czyli trójkąt, bez pudełka
    public List<Vector3> B;  //x,y,z,X,Y,Z - granice pudełka
    public List<Vector3Int> T;    //trójkąty
    public List<int> SUB;    //submeshes
    public List<subMeshMaterial> SUB2;    //submeshes
    public List<Vector3> V;  //wierzchołki
    public List<Vector3> N;
    public List<Vector2> UV;
    public List<Material> MAT;
    public int id;

    //public Vector3 position;
    //public Quaternion rotation;
    //public Vector3 scale;
    //public int id;
    //public bool isActive = true;
    Wezel root = null;
    int boxCount = 0;
    public string meshCheckSum = null;

    static string folderName = "BVHdata";
    static Dictionary<string, List<Vector3Int>> treesSetCache = new Dictionary<string, List<Vector3Int>>();
    static Dictionary<int, Texture> texturesCache = new Dictionary<int, Texture>();

    static int checksumIdIterator = 0;
    public static Dictionary<string, int> checksumId = new Dictionary<string, int>();
    static int textureIdIterator = 0;
    public static Dictionary<int, int> textureIds = new Dictionary<int, int>();

    public static void reset()
    {
        checksumId.Clear();
        textureIds.Clear();
        checksumIdIterator = 0;
        textureIdIterator = 0;
        texturesCache.Clear();
        treesSetCache.Clear();
    }

    public static Texture[] getTextures()
    {
        int count = textureIds.Count;
        Texture[] ret = new Texture[count];
        foreach(var pair in texturesCache)
        {
            ret[textureIds[pair.Key]] = pair.Value;
        }
        return ret;
    }

    public static int getId(string meshCheckSum, int InstanceID, bool dynamicMesh, string texture_tag)
    {
        string key = meshCheckSum;
        key += "_" + texture_tag;
        if (dynamicMesh) key += "_" + InstanceID;

        bool con = checksumId.ContainsKey(key);

        int val = 0;
        if (con)
        {
            checksumId.TryGetValue(key, out val);
        } else
        {
            val = checksumIdIterator;
            checksumId.Add(key, val);
            checksumIdIterator++;
        }
        return val;
    }


    public static void loadTreesSet()
    {
        treesSetCache = new Dictionary<string, List<Vector3Int>>();
        var paths = Directory.GetFiles(folderName);
        foreach (var p in paths)
        {
            var arr = File.ReadAllBytes(p);
            int[] intArray = new int[arr.Length / 4];
            Buffer.BlockCopy(arr, 0, intArray, 0, arr.Length);
            var bvhtmp = new List<Vector3Int>();
            for (int i = 0; i < intArray.Length; i+=3)
            {
                Vector3Int v = new Vector3Int(intArray[i], intArray[i+1], intArray[i+2]);
                bvhtmp.Add(v);
            }
            

            var tmp = p.Split('\\');
            var index = tmp[tmp.Length - 1];
            treesSetCache.Add(index, bvhtmp);
        }
    }
    public static string getMeshChecksum(Mesh m)
    {
        var V = m.vertices;
        float[] floatArray = new float[V.Length * 3];
        int i = 0;
        foreach (var ele in V)
        {
            floatArray[i * 3 + 0] = ele.x;
            floatArray[i * 3 + 1] = ele.y;
            floatArray[i * 3 + 2] = ele.z;
            i++;
        }
        var byteArray = new byte[floatArray.Length * 4];
        Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
        using (var md5 = MD5.Create())
        {
            var hash = md5.ComputeHash(byteArray);
            var meshCheckSum = BitConverter.ToString(hash).Replace("-", "");
            return meshCheckSum;
        }
    }

    static List<subMeshMaterial> loadTextures(List<Material> M, List<int> S)
    {
        List<subMeshMaterial> ret = new List<subMeshMaterial>();
        if (M.Count != S.Count)
        {
            UnityEngine.Debug.Log("ERROR: M.Count != S.Count\t" + M.Count + "!=" + S.Count);
            return ret;
        }

        for (int i = 0; i < M.Count; ++i)
        {
            var ele = M[i];
            var tex = ele.mainTexture;
            int texId = -1;
            if (tex != null)
            {
                var id = tex.GetInstanceID();
                if (!texturesCache.ContainsKey(id))
                {
                    texturesCache.Add(id, tex);
                    textureIds.Add(id, textureIdIterator);
                    textureIdIterator++;
                }
                texId = textureIds[id];
            }
            subMeshMaterial sm = new subMeshMaterial(S[i], texId, ele.color, ele.mainTextureScale, ele.mainTextureOffset);
            ret.Add(sm);
        }
        return ret;
    }

    public MeshDataPack(Mesh m, MeshRenderer mr, int InstanceID, bool dynamicMesh)
    {
        if (treesSetCache.Count == 0)
            MeshDataPack.loadTreesSet();
        meshToTables(m, mr, out V, out N, out UV, out T, out SUB, out MAT);//5ms
        SUB2 = loadTextures(MAT, SUB);
        var Ttag = "";
        try
        {
            Ttag = mr.material.mainTexture.name;
        }
        catch
        {

        }
        meshCheckSum = getMeshChecksum(m);//8ms
        id = getId(meshCheckSum, InstanceID, dynamicMesh, Ttag);
        init();
    }
    public MeshDataPack(Mesh m, SkinnedMeshRenderer ms, int InstanceID, bool dynamicMesh)
    {
        if (treesSetCache.Count == 0)
            MeshDataPack.loadTreesSet();
        meshToTables(m, ms, out V, out N, out UV, out T, out SUB, out MAT);//5ms
        SUB2 = loadTextures(MAT, SUB);
        var Ttag = "";
        try
        {
            Ttag = ms.material.mainTexture.name;
        }
        catch
        {

        }
        meshCheckSum = getMeshChecksum(m);//8ms
        id = getId(meshCheckSum, InstanceID, dynamicMesh, Ttag);
        init();
    }

    void init()
    {
        bvh = null;
        var czyZnalezionoDrzewo = treesSetCache.TryGetValue(meshCheckSum, out bvh);

        if (czyZnalezionoDrzewo)
        {
            root = rebuildRootTree(bvh);
            updateBoxes();
        }
        else
        {
            root = getLogicTree();
            rootToTables();

            var byteArray = new byte[bvh.Count * sizeof(int) * 3];
            var intBvhArray = new int[bvh.Count * 3];
            int i = 0;
            foreach (var ele in bvh)
            {
                intBvhArray[i] = ele.x;
                i++;
                intBvhArray[i] = ele.y;
                i++;
                intBvhArray[i] = ele.z;
                i++;
            }

            Buffer.BlockCopy(intBvhArray, 0, byteArray, 0, byteArray.Length);
            File.WriteAllBytes(folderName + "\\" + meshCheckSum, byteArray);
        }
    }

    Wezel rebuildRootTree(List<Vector3Int> list)
    {
        Vector3Int[] bvhTable = list.ToArray();
        Wezel w = new Wezel();
        rebuildRecurention(bvhTable, w, 0);
        return w;
    }

    void rebuildRecurention(Vector3Int[] bvhTable, Wezel w, int index)
    {
        int id = bvhTable[index].x;
        int n1 = bvhTable[index].y;
        int n2 = bvhTable[index].z;

        w.id = id;

        if (n1 == 0 && n2 == 0)
        {

        }
        else
        {
            var w1 = new Wezel();
            w.n1 = w1;
            var w2 = new Wezel();
            w.n2 = w2;
            rebuildRecurention(bvhTable, w1, n1);
            rebuildRecurention(bvhTable, w2, n2);
        }
    }


    public Vector3Int[] Tarr;
    public Vector3[] Varr;
    public Vector3[] Barr;
    public void updateBoxes()
    {
        Tarr = T.ToArray();
        Varr = V.ToArray();
        //Barr = B.ToArray();
        int size = bvh.Count - T.Count;
        Barr = new Vector3[size * 2];
        Vector3 b0, B0;
        updateBoxesRec(root, out b0, out B0);
        B = new List<Vector3>(Barr);
        Barr = null;
        Varr = null;
        Tarr = null;
    }

    private void updateBoxesRec(Wezel w, out Vector3 b0, out Vector3 B0)
    {
        if (w.n1 == null && w.n2 == null)
        {
            Vector3 a = Varr[Tarr[w.id].x];
            Vector3 b = Varr[Tarr[w.id].y];
            Vector3 c = Varr[Tarr[w.id].z];
            triangleToBox(a, b, c, out b0, out B0);
        }
        else
        {
            Vector3 b1, B1, b2, B2;
            updateBoxesRec(w.n1, out b1, out B1);
            updateBoxesRec(w.n2, out b2, out B2);
            getMergeBox(b1, B1, b2, B2, out b0, out B0);
            w.boxmin = b0;
            w.boxmax = B0;
            w.solveCentroid();
            Barr[w.id] = b0;
            Barr[w.id + 1] = B0;
        }
    }

    void rootToTables()
    {
        B = new List<Vector3>();
        bvh = new List<Vector3Int>();
        iteracja = 0;
        Bindex = 0;
        bvharr = new Vector3Int[boxCount];
        int outIndex;
        try
        {
            rekurencja(root, out outIndex);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e.ToString());
        }
        bvh = new List<Vector3Int>(bvharr);
        bvharr = null;
        iteracja = 0;
        Bindex = 0;
    }

    int iteracja;
    int Bindex;
    Vector3Int[] bvharr;
    void rekurencja(Wezel w, out int index)
    {
        index = iteracja; //*3
        bvharr[index].x = Bindex;
        iteracja++;
        
        int index1_v = 0;
        int index2_v = 0;

        if (w.n1 == null && w.n2 == null)
        {
            bvharr[index].x = w.id;
        }
        else
        {
            B.Add(w.boxmin);
            B.Add(w.boxmax);
            Bindex += 2;
            if (w.n1 != null) rekurencja(w.n1, out index1_v);
            if (w.n2 != null) rekurencja(w.n2, out index2_v);
        }
        bvharr[index].y = index1_v;
        bvharr[index].z = index2_v;
    }

    public static void meshToTables(Mesh m, MeshRenderer mr, out List<Vector3> t_v, out List<Vector3> t_n, out List<Vector2> t_uv, out List<Vector3Int> t_t, out List<int> t_sub, out List<Material> t_materials)
    {
        List<Material> materials = new List<Material>();
        mr.GetMaterials(materials);

        List<Vector3> v = new List<Vector3>();
        List<Vector3> n = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<Vector3Int> t = new List<Vector3Int>();

        m.GetVertices(v);
        m.GetNormals(n);
        m.GetUVs(0, uv);

        var t_tmp = new List<int>(m.triangles);
        for (int i = 0; i < t_tmp.Count/3; ++i)
        {
            t.Add(new Vector3Int(t_tmp[i * 3 + 0], t_tmp[i * 3 + 1], t_tmp[i * 3 + 2]));
        }

        List<int> sub = getSubTable(m);

        t_v = v;
        t_n = n;

        if (uv.Count == 0)
        {
            for (int i = 0; i < v.Count; ++i)
            {
                uv.Add(new Vector2(0,0));
            }
        }
        t_uv = uv;
        t_t = t;
        t_sub = sub;
        t_materials = materials;
    }

    public static void meshToTables(Mesh m, SkinnedMeshRenderer ms, out List<Vector3> t_v, out List<Vector3> t_n, out List<Vector2> t_uv, out List<Vector3Int> t_t, out List<int> t_sub, out List<Material> t_materials)
    {
        List<Material> materials = new List<Material>();
        ms.GetMaterials(materials);

        List<Vector3> v = new List<Vector3>();
        List<Vector3> n = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<Vector3Int> t = new List<Vector3Int>();

        m.GetVertices(v);
        m.GetNormals(n);
        m.GetUVs(0, uv);

        var t_tmp = new List<int>(m.triangles);
        for (int i = 0; i < t_tmp.Count / 3; ++i)
        {
            t.Add(new Vector3Int(t_tmp[i * 3 + 0], t_tmp[i * 3 + 1], t_tmp[i * 3 + 2]));
        }

        List<int> sub = getSubTable(m);

        t_v = v;
        t_n = n;
        t_uv = uv;
        t_t = t;
        t_sub = sub;
        t_materials = materials;
    }

    public static List<int> getSubTable(Mesh m)
    {
        List<int> ret = new List<int>();
        for (int i = 0; i < m.subMeshCount; ++i)
        {
            ret.Add((int)(m.GetIndexStart(i)/3));
        }
        return ret;
    }

    private class Wezel
    {
        public Wezel n1 = null;
        public Wezel n2 = null;
        public Vector3 boxmin;
        public Vector3 boxmax;
        public Vector3 c;
        public int id;

        public void solveCentroid()
        {
            c = (boxmax + boxmin) / 2.0f;
        }

        public void solveBox()
        {
            if (n1 != null && n2 != null)
            {
                Vector3 max, min;
                getMergeBox(n1.boxmin, n1.boxmax, n2.boxmin, n2.boxmax, out min, out max);
                boxmin = min;
                boxmax = max;
                solveCentroid();
            }
        }
    }
    private struct Para
    {
        public Vector3 c;
        public int id;
        public bool isInBox;

        public Para(Vector3 c, int id)
        {
            this.c = c;
            this.id = id;
            isInBox = false;
        }
    }

    static void triangleToBox(Vector3 a, Vector3 b, Vector3 c, out Vector3 min, out Vector3 max)
    {
        var x = Mathf.Min(a.x, b.x, c.x);
        var y = Mathf.Min(a.y, b.y, c.y);
        var z = Mathf.Min(a.z, b.z, c.z);
        var X = Mathf.Max(a.x, b.x, c.x);
        var Y = Mathf.Max(a.y, b.y, c.y);
        var Z = Mathf.Max(a.z, b.z, c.z);
        min = new Vector3(x, y, z);
        max = new Vector3(X, Y, Z);
    }

    public static void getMergeBox(Vector3 a, Vector3 A, Vector3 b, Vector3 B, out Vector3 c, out Vector3 C)
    {
        var x = Mathf.Min(a.x, b.x);
        var y = Mathf.Min(a.y, b.y);
        var z = Mathf.Min(a.z, b.z);
        var X = Mathf.Max(A.x, B.x);
        var Y = Mathf.Max(A.y, B.y);
        var Z = Mathf.Max(A.z, B.z);
        c = new Vector3(x, y, z);
        C = new Vector3(X, Y, Z);
    }

    public static Vector3 getCentroid(Vector3 a, Vector3 b, Vector3 c)
    {
        return (a + b + c) / 3.0f;
    }

    Wezel getLogicTree()
    {
        List<Para> centroids = new List<Para>();
        boxCount = 0;
        var Tarr = T.ToArray();
        var Varr = V.ToArray();
        for (int i = 0; i < T.Count; ++i)
        {
            var ai = Tarr[i].x;
            var bi = Tarr[i].y;
            var ci = Tarr[i].z;

            var a = Varr[ai];
            var b = Varr[bi];
            var c = Varr[ci];

            var centroid = getCentroid(a, b, c);
            Para p = new Para(centroid, i);
            centroids.Add(p);
        }

        List<Wezel> wezly = new List<Wezel>();

        var centroidsArr = centroids.ToArray();

        for (int i = 0; i < centroidsArr.Length; ++i)
        {
            var t1 = centroidsArr[i];
            Wezel w = new Wezel(); boxCount++;
            w.id = t1.id;
            Vector3 a = Varr[Tarr[t1.id].x];
            Vector3 b = Varr[Tarr[t1.id].y];
            Vector3 c = Varr[Tarr[t1.id].z];
            Vector3 boxmin;
            Vector3 boxmax;
            triangleToBox(a, b, c, out boxmin, out boxmax);
            w.boxmax = boxmax;
            w.boxmin = boxmin;
            w.solveCentroid();
            wezly.Add(w);
        }

        /*for (int i = 0; i < centroidsArr.Length; ++i)
        {
            var t1 = centroidsArr[i];
            if (t1.isInBox == true) continue;
            t1.isInBox = true;
            centroids.Remove(t1);

            int bestId = -1;
            float min = float.PositiveInfinity;
            foreach (var t in centroids)
            {
                if (t.isInBox == true) continue;
                float d = (t.c - t1.c).magnitude;
                if (d < min)
                {
                    min = d;
                    bestId = t.id;
                }
            }
            Wezel w = new Wezel();
            boxCount++;
            if (bestId == -1)
            {
                //if (bestId == -1) throw new System.Exception("Nie znaleziono pary dla trojkata w drzewie MVH");
                //nie ma pary :(
                w.id = t1.id;
                Vector3 a = Varr[Tarr[t1.id * 3 + 0]];
                Vector3 b = Varr[Tarr[t1.id * 3 + 1]];
                Vector3 c = Varr[Tarr[t1.id * 3 + 2]];
                Vector3 boxmin;
                Vector3 boxmax;
                triangleToBox(a, b, c, out boxmin, out boxmax);
                w.boxmax = boxmax;
                w.boxmin = boxmin;
                w.solveCentroid();
            }
            else
            {
                var t2 = centroidsArr[bestId];
                centroids.Remove(t2);
                t2.isInBox = true;

                Wezel w1 = new Wezel();
                boxCount++;
                Vector3 a = Varr[Tarr[t1.id * 3 + 0]];
                Vector3 b = Varr[Tarr[t1.id * 3 + 1]];
                Vector3 c = Varr[Tarr[t1.id * 3 + 2]];
                Vector3 boxmin;
                Vector3 boxmax;
                triangleToBox(a, b, c, out boxmin, out boxmax);
                w1.id = t1.id;
                w1.boxmin = boxmin;
                w1.boxmax = boxmax;
                w1.solveCentroid();
                Wezel w2 = new Wezel();
                boxCount++;
                a = Varr[Tarr[t2.id * 3 + 0]];
                b = Varr[Tarr[t2.id * 3 + 1]];
                c = Varr[Tarr[t2.id * 3 + 2]];
                triangleToBox(a, b, c, out boxmin, out boxmax);
                w2.id = t2.id;
                w2.boxmin = boxmin;
                w2.boxmax = boxmax;
                w2.solveCentroid();
                w.n1 = w1;
                w.n2 = w2;

                getMergeBox(w1.boxmin, w1.boxmax, w2.boxmin, w2.boxmax, out boxmin, out boxmax);
                w.boxmin = boxmin;
                w.boxmax = boxmax;
                w.solveCentroid();
            }
            wezly.Add(w);
        }*/
        //koniec petli
        //tutaj mogę operować tylko na pudłach
        //List<Wezel> wezly0 = new List<Wezel>(); //obliczone wezly

        LinkedList<Wezel> listIn = new LinkedList<Wezel>(wezly);
        LinkedList<Wezel> listOut = new LinkedList<Wezel>();

        while (listIn.Count > 1)
        {
            for (int i = 0; listIn.Count > 0; ++i)
            {

                var w1 = listIn.First.Value;
                listIn.RemoveFirst();

                Wezel best = null;
                int bestId = -1;
                float min = float.PositiveInfinity;
                int j = 0;
                foreach (var w2 in listIn)
                {
                    float d = (w2.c - w1.c).magnitude;
                    if (d < min)
                    {
                        min = d;
                        best = w2;
                        bestId = j;
                    }
                    j++;
                }
                if (best == null)
                {
                    //brak pary
                    listOut.AddFirst(w1);
                }
                else
                {
                    var w2 = best;
                    listIn.Remove(best);
                    //para
                    Wezel W = new Wezel(); boxCount++;
                    W.n1 = w1;
                    W.n2 = w2;
                    W.solveBox();
                    listOut.AddFirst(W);
                }
            }
            listIn = listOut;
            listOut = new LinkedList<Wezel>();
        }

        Wezel root = listIn.First.Value;
        return root;
    }
}

public struct subMeshMaterial {
    public int sub;
    public int textureId;
    public Color c;
    public Vector2 tiling;
    public Vector2 offset;

    public subMeshMaterial(int s, int tex, Color color, Vector2 tiling0, Vector2 offset0)
    {
        sub = s;
        textureId = tex;
        c = color;
        tiling = tiling0;
        offset = offset0;
    }
    /*public static int getClassSize()
    {
        return 4*2+4*8;
    }*/
}

public struct transformPack
{
    public int meshId;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public transformPack(int id, Vector3 pos, Quaternion rot, Vector3 sc)
    {
        meshId = id;
        position = pos;
        rotation = rot;
        scale = sc;
    }
    /*public static int getClassSize()
    {
        return 4 * 11;
    }*/
}