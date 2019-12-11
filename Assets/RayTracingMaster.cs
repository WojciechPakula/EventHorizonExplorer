using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RayTracingMaster : MonoBehaviour
{
    public ComputeShader RayTracingShader;
    public Texture SkyboxTexture;
    public Light DirectionalLight;

    [Header("Spheres")]
    public int SphereSeed;
    public Vector2 SphereRadius = new Vector2(3.0f, 8.0f);
    public uint SpheresMax = 100;
    public float SpherePlacementRadius = 100.0f;

    private Camera _camera;
    private float _lastFieldOfView;
    private RenderTexture _target;
    private RenderTexture _converged;
    private Material _addMaterial;
    private uint _currentSample = 0;
    private ComputeBuffer _sphereBuffer;
    private ComputeBuffer _blackHoleBuffer;
    private ComputeBuffer _floatArr2;
    //private ComputeBuffer _floatArrSt;
    private List<Transform> _transformsToWatch = new List<Transform>();

    private ComputeBuffer _vertexBuffer;
    private ComputeBuffer _triangleBuffer;
    private ComputeBuffer _normalsBuffer;
    private ComputeBuffer _uvBuffer;

    struct Sphere
    {
        public Vector3 position;
        public float radius;
        public Vector3 albedo;
        public Vector3 specular;
        public float smoothness;
        public Vector3 emission;
    }

    struct BlackHole
    {
        public Vector3 position;
        public float radius;
        public Vector3 albedo;
        public Vector3 specular;
        public float smoothness;
        public Vector3 emission;
    }

    

    /*TESTY*/
    private void Start()
    {
        test();
    }
    //ComputeBuffer _V;
    void test()
    {
        /*var stat = RayTracingMeshRenderer.getStaticMeshes();//
        _V = new ComputeBuffer(stat[0].V.Count, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector3)));
        _V.SetData(stat[0].V.ToArray());
        RayTracingShader.SetBuffer(0, "_test_V", _V);*/
        RayTracingMeshRenderer.pushStaticMeshes(RayTracingShader);
        RayTracingMeshRenderer.pushTextures(RayTracingShader);
    }
    /*TESTY*/

    private void Awake()
    {
        int w = 10000;
        int h = 10000;
        try
        {
            _camera = GetComponent<Camera>();

            _transformsToWatch.Add(transform);
            _transformsToWatch.Add(DirectionalLight.transform);

            RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);

            //var tex = TextureManager.loadPFMtoTexture("Wypelnienie2.pfm"); 
            //var tex = TextureManager.loadPFMtoTexture("PFM_preview_wypelnienie_R_GIG.pfm");//SIM_checkpoint_ULTRA.pfm
            var tex = TextureManager.loadPFMtoTexture("FAST_SIM_ULTRA.pfm");
            //RayTracingShader.SetTexture(0, "_FloatTexture", tex);
            var floatArr = TextureManager.textureToFloats(tex);

            //RayTracingShader.SetFloats("_floatArr", floatArr);
            _floatArr2 = new ComputeBuffer(w * h, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float)));
            _floatArr2.SetData(floatArr);
            
            RayTracingShader.SetBuffer(0, "_textureGravity", _floatArr2);
            RayTracingShader.SetInt("_textureWidth", w);
            RayTracingShader.SetInt("_textureHeight", h);
        } catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void OnEnable()
    {
        _currentSample = 0;
        SetUpScene();
    }

    private void OnDisable()
    {
        if (_sphereBuffer != null)
            _sphereBuffer.Release();
        if (_blackHoleBuffer != null)
            _blackHoleBuffer.Release();
    }



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            ScreenCapture.CaptureScreenshot(Time.time + "-" + _currentSample + ".png");
        }

        if (_camera.fieldOfView != _lastFieldOfView)
        {
            _currentSample = 0;
            _lastFieldOfView = _camera.fieldOfView;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            RayTracingShader.SetInt("_interpolationMode", 0);
            _currentSample = 0;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RayTracingShader.SetInt("_interpolationMode", 1);
            _currentSample = 0;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            RayTracingShader.SetInt("_interpolationMode", 2);
            _currentSample = 0;
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            RayTracingShader.SetInt("_interpolationMode", 3);
            _currentSample = 0;
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            RayTracingShader.SetInt("_axisMode", 0);
            _currentSample = 0;
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            RayTracingShader.SetInt("_axisMode", 1);
            _currentSample = 0;
        }
        foreach (Transform t in _transformsToWatch)
        {
            if (t.hasChanged)
            {
                _currentSample = 0;
                t.hasChanged = false;
            }
        }
    }

    private void submitDropdown()
    {
        
    }

    private void SetUpScene()
    {
        Random.InitState(SphereSeed);
        List<Sphere> spheres = new List<Sphere>();
        List<BlackHole> blackHoles = new List<BlackHole>();

        // Add a number of random spheres
        for (int i = 0; i < SpheresMax; i++)
        {
            Sphere sphere = new Sphere();

            // Radius and radius
            sphere.radius = SphereRadius.x + Random.value * (SphereRadius.y - SphereRadius.x);
            sphere.radius = 0.001f;
            Vector2 randomPos = Random.insideUnitCircle * SpherePlacementRadius;
            sphere.position = new Vector3(randomPos.x, sphere.radius, randomPos.y);
            sphere.position = new Vector3(2000, 0, 0);

            // Reject spheres that are intersecting others
            foreach (Sphere other in spheres)
            {
                float minDist = sphere.radius + other.radius;
                if (Vector3.SqrMagnitude(sphere.position - other.position) < minDist * minDist)
                    goto SkipSphere;
            }

            // Albedo and specular color
            Color color = Random.ColorHSV();
            float chance = Random.value;
            if (chance < 0.99998f)
            {
                bool metal = chance < 0.999994f;
                sphere.albedo = metal ? Vector4.zero : new Vector4(color.r, color.g, color.b);
                sphere.specular = metal ? new Vector4(color.r, color.g, color.b) : new Vector4(0.04f, 0.04f, 0.04f);
                sphere.specular = new Vector4(1.0f, 1.0f, 1.0f);
                //sphere.smoothness = Random.value;
                sphere.smoothness = 1.0f;
            }
            else
            {
                Color emission = Random.ColorHSV(0, 1, 0, 1, 3.0f, 8.0f);
                sphere.emission = new Vector3(emission.r, emission.g, emission.b);
            }

            // Add the sphere to the list
            spheres.Add(sphere);

            SkipSphere:
            continue;
        }

        BlackHole bh = new BlackHole();
        bh.radius = 3;
        bh.position = new Vector3(0,0,0);
        bh.specular = new Vector4(1.0f, 0.5f, 1.0f);
        bh.albedo = Vector4.zero;
        bh.emission = new Vector3(0, 0, 0);
        bh.smoothness = 1.0f;

        blackHoles.Add(bh);

        // Assign to compute buffer
        if (_sphereBuffer != null)
            _sphereBuffer.Release();
        if (_blackHoleBuffer != null)
            _blackHoleBuffer.Release();
        if (spheres.Count > 0)
        {
            int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Sphere));
            _sphereBuffer = new ComputeBuffer(spheres.Count, size);
            _sphereBuffer.SetData(spheres);
        }
        if (blackHoles.Count > 0)
        {
            int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(BlackHole));
            _blackHoleBuffer = new ComputeBuffer(blackHoles.Count, size);
            _blackHoleBuffer.SetData(blackHoles);
        }
    }

    /*private void setupMeshBuffer()
    {
        MeshRenderer[] allObjects = UnityEngine.Object.FindObjectsOfType<MeshRenderer>();


        foreach (var meshR in allObjects)
        {
            Mesh mesh = meshR.gameObject.GetComponent<MeshFilter>().mesh;
            List<Material> materials = new List<Material>();
            meshR.GetMaterials(materials);

            List<Vector3> v = new List<Vector3>();
            List<Vector3> n = new List<Vector3>();
            List<int> t;
            List<Vector2> uv0 = new List<Vector2>();

            mesh.GetVertices(v);
            mesh.GetNormals(n);
            mesh.GetUVs(0, uv0);
            t = new List<int>(mesh.triangles);


        }

        //_vertexBuffer = new ComputeBuffer(blackHoles.Count, size);
        //_triangleBuffer = new ComputeBuffer(blackHoles.Count, size);
        //_normalsBuffer = new ComputeBuffer(blackHoles.Count, size);
        //_uvBuffer = new ComputeBuffer(blackHoles.Count, size);
    }*/

    private void SetShaderParameters()
    {
        //RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
        RayTracingShader.SetFloat("_Seed", Random.value);

        Vector3 l = DirectionalLight.transform.forward;
        RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));

        if (_sphereBuffer != null)
            RayTracingShader.SetBuffer(0, "_Spheres", _sphereBuffer);
        if (_blackHoleBuffer != null)
            RayTracingShader.SetBuffer(0, "_BlackHoles", _blackHoleBuffer);

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

        // Set the target and dispatch the compute shader
        RayTracingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f); 
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

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        Render(destination);
    }
}
/*float3 rodrigez(float3 v, float3 k, float angle)
{
    k = normalize(k);
    float3 a0 = v * cos(angle);
    float3 a1 = cross(k, v) * sin(angle);
    float3 a2 = k * dot(k, v) * (1.0f - cos(angle));
    return a0 + a1 + a2;
}*/
/*struct BlackHole
{
    float3 position;
    float m;
};

StructuredBuffer<BlackHole> _BlackHoles;*/
/*void IntersectBlackHole(Ray ray, inout RayHit bestHit, BlackHole blackHole)
{
    // Calculate distance along the ray where the sphere is intersected
    float m = blackHole.m;
    
    float3 d = -(ray.origin - blackHole.position);
    float dsc = d.x * d.x + d.y * d.y + d.z * d.z;
    dsc = sqrt(dsc);    

    float3 nd = normalize(d);
    float3 no = normalize(ray.direction);

    float wsp = dot(nd, no);

    float b = dsc * wsp;

    float t = b;

    float3 bvec = no * b;

    float3 hitPoint = ray.origin + bvec;

    float3 k = cross(d, ray.direction);

    float3 a = (b - d);

    float a_l = length(a)-3*m;

    float angle = 4.0f * m / a_l;

    angle = angle / 2.0f + PI / 2.0f;

    float3 rod_out = rodrigez(ray.direction, k, angle);
    rod_out = normalize(rod_out);

    if (angle < 0)
    {
        bestHit.distance = 1.#INF;
        bestHit.position = ray.origin;
        bestHit.normal = 0.0f;
    }

    if (t > 0 && t < bestHit.distance)
    {
        bestHit.distance = t;
        bestHit.position = ray.origin + t * ray.direction;
        bestHit.normal = rod_out;
    }


}
*/
