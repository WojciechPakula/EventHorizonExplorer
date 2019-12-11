using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class TextureManager {
    static byte[] getArrayToByte(byte[] arr, int index, byte[] notAllowed)
    {
        List<byte> ret = new List<byte>();
        for (int i = index; i < arr.Length; ++i)
        {
            byte element = arr[i];
            foreach (var stop in notAllowed) if (element == stop) return ret.ToArray();
            
            ret.Add(element);
        }
        return ret.ToArray();
    }


    //działa
    public static Texture2D loadPFMtoTexture(string fileName)
    {
        byte[] data = File.ReadAllBytes(fileName);

        if ((data[0] == 'P' || data[0] == 'p') && (data[1] == 'F' || data[1] == 'f'))
        {
            //32 spacja
            bool littleEndian = true;
            byte[] notAllowed = new byte[2] {10, 32};

            int indexer = 3;
            var bw = getArrayToByte(data, indexer, notAllowed);
            indexer += bw.Length + 1;
            var bh = getArrayToByte(data, indexer, notAllowed);
            indexer += bh.Length + 1;
            var litt = getArrayToByte(data, indexer, notAllowed);
            if (litt[0] != (byte)'-') littleEndian = false;
            if (littleEndian == false) throw new Exception("bigEndian obrazu PFM nie jest obsługiwany");

            indexer += litt.Length + 1;

            string sw = System.Text.Encoding.UTF8.GetString(bw);
            string sh = System.Text.Encoding.UTF8.GetString(bh);

            int w = int.Parse(sw);
            int h = int.Parse(sh);

            byte[] floats = new byte[data.Length-indexer];
            System.Buffer.BlockCopy(data, indexer, floats, 0, floats.Length);
            Texture2D tex = new Texture2D(w, h, TextureFormat.RFloat, false);  //w tex tekstura jest obrocona w osi y
            tex.LoadRawTextureData(floats);

            //
            //byte[] bytes = tex.EncodeToPNG();
            //File.WriteAllBytes("loadPFMtoTexturePreview.png", bytes);
            //

            return tex;
        }

        return null;
    }

    //działa
    public static byte[] textureToPFM (Texture2D tex) {
        if (tex.format != TextureFormat.RFloat) throw new System.Exception("Tekstura musi być w formacie RFloat");
        var raw2 = tex.GetRawTextureData();
        int w = tex.width;
        int h = tex.height;

        byte[] header = getPFMHeader((uint)w, (uint)h);

        byte[] data = new byte[header.Length + raw2.Length];
        System.Buffer.BlockCopy(header, 0, data, 0, header.Length);
        System.Buffer.BlockCopy(raw2, 0, data, header.Length, raw2.Length);

        return data;
    }

    static byte[] getPFMHeader(uint w, uint h, bool littleEndian = true)
    {
        List<byte> info = new List<byte>();
        byte nowaLinia = 10;
        info.Add((byte)'P');
        info.Add((byte)'f');
        info.Add(nowaLinia);

        var wb = intToBytes((uint)w);
        var hb = intToBytes((uint)h);

        foreach (var ele in wb) info.Add(ele);
        info.Add((byte)' ');
        foreach (var ele in hb) info.Add(ele);
        info.Add(nowaLinia);
        if (littleEndian) info.Add((byte)'-');
        info.Add((byte)'1');
        info.Add((byte)'.');
        info.Add((byte)'0');
        info.Add(nowaLinia);
        return info.ToArray();
    }

    static byte[] intToBytes(uint v)
    {
        List<byte> cyfry = new List<byte>();
        for (; v>0; )
        {
            byte r = (byte)((v % 10) + 48);
            v = v / 10;
            cyfry.Add(r);
        }
        cyfry.Reverse();
        return cyfry.ToArray();
    }

    public static bool ByteArrayToFile(string fileName, byte[] byteArray)
    {
        try
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(byteArray, 0, byteArray.Length);
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    public static Texture2D floatsToTexture(float[] data, int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RFloat, false);
        List<byte> raw = new List<byte>();
        for (int y = 0; y < h; ++y)
        {
            for (int x = 0; x < w; ++x)
            {
                float ele = data[y * w + x];
                byte[] ar = System.BitConverter.GetBytes(ele);
                raw.Add(ar[0]);
                raw.Add(ar[1]);
                raw.Add(ar[2]);
                raw.Add(ar[3]);
            }
        }
        tex.LoadRawTextureData(raw.ToArray());
        return tex;
    }

    public static float[] textureToFloats(Texture2D tex)
    {
        var bytes = tex.GetRawTextureData<float>();
        return bytes.ToArray();
    }

    
}
