using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracingManager : MonoBehaviour
{
    public ComputeShader RayTracingShader;
    public Texture SkyboxTexture;

    //debug
    public List<GameObject> bh_t = new List<GameObject>();

    private Camera _camera;
    private float _lastFieldOfView;
    private RenderTexture _target;
    private RenderTexture _converged;
    private Material _addMaterial;
    private uint _currentSample = 0;
    private ComputeBuffer _blackHoleBuffer;
    //private ComputeBuffer _textureGravity;
    private ComputeBuffer _textureGravityFar;
    public static List<Transform> _transformsToWatch = new List<Transform>();

    struct sBlackHole
    {
        public Vector3 position;
        public float rs;
    }

    // Start is called before the first frame update
    void Start()
    {
        RayTracingMeshRenderer.pushStaticMeshes(RayTracingShader);
        RayTracingMeshRenderer.pushTextures(RayTracingShader);

        var obs_ = RayTracingMeshRenderer.getObjectsToRender();
        foreach (var ele in obs_) _transformsToWatch.Add(ele.transform);
        var m = FindObjectsOfType<Mass>();
        bh_t.Clear();
        foreach (var ele in m)
        {
            bh_t.Add(ele.gameObject);
            _transformsToWatch.Add(ele.transform);
        }
    }

    private void Awake()
    {
        try
        {
            _camera = GetComponent<Camera>();
            _transformsToWatch.Add(transform);
            //foreach (var ele in bh_t) _transformsToWatch.Add(ele.transform);
            
            RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
            RayTracingShader.SetInt("_rayLimit", 5);
            initGravityTexture();
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    void initGravityTexture()
    {
        

        if (GameManager._textureGravity == null)
        {
            var tex = TextureManager.loadPFMtoTexture("BLACK_HOLE_ULTRA.pfm");
            //var tex = TextureManager.loadPFMtoTexture("FAST_SIM_ULTRA.pfm");
            var floatArr = TextureManager.textureToFloats(tex);
            GameManager._textureGravity = new ComputeBuffer(tex.width * tex.height, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float)));
            GameManager._textureGravity.SetData(floatArr);
            RayTracingShader.SetBuffer(0, "_textureGravity", GameManager._textureGravity);
            GameManager.w = tex.width;
            GameManager.h = tex.height;
            RayTracingShader.SetInt("_interpolationMode", 0);
            RayTracingShader.SetInt("_textureWidth", tex.width);
            RayTracingShader.SetInt("_textureHeight", tex.height);
        } else
        {
            RayTracingShader.SetBuffer(0, "_textureGravity", GameManager._textureGravity);
            RayTracingShader.SetInt("_interpolationMode", 0);
            RayTracingShader.SetInt("_textureWidth", GameManager.w);
            RayTracingShader.SetInt("_textureHeight", GameManager.h);
        }
        

        var tex2 = TextureManager.loadPFMtoTexture("BLACK_HOLE_FAR_DATA.pfm");
        var floatArr2 = TextureManager.textureToFloats(tex2);
        _textureGravityFar = new ComputeBuffer(tex2.width, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float)));
        _textureGravityFar.SetData(floatArr2);
        RayTracingShader.SetInt("_textureWidthFar", tex2.width);
        RayTracingShader.SetBuffer(0, "_textureGravityFar", _textureGravityFar);
    }

    private void OnEnable()
    {
        _currentSample = 0;
        SetUpScene();
    }

    private void OnDisable()
    {
        if (_blackHoleBuffer != null)
            _blackHoleBuffer.Release();
        /*if (_textureGravity != null)
            _textureGravity.Release();*/
        _transformsToWatch.Clear();
        RayTracingMeshRenderer.dispose();
        MeshDataPack.reset();
    }



    // Update is called once per frame
    void Update()
    {
        SetUpScene();
        List<Transform> tmpList = new List<Transform>();
        foreach (Transform t in _transformsToWatch)
        {
            try
            {
                if (t != null) tmpList.Add(t);
                if (t.hasChanged)
                {
                    _currentSample = 0;
                    t.hasChanged = false;
                }
            } catch
            {

            }
        }
        _transformsToWatch = tmpList;
    }

    private void SetUpScene()
    {
        /*List<sBlackHole> blackHoles = new List<sBlackHole>();

        for (int i = 0; i < bh_t.Length; ++i)
        {
            var  b = bh_t[i];
            sBlackHole bhx = new sBlackHole();

            var comp = b.GetComponent<BlackHole>();
            bhx.rs = comp.rs;
            bhx.position = b.transform.position;
            blackHoles.Add(bhx);
        }

        if (_blackHoleBuffer != null)
            _blackHoleBuffer.Release();
        if (blackHoles.Count > 0)
        {
            int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(sBlackHole));
            _blackHoleBuffer = new ComputeBuffer(blackHoles.Count, size);
            _blackHoleBuffer.SetData(blackHoles);
        }*/
    }

    /////////

    ComputeBuffer outBuffer;
    private void SetShaderParameters()
    {
        //RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
        RayTracingShader.SetFloat("_Seed", Random.value);

        outBuffer = new ComputeBuffer(1, 4*3);

        RayTracingShader.SetBuffer(0, "DEBoutput", outBuffer);


        var objects = UnityEngine.Object.FindObjectsOfType<Mass>();
        RayTracingShader.SetInt("_bhCount", objects.Length);
       
        if (objects.Length > 0) {
            List<sBlackHole> blackHoles = new List<sBlackHole>();

            for (int i = 0; i < objects.Length; ++i)
            {
                var comp = objects[i];
                sBlackHole bhx = new sBlackHole();

                //var comp = b.GetComponent<BlackHole>();
                bhx.rs = comp.rs;
                bhx.position = comp.transform.position;
                blackHoles.Add(bhx);
            }

            if (_blackHoleBuffer != null)
                _blackHoleBuffer.Release();
            if (blackHoles.Count > 0)
            {
                int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(sBlackHole));
                _blackHoleBuffer = new ComputeBuffer(blackHoles.Count, size);
                _blackHoleBuffer.SetData(blackHoles);
            }
            if (_blackHoleBuffer != null)
                RayTracingShader.SetBuffer(0, "_BlackHoles", _blackHoleBuffer);
        } else
        {
            if (_blackHoleBuffer != null)
                _blackHoleBuffer.Release();
            List<sBlackHole> blackHoles = new List<sBlackHole>();
            sBlackHole bhx = new sBlackHole();
            bhx.position = new Vector3(0, 0, 0);
            bhx.rs = 0;
            blackHoles.Add(bhx);

            int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(sBlackHole));
            _blackHoleBuffer = new ComputeBuffer(blackHoles.Count, size);
            _blackHoleBuffer.SetData(blackHoles);
            RayTracingShader.SetBuffer(0, "_BlackHoles", _blackHoleBuffer);
        }


        
        
        RayTracingMeshRenderer.pushGameObjects(RayTracingShader);
    }

    private void InitRenderTexture()
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (_target != null)
            {
                _target.Release();
                _converged.Release();
            }

            // Get a render target for Ray Tracing
            _target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
            _converged = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _converged.enableRandomWrite = true;
            _converged.Create();

            // Reset sampling
            _currentSample = 0;
        }
    }

    public Shader addShader;

    private void Render(RenderTexture destination)
    {
        // Make sure we have a current render target
        InitRenderTexture();

        for (int i = 0; i < 1; ++i)
        {
            if (_currentSample == 0)
                RayTracingShader.SetVector("_PixelOffset", new Vector2(0.5f, 0.5f));
            else
                RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
            RayTracingShader.SetFloat("_Seed", Random.value);
            // Set the target and dispatch the compute shader
            RayTracingShader.SetTexture(0, "Result", _target);
            int threadGroupsX = Mathf.CeilToInt(Screen.width / 16.0f);
            int threadGroupsY = Mathf.CeilToInt(Screen.height / 16.0f);
            RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

            // Blit the result texture to the screen
            if (_addMaterial == null)
            {
                //_addMaterial = new Material(Shader.Find("Hidden/AddShader"));
                _addMaterial = new Material(addShader);
            }

            _addMaterial.SetFloat("_Sample", _currentSample);
            Graphics.Blit(_target, _converged, _addMaterial);
            Graphics.Blit(_converged, destination);
            _currentSample++;
        }

        Vector3[] output = new Vector3[outBuffer.count];
        outBuffer.GetData(output);
        //Debug.Log(output[0].x + " " + output[0].y + " " + output[0].z);
        outBuffer.Dispose();
    }

    public bool fx = false;

    

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (fx)
        {
            SetShaderParameters();
            Render(destination);
        } else 
            Graphics.Blit(source, destination);
    }
}
