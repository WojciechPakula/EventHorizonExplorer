using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRayMethod5 : MonoBehaviour
{
    public Mass[] _BlackHoles;
    public LineRenderer lr;

    float[] _textureGravity;
    int _textureWidth;
    int _textureHeight;

    public GameObject pointer;
    public GameObject pointer2;
    public GameObject pointer3;

    Ray _ray_;

    float PI = 3.14159265f;
    void Start()
    {
        var tex = TextureManager.loadPFMtoTexture("TEX_W2.pfm");
        _textureGravity = TextureManager.textureToFloats(tex);
        _textureWidth = tex.width;
        _textureHeight = tex.height;
        _ray_ = new Ray();
    }

    // Update is called once per frame
    void Update()
    {
        clearPoints();
        _ray_.origin = this.transform.position;
        _ray_.direction = this.transform.forward;

        //_ray_.direction = new Vector3(_ray_.direction.x, 0, _ray_.direction.z);
        //_ray_.origin = new Vector3(_ray_.origin.x, 0, _ray_.origin.z);
        bool state = calcRay5(_ray_);

    }

    void addPoint(Vector3 point)
    {
        var pc = lr.positionCount;
        lr.positionCount++;
        lr.SetPosition(pc, point);
    }

    void clearPoints()
    {
        lr.positionCount = 0;
    }

    float length(Vector3 v)
    {
        return v.magnitude;
    }

    Vector3 normalize(Vector3 v)
    {
        return v.normalized;
    }
    Vector3 projectOnPlane(Vector3 v, Vector3 planeNormal)
    {
        return v - planeNormal * Vector3.Dot(v, planeNormal);
    }
    float angle(Vector3 a, Vector3 b)
    {
        return Mathf.Acos(Mathf.Clamp(Vector3.Dot(normalize(a), normalize(b)), -1.0f, 1.0f));
    }
    float angleToPlane(Vector3 dir, Vector3 normal)
    {
        Vector3 pr = projectOnPlane(dir, normal);
        float ang = angle(pr, dir) * 180 / PI;
        if (angle(pr, normal) > 90)
            ang *= -1;
        return ang;
    }
    bool contains(int[] arr, int count, int element)
    {
        for (int i = 0; i < count; ++i)
        {
            if (element == arr[i])
                return true;
        }
        return false;
    }
    Vector3 getAxis(Ray ray, Vector3 center)
    {
        return normalize(Vector3.Cross(ray.direction, center - ray.origin));
    }
    float getApproachCollisionDistance(Ray ray, Vector3 pos)
    {
        Vector3 tmp = pos - ray.origin;
        return length(tmp) * Vector3.Dot(normalize(ray.direction), normalize(tmp));
    }
    Ray CreateRay(Vector3 origin, Vector3 direction)
    {
        return new Ray(origin, direction);
    }
    Vector3 rodrigez(Vector3 v, Vector3 k, float angle)
    {
        if (angle == 0)
            return v;
        if (length(k) == 0)
            return v;
        k = normalize(k);
        Vector3 a0 = v * Mathf.Cos(angle);
        Vector3 a1 = Vector3.Cross(k, v) * Mathf.Sin(angle);
        Vector3 a2 = k * Vector3.Dot(k, v) * (1.0f - Mathf.Cos(angle));
        return a0 + a1 + a2;
    }
    float sqrt(float v)
    {
        return Mathf.Sqrt(v);
    }
    float getTableNearest(float x, float y)
    {
        return _textureGravity[_textureWidth * (int)y + (int)x];
    }
    float getTableLinear(float tx, float ty)
    {
        int ix0 = (int)(tx - 0.5f);
        int iy0 = (int)(ty - 0.5f);
        int ix1 = ix0 + 1;
        int iy1 = iy0 + 1;

        float pix00=0;
        float pix01=0;
        float pix10=0;
        float pix11=0;

        try
        {
            pix00 = _textureGravity[iy0 * _textureWidth + ix0];
            pix01 = _textureGravity[iy0 * _textureWidth + ix1];
            pix10 = _textureGravity[iy1 * _textureWidth + ix0];
            pix11 = _textureGravity[iy1 * _textureWidth + ix1];
        } catch
        {

        }
        

        float tmp0 = pix00 + (pix01 - pix00) * ((tx - (ix0 + 0.5f)) / ((ix1 + 0.5f) - (ix0 + 0.5f)));
        float tmp1 = pix10 + (pix11 - pix10) * ((tx - (ix0 + 0.5f)) / ((ix1 + 0.5f) - (ix0 + 0.5f)));
        float tmp3 = tmp0 + (tmp1 - tmp0) * ((ty - (iy0 + 0.5f)) / ((iy1 + 0.5f) - (iy0 + 0.5f)));
        return tmp3;
    }
    float getTable(float x, float y, int mode)
    {
        float x2 = _textureWidth * x;
        float y2 = _textureHeight * y;

        float ret = -1;

        x2 = Mathf.Clamp(x2, 0, _textureWidth-1);
        y2 = Mathf.Clamp(y2, 0, _textureHeight-1);

        if (x >= 1 || y >= 1)
        {
            //poza tekstura :(

        }
        else
        {
            if (mode == 0)
            {
                //nearest neighbour
                ret = getTableNearest(x2, y2);
            }
            else if (mode == 1)
            {
                //linear interpolation
                ret = getTableLinear(x2, y2);

            }
            else if (mode == 2)
            {
                //2

            }
        }
        return ret;
    }
    Vector3 planeIntersection(Vector3 pointt, Vector3 normal, Vector3 origin, Vector3 direction)
    {
        direction = normalize(direction);
        normal = normalize(normal);
        Vector3 normal2 = -1.0f * normal;

        Vector3 tmp = normal; //wybrac lepszy normal

        float kat = angle(direction, normal);
        if (kat <= PI / 2.0f)
            tmp = normal;
        else
            tmp = normal2;

        float alpha = PI / 2.0f - angle(origin - pointt, tmp);
        float beta = PI / 2.0f - angle(direction, tmp);

        float d = length(pointt - origin);
        float x = -d * Mathf.Sin(alpha) / Mathf.Sin(beta);

        return origin + direction * x;
    }
    float getBlackHoleAngle(Transform bc, Ray ray, float addLength = 0)//zrobic!!!! ignoruje promien
    {
        float beta = 0;

        ray.origin = ray.origin - ray.direction * addLength;

        Vector3 vr = bc.position - ray.origin;
        float alpha = angle(ray.direction, vr);
        float r = length(vr);

        float _x_r = sqrt(sqrt(r)) / 4.0f;
        float _y_alpha = alpha / PI;

        float tableOutput = 0;
        tableOutput = getTable(_x_r, _y_alpha, 1);

        if (tableOutput >= 6.0f)
            return 100;

        beta = tableOutput * tableOutput * tableOutput * tableOutput * PI / 180.0f;//w radianach

        return beta;
    }
    float getBlackHoleAngle(Mass bc, Ray ray, float addLength = 0)//zrobic!!!! ignoruje promien
    {
        float unit = bc.rs/2.0f;

        float beta = 0;

        ray.origin = ray.origin - ray.direction * addLength;

        Vector3 vr = bc.transform.position - ray.origin;
        float alpha = angle(ray.direction, vr);
        float r = length(vr) * unit;

        float _x_r = sqrt(sqrt(r)) / 4.0f;
        float _y_alpha = alpha / PI;

        float tableOutput = 0;
        tableOutput = getTable(_x_r, _y_alpha, 1);

        if (tableOutput >= 6.0f)
            return 100;

        beta = tableOutput * tableOutput * tableOutput * tableOutput * PI / 180.0f;//w radianach

        return beta;
    }
    float getBlackHoleAngle2(Mass bc, Ray ray, float addLength = 0)//zrobic!!!! ignoruje promien
    {
        float unit = bc.rs / 2.0f;

        float beta = 0;

        ray.origin = ray.origin - ray.direction * addLength;

        Vector3 vr = bc.transform.position - ray.origin;
        float alpha = angle(ray.direction, vr);
        float r = length(vr) / unit;

        float _x_r = sqrt(sqrt(r)) / 4.0f;
        float _y_alpha = alpha / PI;

        float tableOutput = 0;
        tableOutput = getTable(_x_r, _y_alpha, 1);

        beta = tableOutput * PI / 180.0f;//w radianach

        return beta;
    }
    Vector3 rotatePointAroundPoint(Vector3 p1, Vector3 c, Vector3 axis, float angle)
    {
        return rodrigez(p1 - c, axis, angle) + c;
    }
    float angleAdvanced(Vector3 a, Vector3 b, Vector3 axis)
    {
        a = normalize(projectOnPlane(a, axis));
        b = normalize(projectOnPlane(b, axis));

        float ang = angle(a, b);
        Vector3 cr = normalize(Vector3.Cross(a, b));

        if (length(cr - axis) < 0.001)
        {
            return ang;
        }
        else
        {
            return 2 * PI - ang;
        }
    }



    

    bool[] forbidden = new bool[100];

    Vector3 getBlackHoleAngleV(Mass bh, Ray ray)
    {
        float a = getBlackHoleAngle(bh, ray);
        Vector3 axis = getAxis(ray, bh.transform.position);

        return axis * a;
    }
    Vector3 getBlackHoleAngleV2(Mass bh, Ray ray, out bool shadowHit)
    {
        shadowHit = false;
        float a = getBlackHoleAngle2(bh, ray);
        Vector3 axis = getAxis(ray, bh.transform.position);
        if (a < 0) shadowHit = true;
        return axis * a;
    }
    Vector3 getBlackHoleAngleVPrim(Mass bh, Ray ray, float step, out bool shadowHit)
    {
        shadowHit = false;
        //ray = CreateRay(ray.origin, -ray.direction);

        Ray tmp0 = ray;
        Ray tmp1 = CreateRay(ray.origin, -ray.direction);

        Vector3 a0 = getBlackHoleAngleV(bh, tmp0);
        Vector3 a1 = getBlackHoleAngleV(bh, tmp1);

        if (length(a1)>length(a0))
        {
            Vector3 ret0 = a0;
            Vector3 ret1 = getBlackHoleAngleV(bh, CreateRay(tmp0.origin + tmp0.direction * step, tmp0.direction));
            if (length(ret1) >= 90)
            {
                shadowHit = true;
            }
            return (ret0 - ret1);
        } else
        {
            Vector3 ret1 = a1;
            Vector3 ret0 = getBlackHoleAngleV(bh, CreateRay(tmp1.origin - tmp1.direction * step, tmp1.direction));
            if (length(ret1) >= 90)
            {
                shadowHit = true;
            }
            return (ret0 - ret1);
        }
    }

    

    Vector3 getBlackHoleAngleVPrim2(Mass bh, Ray ray, float step, out bool shadowHit, out float step2)
    {
        int n = _BlackHoles.Length;
        Vector3 p = ray.origin;
        float minDist = 99999999;
        float units = 0;
        for (int j = 0; j < n; ++j)
        {
            Mass center = _BlackHoles[j];

            float dist = length(center.transform.position - p);
            dist = dist / center.rs / 2.0f;
            if (dist < minDist)
            {
                minDist = dist;
                units = length(center.transform.position - p);
            }
        }

        step = (units / minDist);

        if (minDist < 3)
        {
            //step = units - (20 * units / minDist);
            step = (units / minDist)*0.5f;
        }
        if (minDist > 20)
        {
            //step = units - (20 * units / minDist);
            step = (units / minDist)*2;
        }
        if (minDist > 50)
        {
            //step = units / 5.0f;
            step = (units / minDist) * 20;
        }
        if (minDist > 100)
        {
            //step = units / 5.0f;
            step = (units / minDist) * 40;
        }


        step2 = step;
        Vector3 dadt0 = getBlackHoleAngleV2(bh, ray, out shadowHit);
        if (shadowHit) return Vector3.zero;

        float dt = step;
        //float epsilon = 0.4f;
        //dt = epsilon / length(dadt0);

        //if (dt < step) dt = step;
        //if (dt > step*3) dt = step*3;
        dt = step;


        Vector3 rot0 = dadt0 * dt;

        //Vector3 dadt1 = getBlackHoleAngleV2(bh, CreateRay(ray.origin + ray.direction * dt, ray.direction), out shadowHit);
        //if (shadowHit) return Vector3.zero;
        //Vector3 rot1 = dadt1 * dt;
        Vector3 rot = rot0;
        return rot;
        /*float l = 0;
        {
            step2 = step;
            Vector3 dadt0 = getBlackHoleAngleV2(bh, ray, out shadowHit);
            if (shadowHit) return Vector3.zero;
            float dt = step;
            Vector3 rot0 = dadt0 * dt;

            Vector3 dadt1 = getBlackHoleAngleV2(bh, CreateRay(ray.origin + ray.direction * step, ray.direction), out shadowHit);
            if (shadowHit) return Vector3.zero;
            Vector3 rot1 = dadt1 * dt;
            Vector3 rot = (rot0 + rot1) * 0.5f;

            l = length(rot);
        }
        step2 = Mathf.Sqrt(step / l);

        {
            Vector3 dadt0 = getBlackHoleAngleV2(bh, ray, out shadowHit);
            if (shadowHit) return Vector3.zero;
            float dt = step2;
            Vector3 rot0 = dadt0 * dt;

            Vector3 dadt1 = getBlackHoleAngleV2(bh, CreateRay(ray.origin + ray.direction * step2, ray.direction), out shadowHit);
            if (shadowHit) return Vector3.zero;
            Vector3 rot1 = dadt1 * dt;
            Vector3 rot = (rot0 + rot1) * 0.5f;
            return rot;
        }*/


    }

    public float step;
    public int steps;

    bool calcRay5(Ray _ray)
    {
        clearPoints();

        int n = _BlackHoles.Length;
        

        addPoint(_ray.origin);
        Vector3 prevVw = Vector3.zero;

        float tmpStep = step;
        float mul = 1;
        for (int i = 0; i < steps; ++i)
        {
            Vector3 Vw = Vector3.zero;
            for (int j = 0; j < n; ++j)
            {
                bool shadowHit;
                Vector3 prim = getBlackHoleAngleVPrim2(_BlackHoles[j], _ray, step, out shadowHit, out mul);
                Vw += prim;
                if (shadowHit) {
                    float d = getApproachCollisionDistance(_ray, _BlackHoles[j].transform.position);
                    if (d > 0)
                    {
                        addPoint(_ray.origin + _ray.direction * d);
                        addPoint(_BlackHoles[j].transform.position);
                    }
                    return true;
                }
            }
            if (i == 0) prevVw = Vw;
            Vector3 bend = (Vw + prevVw) / 2.0f;
            bend = Vw;

            Vector3 dir2 = rodrigez(_ray.direction, bend, length(bend));
            _ray = CreateRay(_ray.origin + dir2 * mul, dir2);
            addPoint(_ray.origin);

            prevVw = Vw;
        }

        

        

        return false;
    }
}
