using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRayMethod4 : MonoBehaviour
{
    public Transform[] _BlackHoles;
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
        var tex = TextureManager.loadPFMtoTexture("GIG.pfm");
        _textureGravity = TextureManager.textureToFloats(tex);
        _textureWidth = tex.width;
        _textureHeight = tex.height;
        _ray_ = new Ray();
    }

    // Update is called once per frame
    void Update()
    {
        //DateTime before = DateTime.Now;
        var obs = FindObjectsOfType<Mass>();
        List<Transform> list = new List<Transform>();
        foreach (var o in obs)
        {
            list.Add(o.transform);
        }
        _BlackHoles = list.ToArray();

        clearPoints();
        _ray_.origin = this.transform.position;
        _ray_.direction = this.transform.forward;

        //_ray_.direction = new Vector3(_ray_.direction.x, 0, _ray_.direction.z);
        //_ray_.origin = new Vector3(_ray_.origin.x, 0, _ray_.origin.z);

        bool state = calcRay4(_ray_);

        //DateTime after = DateTime.Now;
        //TimeSpan duration = after.Subtract(before);
        //Debug.Log("Duration in milliseconds: " + duration.Milliseconds);
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

        float pix00;
        float pix01;
        float pix10;
        float pix11;

        pix00 = _textureGravity[iy0 * _textureWidth + ix0];
        pix01 = _textureGravity[iy0 * _textureWidth + ix1];
        pix10 = _textureGravity[iy1 * _textureWidth + ix0];
        pix11 = _textureGravity[iy1 * _textureWidth + ix1];

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

    public int maxNewtonIterations;

    bool crossConditionLite(Vector3 startPos, Vector3 axis, float beta, Vector3 center, int bhId)
    {
        Vector3 nextBlackHole = _BlackHoles[bhId].position;
        float gamma = angleAdvanced(startPos - center, nextBlackHole - center, axis);
        if (gamma >= 0 && gamma <= beta)
        {
            //zawiera się
            return true;
        }
        else
        {
            //nie zawiera sie
            return false;
        }
    }
    bool crossConditionLite(Vector3 startPos, Vector3 endPos, Vector3 axis, Vector3 center, int bhId)
    {
        Vector3 nextBlackHole = _BlackHoles[bhId].position;
        float beta = angleAdvanced(startPos - center, endPos - center, axis);
        float gamma = angleAdvanced(startPos - center, nextBlackHole - center, axis);
        if (gamma >= 0 && gamma <= beta)
        {
            //zawiera się
            return true;
        }
        else
        {
            //nie zawiera sie
            return false;
        }
    }
    bool crossConditionAdvanced(Vector3 startPos, Vector3 startDir,  Vector3 endPos, Vector3 endDir, Vector3 axis, int bhId)
    {
        Vector3 nextBlackHole = _BlackHoles[bhId].position;
        Vector3 tmp1 = startPos - nextBlackHole;
        float l1 = length(tmp1) * Vector3.Dot(normalize(-startDir), normalize(tmp1));
        Vector3 tmp2 = endPos - nextBlackHole;
        float l2 = length(tmp2) * Vector3.Dot(normalize(-endDir), normalize(tmp2));

        float epsilon = 0.00001f;

        if (l1 >= -epsilon && l2 <= epsilon)
        {
            //zawiera się
            return true;
        }
        else
        {
            //nie zawiera sie
            return false;
        }
    }

    public PLOT plot;

    void plotCrossVector(Vector3 P0, Vector3 dir0, Vector3 P1, Vector3 dir1, float beta, Vector3 axis, Vector3 nextBlackHole)
    {
        plot.clearPoints();
        Vector3 v2 = P1 - P0;
        Vector3 nextBlackHoleProjection = projectOnPlane(nextBlackHole, axis);

        float _od = -5;
        float _do = 5;

        for (int i = 0; i < 1000; ++i)
        {
            float x = (_do - _od)*i/1000 + _od;

            Vector3 d1 = rodrigez(dir0, axis, beta * x);
            Vector3 point1 = P0 + v2 * x;
            Vector3 tmp1 = nextBlackHoleProjection - point1;
            float _f = Vector3.Dot(normalize(tmp1), d1);  //ma zbiegac do zera
            //float _f = angle(tmp1, d1) - PI / 2.0f;

            plot.putPoint(x*100, _f*100);
        }
    }

    float SolveCrossVectorFast(Vector3 P0, Vector3 dir0, Vector3 P1, Vector3 dir1, float beta, Vector3 axis, Vector3 nextBlackHole)
    {
        plotCrossVector( P0,  dir0,  P1,  dir1,  beta,  axis,  nextBlackHole);
        float x = 0;
        Vector3 v2 = P1 - P0;
        float delta = 0;
        float epsilon = 0.001f;
        float dx = 0.001f;

        //Debug.Log("start");
        Vector3 nextBlackHoleProjection = projectOnPlane(nextBlackHole, axis);
        for (int i = 0; i < maxNewtonIterations; ++i)
        {
            Vector3 d1 = rodrigez(dir0, axis, beta * x);
            Vector3 point1 = P0 + v2 * x;
            Vector3 tmp1 = nextBlackHoleProjection - point1;
            float _f = Vector3.Dot(normalize(tmp1), d1);  //ma zbiegac do zera
            //float _f = angle(tmp1, d1) - PI / 2.0f;
            

            Vector3 d2 = rodrigez(dir0, axis, beta * (x + dx));
            Vector3 point2 = P0 + v2 * (x + dx);
            Vector3 tmp2 = nextBlackHoleProjection - point2;
            float _f2 = Vector3.Dot(normalize(tmp2), d2);  //ma zbiegac do zera
            //float _f2 = angle(tmp2, d2) - PI / 2.0f;

            float _fPrim = (_f2 - _f) / dx;
            float diff = _f / _fPrim;
            diff = Mathf.Clamp(diff, -0.4f, 0.4f);
            float tmp3 = (x - diff);

            delta = Mathf.Abs(tmp3 - x);
            x = tmp3;
            //Debug.Log("X: "+x+"\t delta:" + delta + "\tx: "+x);
            pointer3.transform.position = point2;
            if (delta < epsilon)
            {
                break;
            }
        }
        float x0 = x;
        x = 1;
        for (int i = 0; i < maxNewtonIterations; ++i)
        {
            Vector3 d1 = rodrigez(dir0, axis, beta * x);
            Vector3 point1 = P0 + v2 * x;
            Vector3 tmp1 = nextBlackHoleProjection - point1;
            float _f = Vector3.Dot(normalize(tmp1), d1);  //ma zbiegac do zera
            //float _f = angle(tmp1, d1) - PI / 2.0f;

            Vector3 d2 = rodrigez(dir0, axis, beta * (x + dx));
            Vector3 point2 = P0 + v2 * (x + dx);
            Vector3 tmp2 = nextBlackHoleProjection - point2;
            float _f2 = Vector3.Dot(normalize(tmp2), d2);  //ma zbiegac do zera
            //float _f2 = angle(tmp2, d2) - PI / 2.0f;

            float _fPrim = (_f2 - _f) / dx;
            float diff = _f / _fPrim;
            diff = Mathf.Clamp(diff, -0.4f, 0.4f);
            float tmp3 = (x - diff);

            delta = Mathf.Abs(tmp3 - x);
            x = tmp3;
            //Debug.Log("X: "+x+"\t delta:" + delta + "\tx: "+x);
            pointer3.transform.position = point2;
            if (delta < epsilon)
            {
                break;
            }
        }
        float x1 = x;

        if (x0 == x1) return x0;
        if (x0 >= 0 && x0 <= 1) return x0;
        if (x1 >= 0 && x1 <= 1) return x1;


        //Debug.Log("stop");
        /*if (x >= 0 && x <= 1)
        {
            Debug.Log("TAK");
        } else
        {
            Debug.Log("NIE");
        }*/
        return x;
    }
    float SolveCrossVectorFast0(Vector3 P0, Vector3 dir0, Vector3 P1, Vector3 dir1, float beta, Vector3 axis, Vector3 nextBlackHole)
    {
        plotCrossVector(P0, dir0, P1, dir1, beta, axis, nextBlackHole);
        float x = 0.5f;
        Vector3 v2 = P1 - P0;
        float delta = 0;
        float epsilon = 0.001f;

        //Debug.Log("start");
        Vector3 nextBlackHoleProjection = projectOnPlane(nextBlackHole, axis);

        Vector3 d0 = dir0;
        Vector3 point0 = P0;
        Vector3 tmp0 = nextBlackHoleProjection - point0;
        float _f0 = angle(tmp0, d0) - PI / 2.0f;

        Vector3 d00 = dir1;
        Vector3 point00 = P0 + v2;
        Vector3 tmp00 = nextBlackHoleProjection - point00;
        float _f00 = angle(tmp00, d00) - PI / 2.0f;

        if (_f0*_f00 < 0)
        {
            //rozne znaki
        }

        for (int i = 0; i < maxNewtonIterations; ++i)
        {
            Vector3 d1 = rodrigez(dir0, axis, beta * x);
            Vector3 point1 = P0 + v2 * x;
            Vector3 tmp1 = nextBlackHoleProjection - point1;
            //float _f = Vector3.Dot(normalize(tmp1), d1);  //ma zbiegac do zera
            float _f = angle(tmp1, d1) - PI / 2.0f;
            float dx = 0.00001f;

            Vector3 d2 = rodrigez(dir0, axis, beta * (x + dx));
            Vector3 point2 = P0 + v2 * (x + dx);
            Vector3 tmp2 = nextBlackHoleProjection - point2;
            //float _f2 = Vector3.Dot(normalize(tmp2), d2);  //ma zbiegac do zera
            float _f2 = angle(tmp2, d2) - PI / 2.0f;

            float _fPrim = (_f2 - _f) / dx;
            float tmp3 = x - _f / _fPrim;
            delta = Mathf.Abs(tmp3 - x);
            x = tmp3;
            //Debug.Log("X: "+x+"\t delta:" + delta + "\tx: "+x);
            pointer3.transform.position = point2;
            if (delta < epsilon)
            {
                break;
            }
        }
        //Debug.Log("stop");
        /*if (x >= 0 && x <= 1)
        {
            Debug.Log("TAK");
        } else
        {
            Debug.Log("NIE");
        }*/
        return x;
    }
    void solveFullState(Ray ray, int bhInd, float addLength, out Vector3 rot, out Vector3 v1, out Vector3 v2, out Vector3 v3, out float angle, out Vector3 axis)
    {
        rot = new Vector3(0, 0, 0);
        v1 = new Vector3(0, 0, 0);
        v2 = new Vector3(0, 0, 0);
        v3 = new Vector3(0, 0, 0);

        angle = getBlackHoleAngle(_BlackHoles[bhInd], ray, addLength);
        axis = normalize(getAxis(ray, _BlackHoles[bhInd].position));
        float d = getApproachCollisionDistance(ray, _BlackHoles[bhInd].position);
        v1 = d * ray.direction;
        Vector3 hitBack = v1 + ray.origin;
        Vector3 hitFront = rotatePointAroundPoint(hitBack, _BlackHoles[bhInd].position, axis, angle);
        v2 = hitFront - hitBack;
        v3 = normalize(rodrigez(ray.direction, axis, angle));
        rot = axis * angle;
    }

    int debugCounter = 0;

    void debugPrint()
    {
        Debug.Log("DC: "+debugCounter);
        debugCounter++;
    }

    bool bendVector(Ray ray, float rayLength, bool backMode, out Vector3 v1, out Vector3 v2, out Vector3 v3, out bool shadowHit)
    {
        bool[] forbidden2 = new bool[100];
        for (int i = 0; i < _BlackHoles.Length; ++i)
            forbidden2[i] = false;

        v1 = Vector3.zero;
        v2 = Vector3.zero;
        v3 = ray.direction; shadowHit = false;
        Vector3 rot = Vector3.zero;//caly obrot
        if (backMode)
        {
            for (int i = 0; i < _BlackHoles.Length; ++i)
            {
                Transform center = _BlackHoles[i];
                Vector3 axis = getAxis(ray, center.position);

                //nastepna czarna dziura
                Vector3 tmp_v2 = Vector3.zero;
                float d = getApproachCollisionDistance(ray, center.position);
                if (d < 0)
                {
                    forbidden[i] = true;
                    forbidden2[i] = true;
                    float beta = getBlackHoleAngle(center, ray);
                    rot += axis * beta;

                    if (length(rot) >= 90)
                    {
                        //bh shadow
                        v2 = Vector3.zero;
                        v3 = _BlackHoles[i].position - (ray.origin);
                        shadowHit = true;
                        return false;
                    }

                    v3 = rodrigez(ray.direction, rot, length(rot));

                    Vector3 vecBack = (ray.origin + ray.direction * d) - center.position;
                    Vector3 dir = ray.origin - center.position;
                    if (angleAdvanced(vecBack, dir, axis) <= beta)
                    {
                        //middle state
                        Vector3 rod1 = rodrigez(ray.direction, axis, beta / 2.0f);
                        Vector3 hitBack = planeIntersection(center.position, ray.direction, ray.origin, rod1);
                        Vector3 hitFront = rotatePointAroundPoint(hitBack, center.position, axis, beta);
                        tmp_v2 = hitFront - ray.origin;
                    } else
                    {
                        //back state
                    }
                    v2 += tmp_v2;
                }

                //szukanie stanow posrednich dla V2
                for (int j = 0; j < _BlackHoles.Length; ++j)
                {
                    if (i == j || forbidden[j]) continue;
                    bool cond = crossConditionAdvanced(ray.origin + v1, ray.direction, ray.origin + v1 + v2, v3, axis, j);

                    if (cond)
                    {
                        float x = SolveCrossVectorFast(ray.origin + v1, ray.direction, ray.origin + v1 + v2, v3, length(rot), axis, _BlackHoles[j].position);
                        if (x >= 0 && x <= 1)
                        {
                            //modyfikacja v2
                            //Debug.Log("zaawansowane wyznaczenie sciezki aktywne");
                            forbidden[j] = true;
                            forbidden2[j] = true;
                            Vector3 hitpoint = ray.origin + v1 + v2 * x;

                            pointer3.transform.position = hitpoint;

                            Vector3 dir = rodrigez(ray.direction, axis, length(rot) * x);
                            Vector3 _v1, _v2, _v3, _rot, _axis;
                            float _angle;
                            solveFullState(CreateRay(hitpoint, dir), j, length(hitpoint - ray.origin), out _rot, out _v1, out _v2, out _v3, out _angle, out _axis);

                            if (length(_rot) >= 90)
                            {
                                //bh shadow
                                v2 = hitpoint - (ray.origin + v1);
                                v3 = _BlackHoles[j].position - hitpoint;
                                shadowHit = true;
                                return false;
                            }

                            rot += _rot;
                            v3 = rodrigez(ray.direction, rot, length(rot));
                            v2 += _v2;
                        }
                    }
                }
            }
        } else
        {
            //normalny tryb
            float nearestDist = 9999999;
            int bhid = -1;
            for (int i = 0; i < _BlackHoles.Length; ++i)
            {
                if (forbidden[i]) continue;
                Transform center = _BlackHoles[i];
                float d = getApproachCollisionDistance(ray, center.position);
                if (d >= 0 && d < nearestDist)
                {
                    nearestDist = d;
                    bhid = i;
                }
            }

            if (bhid == -1)
            {
                //STOP promien leci w nieskonczonosc
                return false;
            } else
            {
                {
                    Vector3 _v1, _v2, _v3, _rot, _axis;
                    float _angle;
                    solveFullState(ray, bhid, rayLength, out _rot, out _v1, out _v2, out _v3, out _angle, out _axis);
                    rot += _rot;
                    v3 = rodrigez(ray.direction, rot, length(rot));
                    v2 += _v2;
                    v1 = _v1;
                    forbidden[bhid] = true;
                    //forbidden2[bhid] = true;//delay
                    if (length(_rot) >= 90)
                    {
                        //bh shadow
                        v2 = Vector3.zero;
                        v3 = _BlackHoles[bhid].position - (ray.origin + v1);
                        shadowHit = true;
                        return false;
                    }
                }

                Vector3 axis = getAxis(ray, _BlackHoles[bhid].position);
                //szukanie stanow posrednich dla V2
                bool ifany = false;
                for (int j = 0; j < _BlackHoles.Length && bhid != -1; ++j)
                {
                    if (bhid == j || forbidden[j]) continue;
                    bool cond = crossConditionAdvanced(ray.origin + v1, ray.direction, ray.origin + v1 + v2, v3, normalize(rot), j);
                    //cond = false;
                    if (cond)
                    {
                        ifany = true;
                        float x = SolveCrossVectorFast(ray.origin + v1, ray.direction, ray.origin + v1 + v2, v3, length(rot), normalize(rot), _BlackHoles[j].position);
                        if (x >= 0 && x <= 1)
                        {
                            //modyfikacja v2
                            //Debug.Log("zaawansowane wyznaczenie sciezki aktywne");
                            forbidden[j] = true;
                            forbidden2[j] = true;
                            Vector3 hitpoint = ray.origin + v1 + v2 * x;

                            pointer3.transform.position = hitpoint;

                            Vector3 dir = rodrigez(ray.direction, normalize(rot), length(rot) * x);
                            Vector3 _v1, _v2, _v3, _rot, _axis;
                            float _angle;
                            solveFullState(CreateRay(hitpoint, dir), j, rayLength + length(hitpoint - ray.origin), out _rot, out _v1, out _v2, out _v3, out _angle, out _axis);

                            Vector3 deb = new Vector3(j, rayLength + length(hitpoint - ray.origin), _angle);
                            Debug.Log("O: " + deb.x+" " + deb.y+" "+ deb.z);

                            rot += _rot;
                            v3 = rodrigez(ray.direction, rot, length(rot));
                            v2 += _v2;

                            if (length(_rot) >= 90)
                            {
                                //bh shadow
                                v2 = hitpoint - (ray.origin + v1);
                                v3 = _BlackHoles[j].position - hitpoint;
                                shadowHit = true;
                                return false;
                            }

                            j = -1;
                        }
                    }
                }
                if (!ifany) forbidden2[bhid] = true;

                //for (int k = 0; k < _BlackHoles.Length; ++k)
                //    forbidden[k] = forbidden2[k];

                //rzutowanie rownolegle frontowe
                {
                    /*Vector3 frontHit = ray.origin + v1;
                    for (int i = 0; i < _BlackHoles.Length; ++i)
                    {
                        if (forbidden[i] || i == bhid) continue;

                        Vector3 backHit = frontHit + v2;
                        float fullLength = Vector3.Dot(ray.direction, backHit - frontHit);

                        if (length(_BlackHoles[i].position - frontHit) < length(_BlackHoles[bhid].position - frontHit)) continue;
                        float testLength = Vector3.Dot(ray.direction, _BlackHoles[i].position - frontHit);
                        float ratio = testLength / fullLength;
                        //Debug.Log("ratio: "+ ratio);
                        if (ratio >= 0 && ratio <= 1)
                        {
                            ratio = 1 - ratio;
                            Vector3 _v1, _v2, _v3, _rot, _axis;
                            float _angle;
                            solveFullState(ray, i, rayLength, out _rot, out _v1, out _v2, out _v3, out _angle, out _axis);
                            v2 += _v2 * ratio;
                            rot += _rot * ratio;
                            v3 = rodrigez(ray.direction, rot, length(rot));
                            if (length(_rot) >= 90)
                            {
                                //bh shadow
                                v2 = Vector3.zero;
                                v3 = _BlackHoles[i].position - (ray.origin + v1);
                                shadowHit = true;
                                return false;
                            }
                        }
                    }*/
                }

                //rzutowanie rownolegle wsteczne
                if (false) {
                    Vector3 frontHit = ray.origin + v1;
                    for (int i = 0; i < _BlackHoles.Length; ++i)
                    {
                        if (forbidden[i] || i == bhid) continue;
                        
                        Vector3 backHit = frontHit + v2;
                        float fullLength = -Vector3.Dot(v3, backHit - frontHit);

                        Vector3 tmpAx = normalize(rot);
                        Vector3 projbhid = projectOnPlane(_BlackHoles[bhid].position, tmpAx);
                        Vector3 proji = projectOnPlane(_BlackHoles[i].position, tmpAx);

                        if (length(proji - backHit) < length(projbhid - backHit)) continue;
                        float testLength = Vector3.Dot(v3, proji - backHit);
                        float ratio = testLength / fullLength;
                        //Debug.Log("ratio: "+ ratio);
                        if (ratio >= 0 && ratio <= 1)
                        {
                            ratio = 1 - ratio;
                            Debug.Log("ratio: " + ratio);
                            Vector3 _v1, _v2, _v3, _rot, _axis;
                            float _angle;
                            solveFullState(CreateRay(ray.origin+v1+v2, v3), i, rayLength + length(v1) + length(v2), out _rot, out _v1, out _v2, out _v3, out _angle, out _axis);
                            v2 += _v2 * ratio;
                            rot += _rot * ratio;
                            v3 = rodrigez(ray.direction, rot, length(rot));
                            if (length(_rot) >= 90)
                            {
                                //bh shadow
                                v2 = Vector3.zero;
                                v3 = _BlackHoles[i].position - (ray.origin + v1);
                                shadowHit = true;
                                return false;
                            }
                        }
                    }
                }
            }
        }
        //for (int k = 0; k < _BlackHoles.Length; ++k)
        //    forbidden[k] = forbidden2[k];
        return true;
    }

    bool[] forbidden = new bool[100];

    bool calcRay4(Ray _ray)
    {

        /*for (int i = 0; i < _BlackHoles.Length; ++i)
        {
            float d = getApproachCollisionDistance(_ray, _BlackHoles[i].position);
        }*/

        
        for (int i = 0; i < _BlackHoles.Length; ++i)
            forbidden[i] = false;
        


        clearPoints();
        /*Vector3 rot, v1, v2, v3, axis;
        float beta;*/

        Vector3 v1, v2, v3;
        bool shadowHit;

        int totalBends = 0;

        bendVector(_ray, 0, true, out v1, out v2, out v3, out shadowHit); totalBends++;

        addPoint(_ray.origin);
        addPoint(_ray.origin + v1);
        addPoint(_ray.origin + v1 + v2);
        float len = 0;

        if (shadowHit) return false;

        Vector3 lastv3 = v3;
        for (int i = 0; i < 5; ++i)
        {
            len += length(v1) + length(v2);
            _ray = CreateRay(_ray.origin + v1 + v2, v3);
            bool cond = bendVector(_ray, len, false, out v1, out v2, out v3, out shadowHit); totalBends++;
            if (shadowHit)
            {
                lastv3 = Vector3.zero;
                addPoint(_ray.origin + v1);
                addPoint(_ray.origin + v1 + v2);
                addPoint(_ray.origin + v1 + v2 + v3);
                break;
            }
            if (cond == false) break;
            addPoint(_ray.origin + v1);
            pointer.transform.position = _ray.origin + v1;
            addPoint(_ray.origin + v1 + v2);
            pointer2.transform.position = _ray.origin + v1 + v2;
            lastv3 = v3;
        }

       // Debug.Log("Bends: " + totalBends);

        if (!shadowHit)
            addPoint(_ray.origin + lastv3 * 100);

        //addPoint(lastv3*1000);
        //rozwiazanie problemu polega na poziomym przeskalowaniu rozwiązania z wagą od 0 do 1
        //czarna dziura która powinna zostać obliczona, ale jest zasłonięta, będzie wtedy miała wpływ na rozwiązanie.


        /*solveFullState(_ray, 0, out rot, out v1, out v2, out v3, out beta, out axis);

        SolveCrossVectorFast(_ray.origin + v1, _ray.direction, _ray.origin + v1+v2, v3, beta, axis, _BlackHoles[1].position);
        bool testBool = crossConditionAdvanced(_ray.origin + v1, _ray.direction, _ray.origin + v1 + v2, v3, axis, 1);
        Debug.Log(testBool);

        addPoint(_ray.origin);
        addPoint(_ray.origin+v1);
        addPoint(_ray.origin + v1 + v2);
        addPoint(_ray.origin + v1 + v2+v3*10);*/

        return false;
    }
}
