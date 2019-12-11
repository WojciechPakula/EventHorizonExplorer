using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRayMethod2 : MonoBehaviour
{
    public Transform[] _BlackHoles;

    public LineRenderer lr;

    float[] _textureGravity;
    int _textureWidth;
    int _textureHeight;

    public GameObject pointer;
    public GameObject pointer2;
    //public float publicFloat;
    Ray _ray_;

    float PI = 3.14159265f;

    // Start is called before the first frame update
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
        clearPoints();
        _ray_.origin = this.transform.position;
        _ray_.direction = this.transform.forward;
        bool state = calcRay3(_ray_);

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
    float getBlackHoleAngle(Transform bc, Ray ray)//zrobic!!!! ignoruje promien
    {
        float beta = 0;
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
    bool calcRay3(Ray _ray)
    {
        clearPoints();
        addPoint(_ray.origin);

        Vector3 v1, v2, v3;
        int[] forbidden = new int[100];
        int forbiddenC = 0;
        bool shadowHit = false;
        startPosition(_ray, out v1, out v2, out v3, out forbidden, out forbiddenC, out shadowHit);
        //v1 = new Vector3(0, 0, 0); //debug
        //v2 = new Vector3(0, 0, 0);
        //v3 = _ray.direction;

        addPoint(_ray.origin + v1);
        addPoint(_ray.origin + v1 + v2);
        if (shadowHit)
        {
            return shadowHit;
        }
        float raylength = 0;
        for (int i = 0; i < 5; ++i)
        {
            raylength += length(v1) + length(v2);
            _ray.direction = v3;
            _ray.origin = _ray.origin + v1 + v2;
            int[] forbidden2 = new int[100];
            int forbidden2C = 0;
            bool state = nextPosition(_ray, raylength, forbidden, forbiddenC,out v1, out v2, out v3, out forbidden2, out forbidden2C, out shadowHit);
            if (shadowHit)
            {
                addPoint(_ray.origin + v1);
                addPoint(_ray.origin + v1 + v2);
                break;
            }
            if (!state)
            {
                addPoint(_ray.origin + v1 + v2 + v3 * 100);
                break;
            }
            //forbidden.AddRange(forbidden2);
            //forbidden = forbidden2;
            //forbiddenC = forbidden2C;

            for (int k = 0; k < forbidden2C; ++k)
            {
                forbidden[forbiddenC] = forbidden2[k];
                forbiddenC++;
            }

            addPoint(_ray.origin + v1);
            addPoint(_ray.origin + v1 + v2);
        }
        //Debug.Log(angleToPlane(v3, Vector3.up));
        addPoint(v3); //debug
                      /*if (angleToPlane(v3, Vector3(0, 1, 0)) < 2.0f)
                      {
                          return true;
                      }*/
        Debug.Log(angleToPlane(v3, new Vector3(0, 1, 0)));
        return shadowHit;
    }
    Vector3 rotatePointAroundPoint(Vector3 p1, Vector3 c, Vector3 axis, float angle)
    {
        return rodrigez(p1 - c, axis, angle) + c;
    }
    void solveFullState(Ray ray, int bhInd, out Vector3 rot, out Vector3 v1, out Vector3 v2, out Vector3 v3)
    {
        rot = new Vector3(0, 0, 0);
        v1 = new Vector3(0, 0, 0);
        v2 = new Vector3(0, 0, 0);
        v3 = new Vector3(0, 0, 0);

        float angle = getBlackHoleAngle(_BlackHoles[bhInd], ray);
        Vector3 axis = normalize(getAxis(ray, _BlackHoles[bhInd].position));
        float d = getApproachCollisionDistance(ray, _BlackHoles[bhInd].position);
        v1 = d * ray.direction;
        Vector3 hitBack = v1 + ray.origin;
        Vector3 hitFront = rotatePointAroundPoint(hitBack, _BlackHoles[bhInd].position, axis, angle);
        v2 = hitFront - hitBack;
        v3 = normalize(rodrigez(ray.direction, axis, angle));
        rot = axis * angle;
    }
    bool solveCrossVector(Ray ray, Vector3 rot, Vector3 v1, Vector3 v2, Vector3 v3, int[] forbidden, int forbiddenC, out Ray _ray2, out Vector3 _rot, out Vector3 _v1, out Vector3 _v2, out Vector3 _v3, out int choosenBH)
    {
        Vector3 axis = normalize(rot); //???????
        float beta = length(rot);
        Vector3 startDir = ray.direction;
        Vector3 endDir = normalize(v3);
        Vector3 startPos = ray.origin + v1;
        Vector3 endPos = ray.origin + v1 + v2;
        Vector3 diff = endPos - startPos;

        _v1 =new Vector3(0, 0, 0);
        _v2 = new Vector3(0, 0, 0);
        _v3 = new Vector3(0, 0, 0);
        _rot = new Vector3(0, 0, 0);
        _ray2 = CreateRay(new Vector3(0, 0, 0),new Vector3(0, 0, 0));

        bool isCross = false;
        float outDMin = 1000;
        Vector3 choosenDir = new Vector3(0, 0, 0);
        Vector3 choosenPos = new Vector3(0, 0, 0);
        choosenBH = -1;

        uint c = (uint)_BlackHoles.Length;

        for (int i = 0; i < (int)c; ++i)
        {
            if (contains(forbidden, forbiddenC, i))
                continue;
            Vector3 nextBlackHole = _BlackHoles[i].position;

            /*Vector3 t0 = startPos - nextBlackHole;
            Vector3 t1 = endPos - nextBlackHole;

            Vector3 axis_advanced = Vector3.Cross(t0, t1).normalized;*/

            Vector3 pos, dir;
            float outD;

            //metodaNewtona
            float epsilon = 0.08f;
            float x = 0.5f;
            float delta = 0;
            pos = new Vector3(0, 0, 0);
            dir = new Vector3(0, 0, 0);
            outD = 0;
            for (int j = 0; j < 10; ++j)
            {
                Vector3 d = rodrigez(startDir, axis, beta * x);
                Vector3 point1 = startPos + normalize(diff) * length(diff) * x;
                Vector3 tmp = point1 - nextBlackHole;
                float ang = angle(d, normalize(tmp));
                float _f = Mathf.Abs(Mathf.Cos(ang) * length(tmp));

                float dx = 0.00001f;

                Vector3 d2 = rodrigez(startDir, axis, beta * (x + dx));
                Vector3 point2 = startPos + normalize(diff) * length(diff) * (x + dx);
                Vector3 tmp2 = point2 - nextBlackHole;
                float ang2 = angle(d2, normalize(tmp2));
                float _f2 = Mathf.Abs(Mathf.Cos(ang2) * length(tmp2));

                float _fPrim = (_f2 - _f) / dx;
                float tmp3 = x - _f / _fPrim;
                delta = Mathf.Abs(tmp3 - x);
                x = tmp3;
                if (delta < epsilon)
                {
                    break;
                }
            }
            if (delta < epsilon && x >= 0 && x <= 1)
            {
                //sukces
                outD = x;

                //pos = startPos + diff.normalized * diff.magnitude * x;
                //dir = getInterpolationDirection(startDir, axis, beta, x).normalized;

                //isSuccess = true;

                isCross = true;
                if (outD <= outDMin)
                {
                    outDMin = outD;
                    choosenDir = normalize(rodrigez(startDir, axis, beta * x));
                    choosenPos = startPos + normalize(diff) * length(diff) * outD;
                    choosenBH = i;
                }
            }
        }
        if (isCross)
        {
            //ustaw wyjscie
            _ray2 = CreateRay(choosenPos - choosenDir * (length(v1) + length(diff) * outDMin), choosenDir);
            float beta2 = getBlackHoleAngle(_BlackHoles[choosenBH], _ray2);
            Vector3 axis2 = normalize(Vector3.Cross(choosenDir, _BlackHoles[choosenBH].position - choosenPos));
            _rot = axis2 * beta2;

            _v1 = (choosenPos - _ray2.origin);
            _v2 = rotatePointAroundPoint(choosenPos, _BlackHoles[choosenBH].position, axis2, beta2) - choosenPos;
            _v3 = normalize(rodrigez(choosenDir, axis2, beta));
        }
        return isCross;
    }
    bool solveCrossVector2(Ray ray, Vector3 rot, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 center, int[] forbidden, int forbiddenC, out Ray _ray2, out Vector3 _rot, out Vector3 _v1, out Vector3 _v2, out Vector3 _v3, out int choosenBH)
    {
        Vector3 axis = normalize(rot);
        float beta = length(rot);
        Vector3 startDir = ray.direction;
        Vector3 endDir = normalize(v3);
        Vector3 startPos = ray.origin + v1;
        Vector3 endPos = ray.origin + v1 + v2;
        Vector3 diff = endPos - startPos;

        _v1 = new Vector3(0, 0, 0);
        _v2 = new Vector3(0, 0, 0);
        _v3 = new Vector3(0, 0, 0);
        _rot = new Vector3(0, 0, 0);
        _ray2 = CreateRay(new Vector3(0, 0, 0), new Vector3(0, 0, 0));

        bool iscross = false;
        float outDMin = 1000;
        Vector3 choosenDir = new Vector3(0, 0, 0);
        Vector3 choosenPos = new Vector3(0, 0, 0);
        choosenBH = -1;

        int c = _BlackHoles.Length;

        for (int i = 0; i < c; ++i)
        {
            if (contains(forbidden, forbiddenC, i))
                continue;

            Vector3 nextBlackHole = _BlackHoles[i].position;

            Vector3 projection = projectOnPlane(nextBlackHole, axis);

            float gamma = angleAdvanced(startPos - center, projection - center, axis);
            float outD = 0;
            if (gamma >= 0 && gamma <= beta)
            {
                //zawiera się
                outD = gamma / beta;

                iscross = true;
                if (outD <= outDMin)
                {
                    outDMin = outD;
                    choosenDir = normalize(rodrigez(startDir, axis, beta * outD));
                    choosenPos = startPos + diff * outD;
                    choosenBH = i;
                }
            }
            else
            {
                //nie zawiera sie
            }
        }
        if (iscross)
        {
            //ustaw wyjscie
            _ray2 = CreateRay(choosenPos - choosenDir * (length(v1) + length(diff) * outDMin), choosenDir);
            float beta2 = getBlackHoleAngle(_BlackHoles[choosenBH], _ray2);
            Vector3 axis2 = normalize(Vector3.Cross(choosenDir, _BlackHoles[choosenBH].position - choosenPos));
            _rot = axis2 * beta2;

            _v1 = (choosenPos - _ray2.origin);
            _v2 = rotatePointAroundPoint(choosenPos, _BlackHoles[choosenBH].position, axis2, beta2) - choosenPos;
            _v3 = normalize(rodrigez(choosenDir, axis2, beta));
        }
        return iscross;
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
    void startPosition(Ray ray, out Vector3 v1, out Vector3 v2, out Vector3 v3, out int[] forbidden, out int forbiddenC, out bool shadowHit)
    {

        v1 = new Vector3(0, 0, 0);
        v2 = new Vector3(0, 0, 0);
        v3 = new Vector3(0, 0, 0);

        forbiddenC = 0;
        forbidden = new int[100];
        int front = -1;
        float minDist = 99999999;
        shadowHit = false;
        Vector3 totalRotVector = new Vector3(0, 0, 0);
        Vector3 halfRotVector = new Vector3(0, 0, 0);
        

        uint c = (uint)_BlackHoles.Length;

        for (int i = 0; i < c; ++i)
        {
            if (contains(forbidden, forbiddenC, (int)i))
                continue;
            Transform center = _BlackHoles[i];
            Vector3 axis = (getAxis(ray, center.position));
            float d = getApproachCollisionDistance(ray, center.position);
            if (d <= 0)
            {
                //za lub posrednio
                float beta = getBlackHoleAngle(center, ray);
                if (beta >= 90)
                {
                    //bh shadow
                    v2 = center.position - ray.origin;
                    shadowHit = true;
                    return;
                }
                beta = beta % (2 * PI);
                Vector3 vecBack = (ray.origin + ray.direction * d) - center.position;
                Vector3 dir = ray.origin - center.position;
                if (angleAdvanced(vecBack, dir, axis) > beta)
                {
                    //za
                    totalRotVector += axis * beta;
                    halfRotVector += axis * beta / 2.0f;
                    forbidden[forbiddenC] = i;
                    forbiddenC++;
                }
            }
        }
        Vector3 v2out = new Vector3(0, 0, 0);
        ray.direction = rodrigez(ray.direction, normalize(halfRotVector), length(halfRotVector) * 2);
        halfRotVector = new Vector3(0,0,0);
        Vector3 v3out = ray.direction;
        for (int i2 = 0; i2 < c; ++i2)
        {
            if (contains(forbidden, forbiddenC, (int)i2))
                continue;
            Transform center = _BlackHoles[i2];
            Vector3 axis = normalize(getAxis(ray, center.position));
            float d = getApproachCollisionDistance(ray, center.position);
            if (d > 0)
            {

            }
            else
            {
                float beta = getBlackHoleAngle(center, ray);
                Vector3 vecBack = (ray.origin + ray.direction * d) - center.position;
                Vector3 dir = ray.origin - center.position;
                if (angleAdvanced(vecBack, dir, axis) <= beta)
                {
                    forbidden[forbiddenC] = i2;
                    forbiddenC++;
                    //stan posredni
                    Vector3 rot;
                    Vector3 mv1;
                    Vector3 mv2;
                    Vector3 mv3;
                    solveMiddleState(ray, i2, out rot, out mv1, out mv2, out mv3);
                    v2out += mv2;
                    halfRotVector += rot / 2.0f;

                    if (beta >= 90)
                    {
                        //bh shadow
                        v1 = ray.direction * d;
                        v2 = center.position - ray.origin - v1;
                        shadowHit = true;
                        return;
                    }

                    for (uint _i = 0; _i < 10; ++_i)
                    {
                        Ray ray2;
                        Vector3 ov1, ov2, ov3, orot;
                        int bhInd;
                        bool state = solveCrossVector2(ray, rot, mv1, mv2, mv3, center.position, forbidden, forbiddenC, out ray2, out orot, out ov1, out ov2, out ov3, out bhInd);

                        if (state)
                        {
                            if (length(orot) >= 90)
                            {
                                //bh shadow
                                v1 = ray.direction * d;
                                v2 = _BlackHoles[bhInd].position - ray.origin - v1;
                                shadowHit = true;
                                return;
                            }
                            v2out += ov2;
                            halfRotVector += orot / 2.0f;
                            forbidden[forbiddenC] = bhInd;
                            forbiddenC++;
                        }
                        else
                            break;
                    }
                } else
                {
                    halfRotVector += axis * beta / 2.0f;
                    forbidden[forbiddenC] = i2;
                    forbiddenC++;
                }
            }
        }
        v2 = v2out;
        v3 = rodrigez(v3out, normalize(halfRotVector), length(halfRotVector) * 2);
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
    void solveMiddleState(Ray ray, int bhInd, out Vector3 rot, out Vector3 v1, out Vector3 v2, out Vector3 v3)
    {
        rot = new Vector3(0, 0, 0);
        v1 = new Vector3(0, 0, 0);
        v2 = new Vector3(0, 0, 0);
        v3 = new Vector3(0, 0, 0);

        float angle = getBlackHoleAngle(_BlackHoles[bhInd], ray);
        Vector3 axis = normalize(getAxis(ray, _BlackHoles[bhInd].position));
        Vector3 rod1 = rodrigez(ray.direction, axis, angle / 2.0f);
        float d = getApproachCollisionDistance(ray, _BlackHoles[bhInd].position);

        Vector3 hitBack = planeIntersection(_BlackHoles[bhInd].position, ray.direction, ray.origin, rod1);
        Vector3 hitFront = rotatePointAroundPoint(hitBack, _BlackHoles[bhInd].position, axis, angle);

        v2 = hitFront - ray.origin;
        v3 = normalize(rodrigez(ray.direction, axis, angle));
        rot = axis * angle;
    }
    bool nextPosition(Ray ray, float rayLength, int[] forbiddenIn, int forbiddenC, out Vector3 v1, out Vector3 v2, out Vector3 v3, out int[] outForbidden, out int outForbiddenC, out bool shadowHit)
    {
        v1 = new Vector3(0, 0, 0);
        v2 = new Vector3(0, 0, 0);
        v3 = new Vector3(0, 0, 0);

        int[] forbidden = new int[100];
        for (int t = 0; t < forbiddenC; ++t)
        {
            forbidden[t] = forbiddenIn[t];
        }
        outForbidden = new int[100];

        int frontId = -1;
        float minDist = 99999999;

        Vector3 totalRotVector = new Vector3(0, 0, 0);
        Vector3 halfRotVector = new Vector3(0, 0, 0);

        shadowHit = false;

        outForbiddenC = 0;

        uint c = (uint)_BlackHoles.Length;

        bool istnieje = false;
        for (int i = 0; i < c; ++i)
        {
            if (contains(forbidden, forbiddenC, i))
                continue;
            Transform center = _BlackHoles[i];
            Vector3 axis = normalize(getAxis(ray, center.position));
            float d = getApproachCollisionDistance(ray, center.position);
            if (d > 0)
            {
                if (d < minDist)
                {
                    minDist = d;
                    frontId = i;
                }
            }
        }

        Vector3 v2out = new Vector3(0, 0, 0);
        Vector3 v3out = ray.direction;

        if (frontId >= 0)
        {
            istnieje = true;
            forbidden[forbiddenC] = frontId;
            forbiddenC++;
            outForbidden[outForbiddenC] = frontId;
            outForbiddenC++;

            v1 = ray.direction * minDist;
            Vector3 rot;
            Vector3 mv1;
            Vector3 mv2;
            Vector3 mv3;
            Ray extendedRay = CreateRay(ray.origin - ray.direction * rayLength, ray.direction);
            solveFullState(extendedRay, frontId, out rot,out mv1,out mv2,out mv3);
            if (length(rot) >= 90)
            {
                //bh shadow
                v2 = _BlackHoles[frontId].position - (ray.origin + v1);
                shadowHit = true;
                return false;
            }
            v2out += mv2;
            halfRotVector += rot / 2.0f;
            for (int _i = 0; _i < 10; ++_i)
            {
                Ray ray2;
                Vector3 ov1, ov2, ov3, orot;
                int bhInd;
                bool state = solveCrossVector2(extendedRay, rot, mv1, mv2, mv3, _BlackHoles[frontId].position, forbidden, forbiddenC,out ray2, out orot, out ov1, out ov2, out ov3, out bhInd);
                //bool state = solveCrossVector(ray, rot, v1, mv2, mv3, forbidden, forbiddenC, ray2, orot, ov1, ov2, ov3, bhInd);

                if (state)
                {

                    if (length(orot) >= 90)
                    {
                        //bh shadow
                        v2 = _BlackHoles[bhInd].position - (ray.origin + v1);
                        shadowHit = true;
                        return false;
                    }
                    v2out += ov2;
                    halfRotVector += orot / 2.0f;
                    //forbidden.Add(bhInd);
                    //outForbidden.Add(frontId);
                    forbidden[forbiddenC] = bhInd;
                    forbiddenC++;
                    outForbidden[outForbiddenC] = bhInd; //to bylo inaczej !!!
                    outForbiddenC++;
                }
                else
                    break;
            }
        }
        else
        {
            //koniec, promień leci w nieskończoność
        }
        v2 = v2out;
        v3 = rodrigez(v3out, normalize(halfRotVector), length(halfRotVector) * 2);
        return istnieje;
    }
}
