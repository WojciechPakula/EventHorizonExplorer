using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRayMethod : MonoBehaviour
{
    public Transform[] bh;

    public LineRenderer lr;

    float[] _textureGravity;
    int _textureWidth;
    int _textureHeight;

    public GameObject pointer;
    public GameObject pointer2;
    public float publicFloat;

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

    float getBlackHoleAngle(Transform bc, Ray ray)
    {
        return getBlackHoleAngle(bc.position, ray);
    }

    float getBlackHoleAngle(Vector3 bc, Ray ray)
    {
        float beta = 0;
        Vector3 vr = bc - ray.origin;
        float alpha = Vector3.Angle(ray.direction, vr) * Mathf.PI / 180.0f;
        float r = vr.magnitude;

        float _x_r = Mathf.Sqrt(Mathf.Sqrt(r)) / 4.0f;
        float _y_alpha = alpha / Mathf.PI;

        float tableOutput = getTable(_x_r, _y_alpha, 1);

        if (tableOutput >= 6.0f)
        {
            return 100;
        }
        beta = tableOutput * tableOutput * tableOutput * tableOutput * Mathf.PI / 180.0f;//w radianach

        return beta;
    }

    float getApproachCollisionDistance(Ray ray, Vector3 pos)
    {
        Vector3 tmp = pos - ray.origin;
        var ret = tmp.magnitude* Vector3.Dot(ray.direction.normalized, tmp.normalized);
        /*if (Vector3.Angle(ray.direction.normalized, tmp.normalized) > 90)
        {
            ret *= -1;
        }*/
        return ret;
    }

    Ray _ray;

    // Start is called before the first frame update
    void Start()
    {
        var tex = TextureManager.loadPFMtoTexture("GIG.pfm");
        _textureGravity = TextureManager.textureToFloats(tex);
        _textureWidth = tex.width;
        _textureHeight = tex.height;
        _ray = new Ray();
    }

    Vector3 rodrigez(Vector3 v, Vector3 k, float angle)
    {
        k = k.normalized;
        Vector3 a0 = v * Mathf.Cos(angle);
        Vector3 a1 = Vector3.Cross(k, v) * Mathf.Sin(angle);
        Vector3 a2 = k * Vector3.Dot(k, v) * (1.0f - Mathf.Cos(angle));
        return a0 + a1 + a2;
    }
    Ray bendRayLite(Ray ray, Vector3 center, float angle)
    {
        //Vector3 position = ray.origin + ray.direction * getApproachCollisionDistance(ray, center);
        Vector3 bend = rodrigez(ray.direction, Vector3.Cross(ray.direction, center - ray.origin), angle);
        Ray ret = new Ray(ray.origin, bend);
        return ret;
    }
    Vector3 getAxis(Ray ray, Vector3 center)
    {
        return Vector3.Cross(ray.direction, center - ray.origin);
    }
    Ray bendRayLiteAxis(Ray ray, Vector3 axis, float angle)
    {
        Vector3 bend = rodrigez(ray.direction, axis, angle);
        Ray ret = new Ray(ray.origin, bend);
        return ret;
    }
    Ray bendRayLitePosition(Ray ray, Vector3 center, float angle)
    {
        Vector3 position = ray.origin + ray.direction * getApproachCollisionDistance(ray, center);
        Vector3 bend = rodrigez(ray.direction, Vector3.Cross(ray.direction, center - ray.origin), angle);
        Ray ret = new Ray(position, bend);
        return ret;
    }

    Ray bendRay(Ray ray, Vector3 position, Vector3 center, float angle)
    {
        Vector3 bend = rodrigez(ray.direction, Vector3.Cross(ray.direction, center - ray.origin), angle);
        Ray ret = new Ray(position, bend);
        return ret;
    }
    Ray bendRay2(Ray ray, Vector3 position, Vector3 axis, float angle)
    {
        Vector3 bend = rodrigez(ray.direction, axis, angle);
        Ray ret = new Ray(position, bend);
        return ret;
    }
    
    bool getSimpleBlackholeCollision(Ray ray, int mode, out Transform blackHole, out float d, out int index)
    {
        int count = bh.Length;
        blackHole = null;
        d = 0;
        index = 0;
        if (count <= 0) return false;

        bool ret = false;
        blackHole = bh[0];
        if (mode == -1)
        {
            float max = -9999999;
            for (int i = 0; i < count; i++)
            {
                Transform tmp = bh[i];
                float a = getApproachCollisionDistance(ray, tmp.position);
                if (a < 0 && a > max)
                {
                    max = a;
                    d = -max;
                    blackHole = tmp;
                    ret = true;
                    index = i;
                }
            }
        }
        else if (mode == 1)
        {
            float min = 9999999;
            for (int i = 0; i < count; i++)
            {
                Transform tmp = bh[i];
                float a = getApproachCollisionDistance(ray, tmp.position);
                if (a >= 0 && a < min)
                {
                    min = a;
                    d = min;
                    blackHole = tmp;
                    ret = true;
                    index = i;
                }
            }
        }
        return ret;
    }

    Vector3 directionCollector(Ray ray, int index)
    {
        Vector3 dir = ray.direction;
        for (int i = 0; i < bh.Length; ++i)
        {
            if (i == index) continue;
            Transform tmp = bh[i];
            var beta = getBlackHoleAngle(tmp, ray);
            Ray r2 = bendRayLite(ray, tmp.position, beta);
            dir += r2.direction;
        }
        return dir / bh.Length;
    }

    int rayStep(int index)
    {
        float dd = 0, du = 0;
        Transform bp, bt;
        int indp = 0, indt = 0;
        bool tyl = getSimpleBlackholeCollision(_ray, -1, out bt, out dd, out indt);

        int ret = index;
        
        if (tyl && indt != index)
        {
            var beta = getBlackHoleAngle(bt, _ray);
            _ray = bendRayLite(_ray, bt.position, beta);
            ret = indt;
        }
        bool przod = getSimpleBlackholeCollision(_ray, 1, out bp, out du, out indp);
        if (przod)
        {
            var beta = getBlackHoleAngle(bp, _ray);
            var axis = getAxis(_ray, bp.position);
            _ray = bendRayLitePosition(_ray, bp.position, beta/2);
            addPoint(_ray.origin);
            _ray.direction = directionCollector(_ray, indp);
            Vector3 bend = _ray.origin + _ray.direction * (bp.position - _ray.origin).magnitude * Mathf.Sqrt(2 * (1 - Mathf.Cos(beta)));
            _ray.origin = bend;
            addPoint(_ray.origin);
            _ray = bendRayLiteAxis(_ray, axis, beta / 2);
            _ray.origin = _ray.origin + _ray.direction * 0.001f;
            ret = indp;
        } else
        {
            return -2;
        }
        return ret;
    }

    void rayTest(Ray ray, float length, List<int> forbidden)
    {
        for (int i = 0; i < bh.Length; ++i)
        {
            if (forbidden.Contains(i)) continue;
            float d = getApproachCollisionDistance(ray, bh[i].position);
            if (d > 0 && d < length)
            {
                //przeciecie
                var beta = getBlackHoleAngle(bh[i], ray);
                Vector3 tmp = ray.direction;
                Ray ray2 = bendRayLite(_ray, bh[i].position, beta/2);

            }
        }
        //getApproachCollisionDistance(ray, tmp.position);
    }

    List<int> rayStep2(List<int> forbidden)
    {
        float dd = 0, du = 0;
        Transform bp, bt;
        int indp = 0, indt = 0;
        bool tyl = getSimpleBlackholeCollision(_ray, -1, out bt, out dd, out indt);

        List<int> forbidden2 = new List<int>();

        if (tyl && !forbidden.Contains(indt))
        {
            var beta = getBlackHoleAngle(bt, _ray);
            _ray = bendRayLite(_ray, bt.position, beta);
            forbidden2.Add(indt);
        }
        bool przod = getSimpleBlackholeCollision(_ray, 1, out bp, out du, out indp);
        if (przod && !forbidden.Contains(indp))
        {
            var beta = getBlackHoleAngle(bp, _ray);
            var axis = getAxis(_ray, bp.position);
            _ray = bendRayLitePosition(_ray, bp.position, beta / 2);
            addPoint(_ray.origin);
            //jezeli sie przecina


            /*var beta = getBlackHoleAngle(bp, _ray);
            var axis = getAxis(_ray, bp.position);
            _ray = bendRayLitePosition(_ray, bp.position, beta / 2);
            addPoint(_ray.origin);
            _ray.direction = directionCollector(_ray, indp);
            Vector3 bend = _ray.origin + _ray.direction * (bp.position - _ray.origin).magnitude * Mathf.Sqrt(2 * (1 - Mathf.Cos(beta)));
            _ray.origin = bend;
            addPoint(_ray.origin);
            _ray = bendRayLiteAxis(_ray, axis, beta / 2);
            _ray.origin = _ray.origin + _ray.direction * 0.001f;
            forbidden2.Add(indp);*/
        }
        else
        {
            return null;
        }
        return forbidden2;
    }

    Vector3 rotatePointAroundPoint(Vector3 p1, Vector3 c, Vector3 axis, float angle)
    {
        return rodrigez(p1 - c, axis, angle) + c;
    }

    int getConditionType(Ray ray, Vector3 center)
    {
        var beta = getBlackHoleAngle(center, ray) % (2 * Mathf.PI);
        var d = getApproachCollisionDistance(ray, center);
        if (d >= 0) return 1;
        else
        if (d < 0)
        {
            Ray r2 = bendRayLite(ray, center, beta/2.0f);
            float c = d/Mathf.Cos(beta/2.0f);
            Vector3 hitpoint = r2.origin + r2.direction * c;
            Vector3 axis = getAxis(ray, center);
            Vector3 hitpoint2 = rotatePointAroundPoint(hitpoint, center, axis, beta);
            if (Vector3.Angle(hitpoint2 - r2.origin, r2.direction) < 0.001f)
                return 2;
            else
                return 3;
        }
        return 0;
    }

    Vector3 planeIntersection(Vector3 point, Vector3 normal, Vector3 origin, Vector3 direction)
    {
        direction = direction.normalized;
        normal = normal.normalized;
        Vector3 normal2 = -1.0f * normal;

        Vector3 tmp = normal; //wybrac lepszy normal

        float kat = Vector3.Angle(direction, normal) * Mathf.PI / 180.0f;
        if (kat <= Mathf.PI / 2.0f)
            tmp = normal;
        else
            tmp = normal2;

        float alpha = Mathf.PI / 2.0f - Vector3.Angle(origin - point, tmp) * Mathf.PI / 180.0f;
        float beta = Mathf.PI / 2.0f - Vector3.Angle(direction, tmp) * Mathf.PI/180.0f;

        float d = (point - origin).magnitude;
        float x = -d * Mathf.Sin(alpha) / Mathf.Sin(beta);

        return origin + direction * x;
    }

    /*void getOffsetVectors(Vector3 dir, Vector3 pos, Vector3 center, out Vector3 v1, out Vector3 v2)
    {
        v1 = Vector3.zero;
        v2 = Vector3.zero;

        Ray r = new Ray(pos, dir);
        var beta = getBlackHoleAngle(center, r) % (2 * Mathf.PI);
        Vector3 axis = getAxis(r, center);

        Vector3 hit2 = rotatePointAroundPoint(pos, center, axis, beta);
        v1 = hit2 - pos;
        v2 = rodrigez(dir, axis, beta);
        v2.Normalize();
    }*/

    Vector3 interpolateDirection(Vector3 start, Vector3 end, float val)
    {
        val = Mathf.Clamp(val, 0, 1);
        return start * (1 - val) + end * val;
    }
    Vector3 interpolateVector(Vector3 start, Vector3 end, float val)
    {
        val = Mathf.Clamp(val, 0, 1);
        return start * (1 - val) + end * val;
    }
    Vector3 interpolateVector2(Vector3 start, Vector3 end, float val)
    {
        val = Mathf.Clamp(Mathf.Pow(val, 0.25f), 0, 1);//?????????????????????????????????????????????????????????????????? czy to ma sens?
        return start * (1 - val) + end * val;
    }
    /*Vector3 interpolateDirectionRadial(Vector3 startPos, Vector3 startDir, float beta, Vector3 center, Vector3 testPoint)
    {
        var axis = getAxis(new Ray(startPos, startDir), center);
        var endPos = rotatePointAroundPoint(startPos, center, axis, beta);
        var endDir = rodrigez(startDir, axis, beta);

        return interpolateDirectionRadial(startPos, startDir, endPos, endDir, beta, center, testPoint);
    }*/
    float getDistanceFromPlane(Vector3 normal, Vector3 pos, Vector3 point)
    {
        var tmp = point - pos;
        var ang = Vector3.Angle(normal, tmp.normalized)*Mathf.PI/180.0f;
        return Mathf.Abs(Mathf.Cos(ang)* tmp.magnitude);
    }

    float fPrim(float x, Vector3 startDir, Vector3 startPos, Vector3 endPos, Vector3 axis, float beta, Vector3 mass, float dx)
    {
        return (f(dx + x, startDir, startPos, endPos, axis, beta, mass) - f(x, startDir, startPos, endPos, axis, beta, mass)) / dx;
    }
    float f(float x, Vector3 startDir, Vector3 startPos, Vector3 endPos ,Vector3 axis, float beta, Vector3 mass)
    {
        //x = Mathf.Clamp(x,0,1);
        var d = getInterpolationDirection(startDir, axis, beta, x).normalized;
        var point = startPos + (endPos - startPos).normalized * (endPos - startPos).magnitude * x;
        return getDistanceFromPlane(d, mass, point);
    }

    bool newtonsMethod(Vector3 startDir, Vector3 startPos, Vector3 endPos, Vector3 axis, float beta, Vector3 mass, out Vector3 pos, out Vector3 dir, out float outD, float dx = 0.0001f)
    {
        float epsilon = 0.001f;
        float x = 0.5f;
        float delta = 0;
        pos = Vector3.zero;
        dir = Vector3.zero;
        outD = 0;
        for (int i = 0; i < 10 ; ++i)
        {
            float _f = f(x, startDir, startPos, endPos, axis, beta, mass);
            float _fPrim = fPrim(x, startDir, startPos, endPos, axis, beta, mass, dx);
            float tmp = x - _f / _fPrim;
            delta = Mathf.Abs(tmp - x);
            x = tmp;
            if (delta < epsilon)
            {
                break;
            }
        }
        if (delta < epsilon && x >= 0 && x <= 1)
        {
            //sukces
            var tmp2 = endPos - startPos;
            pos = startPos + tmp2.normalized * tmp2.magnitude * x;
            dir = getInterpolationDirection(startDir, axis, beta, x).normalized;
            outD = x;
            return true;
        }
        return false;
    }

    //Gotowa interpolacja
    bool interpolateDirectionRadial(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, float beta, Vector3 center, Vector3 testPoint, out Vector3 outPoint, out Vector3 outDirection, out float outD)
    {
        outPoint = Vector3.zero;
        outDirection = Vector3.zero;
        var axis = getAxis(new Ray(startPos, startDir), center);
        outD = 0;
        //var endDir = rodrigez(startDir, axis, beta);
        var result = newtonsMethod(startDir,startPos,endPos,axis,beta,testPoint, out outPoint, out outDirection, out outD, 0.0001f);
        return result;
    }

    Vector3 getInterpolationDirection(Vector3 startDir, Vector3 axis, float beta, float x)
    {
        return rodrigez(startDir, axis, beta * x);
    }

    void getOffsetVectors(Ray ray, Vector3 center, out Vector3 v1, out Vector3 v2, out Vector3 v3)
    {
        v1 = Vector3.zero;
        v2 = Vector3.zero;
        v3 = Vector3.zero;

        var beta = getBlackHoleAngle(center, ray) % (2 * Mathf.PI);
        var d = getApproachCollisionDistance(ray, center);
        Vector3 axis = getAxis(ray, center);
        Vector3 hit2 = Vector3.zero;
        if (d >= 0)
        {
            Vector3 hit1 = ray.origin + ray.direction * d;
            v1 = hit1 - ray.origin;
            hit2 = rotatePointAroundPoint(hit1, center, axis, beta);
            v2 = hit2 - hit1;
            v3 = rodrigez(ray.direction, axis, beta);
            v3.Normalize();
        }
        else
        if (d < 0)
        {
            Vector3 dir2 = rodrigez(ray.direction, axis, beta/2.0f);
            float c = d / Mathf.Cos(beta / 2.0f);
            Vector3 hit1 = ray.origin + dir2 * c;
            hit2 = rotatePointAroundPoint(hit1, center, axis, beta);
            if (Vector3.Angle(hit2 - ray.origin, dir2) < 0.001f)
            {
                // stan posredni
                v2 = hit2 - ray.origin;
            }
            else
            {
                //czarna dziura za promieniem
            }
            v3 = rodrigez(ray.direction, axis, beta);
        }
    }

    Ray solveCondition(Ray ray, Vector3 center)
    {
        var beta = getBlackHoleAngle(center, ray)%(2*Mathf.PI);
        var d = getApproachCollisionDistance(ray, center);
        if (d >= 0)
        {
            //czarna dziura przed promieniem
            Vector3 axis = getAxis(ray, center);
            ray = bendRayLitePosition(ray, center, beta / 2.0f);
            addPoint(ray.origin);
            ray.origin = ray.origin + ray.direction * (center - ray.origin).magnitude * Mathf.Sqrt(2 * (1 - Mathf.Cos(beta)));
            ray = bendRayLiteAxis(ray, axis, beta / 2.0f);
            addPoint(ray.origin);
            return ray;
        }
        else
        if (d < 0)
        {
            Ray r2 = bendRayLite(ray, center, beta / 2.0f);
            float c = d / Mathf.Cos(beta / 2.0f);
            Vector3 hitpoint = r2.origin + r2.direction * c;
            Vector3 axis = getAxis(ray, center);
            Vector3 hitpoint2 = rotatePointAroundPoint(hitpoint, center, axis, beta);
            if (Vector3.Angle(hitpoint2 - r2.origin, r2.direction) < 0.001f)
            {
                // stan posrednireturn ray;
                ray = r2;
                ray.origin += ray.direction*(ray.origin - hitpoint2).magnitude;
                ray = bendRayLiteAxis(ray, axis, beta / 2.0f);
                addPoint(ray.origin);
                return ray;
            }
            else
            {
                //czarna dziura za promieniemreturn ray;
                ray = bendRayLite(ray, center, beta);
                return ray;
            }
        }
        return ray;
    }

    void checkVector(Vector3 origin, Vector3 v, Vector3 startDir, Vector3 endDir, Transform[] bh, List<int> forbidden)
    {

        for (int i = 0; i < bh.Length; ++i)
        {
            if (forbidden.Contains(i)) continue;
            var d = getApproachCollisionDistance(new Ray(origin, v.normalized), bh[i].position);
        }
    }

    void calcRay()
    {

        _ray.origin = this.transform.position;
        _ray.direction = this.transform.right;
        clearPoints();


        addPoint(_ray.origin);

        //List<int> forbidden = new List<int>();

        //Debug.Log(getConditionType(_ray, bh[0].position));
        //_ray = solveCondition(_ray, bh[0].position);

        Vector3 v1, v2, v3;
        List<int> forbidden = new List<int>();

        getOffsetVectors(_ray, bh[0].position, out v1, out v2, out v3);

        forbidden.Add(0);
        //agragacja
        for (int i = 0; i < bh.Length; ++i)
        {
            if (forbidden.Contains(i)) continue;
            Vector3 center = bh[0].position;
            Vector3 outPoint, outDirection;
            float outD;

            var beta = getBlackHoleAngle(bh[0].position, _ray) % (2 * Mathf.PI);
            bool boo = interpolateDirectionRadial(_ray.origin + v1, _ray.direction, _ray.origin + v1 + v2, v3, beta, center, bh[i].position, out outPoint, out outDirection, out outD);

            pointer.transform.position = outPoint;

            if (boo)
            {
                Vector3 v1_, v2_, v3_;

                Vector3 ultPosition = interpolateVector2(_ray.origin ,outPoint, outD);
                pointer2.transform.position = ultPosition;
                //Ray _ray2 = new Ray(ultPosition, outDirection);

                Ray _ray2 = new Ray(outPoint - outDirection * (v1.magnitude + v2.magnitude * outD), outDirection);
                getOffsetVectors(_ray2, bh[i].position, out v1_, out v2_, out v3_);

                v2 += v2_;
                v3 += v3_;
                v3.Normalize();
            }
        }


        addPoint(_ray.origin + v1);
        addPoint(_ray.origin + v1 + v2);

        //var tmp = planeIntersection(bh[0].position, bh[0].right, _ray.origin, _ray.direction);
        //pointer.transform.position = tmp;
        addPoint(_ray.origin + v1 + v2 + v3 * 100);

        /*Transform bh0 = bh[0];

        float d = getApproachCollisionDistance(_ray, bh0.position);

        if (d > 0)
        {
            Vector3 colPoint = _ray.origin + _ray.direction * d;
            addPoint(colPoint);
            var beta = getBlackHoleAngle(bh0, _ray);
            Vector3 axis = Vector3.Cross(_ray.direction, bh0.position - _ray.origin);

            Ray tmp = bendRay2(_ray, colPoint, axis, beta / 2);
            Vector3 bend = rodrigez(colPoint - bh0.position, Vector3.Cross(colPoint - bh0.position, _ray.direction), beta) + bh0.position;
            //Vector3 bend = tmp.origin + tmp.direction * (bh0.position - tmp.origin).magnitude * Mathf.Sqrt(2*(1-Mathf.Cos(beta)));
            addPoint(bend);
            tmp = bendRay2(tmp, bend, axis, beta / 2);

            addPoint(tmp.origin + tmp.direction * 10);
        }

        Transform tmpbh;


        float dd = 0, du = 0;
        getSimpleBlackholeCollision(_ray, -1, out tmpbh, out du);
        getSimpleBlackholeCollision(_ray, 1, out tmpbh, out dd);
        Debug.Log("DEBUG: "+dd+"\t"+du);*/
    }

    Vector3 projectOnPlane(Vector3 vector, Vector3 planeNormal)
    {
        return vector - planeNormal * Vector3.Dot(vector, planeNormal);
    }

    float angleAdvanced(Vector3 a, Vector3 b, Vector3 axis)
    {
        axis = axis.normalized;
        a = projectOnPlane(a, axis).normalized;
        b = projectOnPlane(b, axis).normalized;

        var ang = Vector3.Angle(a,b)*Mathf.PI/180.0f;
        var cr = Vector3.Cross(a,b).normalized;

        if ((cr - axis).magnitude < 0.001)
        {
            return ang;
        } else
        {
            return 2 * Mathf.PI - ang;
        }
    }
    /*Vector3 ProjectOnPlane(Vector3 v, Vector3 planeNormal)
    {
        return v - planeNormal * Vector3.Dot(v, planeNormal);
    }*/
    float angleToPlane(Vector3 dir, Vector3 normal)
    {
        Vector3 pr = projectOnPlane(dir, normal);
        float ang = Vector3.Angle(pr, dir);
        if (Vector3.Angle(pr, normal) > 90) ang *= -1;
        return ang;
    }
    /*void advancedLinePoint(Vector3 p1, Vector3 p2, Vector3 pos)
    {

    }*/
    bool solveCrossVector(Ray ray, Vector3 rot, Vector3 v1, Vector3 v2, Vector3 v3, List<int> forbidden, out Ray _ray2, out Vector3 _rot, out Vector3 _v1, out Vector3 _v2, out Vector3 _v3, out int choosenBH)
    {
        Vector3 axis = rot.normalized;//???????
        float beta = rot.magnitude;
        Vector3 startDir = ray.direction;
        Vector3 endDir = v3.normalized;
        Vector3 startPos = ray.origin + v1;
        Vector3 endPos = ray.origin + v1 + v2;
        Vector3 diff = endPos - startPos;

        _v1 = Vector3.zero;
        _v2 = Vector3.zero;
        _v3 = Vector3.zero;
        _rot = Vector3.zero;
        _ray2 = new Ray(Vector3.zero, Vector3.zero);

        bool isCross = false;
        float outDMin = 1000;
        Vector3 choosenDir = Vector3.zero;
        Vector3 choosenPos = Vector3.zero;
        choosenBH = -1;
        for (int i = 0; i < bh.Length; ++i)
        {
            if (forbidden.Contains(i)) continue;
            var nextBlackHole = bh[i].position;

            /*Vector3 t0 = startPos - nextBlackHole;
            Vector3 t1 = endPos - nextBlackHole;

            Vector3 axis_advanced = Vector3.Cross(t0, t1).normalized;*/

            Vector3 pos, dir;
            float outD;
            
            //metodaNewtona
            float epsilon = 0.001f;
            float x = 0.5f;
            float delta = 0;
            pos = Vector3.zero;
            dir = Vector3.zero;
            outD = 0;
            for (int j = 0; j < 10; ++j)
            {
                var d = rodrigez(startDir, axis, beta * x);
                var point = startPos + (diff).normalized * (diff).magnitude * x;
                var tmp = point - nextBlackHole;
                var ang = Vector3.Angle(d, tmp.normalized) * Mathf.PI / 180.0f;
                float _f = Mathf.Abs(Mathf.Cos(ang) * tmp.magnitude);

                float dx = 0.001f;

                var d2 = rodrigez(startDir, axis, beta * (x+dx));
                var point2 = startPos + (diff).normalized * (diff).magnitude * (x + dx);
                var tmp2 = point2 - nextBlackHole;
                var ang2 = Vector3.Angle(d2, tmp2.normalized) * Mathf.PI / 180.0f;
                float _f2 = Mathf.Abs(Mathf.Cos(ang2) * tmp2.magnitude);

                float _fPrim = (_f2 - _f)/dx;
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
                    choosenDir = rodrigez(startDir, axis, beta * x).normalized;
                    choosenPos = startPos + diff.normalized * diff.magnitude * outD;
                    choosenBH = i;
                }
            }
        }
        if (isCross)
        {
            //ustaw wyjscie
            _ray2 = new Ray(choosenPos - choosenDir * (v1.magnitude + diff.magnitude * outDMin), choosenDir);
            var beta2 = getBlackHoleAngle(bh[choosenBH].position, _ray2);
            var axis2 = Vector3.Cross(choosenDir, bh[choosenBH].position - choosenPos).normalized;
            _rot = axis2 * beta2;

            _v1 = (choosenPos - _ray2.origin);
            _v2 = rotatePointAroundPoint(choosenPos, bh[choosenBH].position, axis2, beta2) - choosenPos;
            _v3 = rodrigez(choosenDir, axis2, beta).normalized;
        }
        return isCross;
    }

    void solveFullState(Ray ray, int bhInd, out Vector3 rot, out Vector3 v1, out Vector3 v2, out Vector3 v3)
    {
        rot = Vector3.zero;
        v1 = Vector3.zero;
        v2 = Vector3.zero;
        v3 = Vector3.zero;

        var angle = getBlackHoleAngle(bh[bhInd], ray);
        var axis = getAxis(ray, bh[bhInd].position).normalized;
        var d = getApproachCollisionDistance(ray, bh[bhInd].position);
        v1 = d * ray.direction;
        var hitBack = v1 + ray.origin;
        var hitFront = rotatePointAroundPoint(hitBack, bh[bhInd].position, axis, angle);
        v2 = hitFront - hitBack;
        v3 = rodrigez(ray.direction, axis, angle).normalized;
        rot = axis * angle;
    }

    void solveMiddleState(Ray ray, int bhInd, out Vector3 rot, out Vector3 v1, out Vector3 v2, out Vector3 v3)
    {
        rot = Vector3.zero;
        v1 = Vector3.zero;
        v2 = Vector3.zero;
        v3 = Vector3.zero;

        var angle = getBlackHoleAngle(bh[bhInd],ray);
        var axis = getAxis(ray, bh[bhInd].position).normalized;
        var rod1 = rodrigez(ray.direction, axis, angle/2.0f);
        var d = getApproachCollisionDistance(ray, bh[bhInd].position);

        var hitBack = planeIntersection(bh[bhInd].position, ray.direction, ray.origin, rod1);
        var hitFront = rotatePointAroundPoint(hitBack, bh[bhInd].position, axis, angle);

        v2 = hitFront - ray.origin;
        v3 = rodrigez(ray.direction, axis, angle).normalized;
        rot = axis * angle;
    }

    bool nextPosition(Ray ray, float rayLength, List<int> forbiddenIn, out Vector3 v1, out Vector3 v2, out Vector3 v3, out List<int> outForbidden, out bool shadowHit)
    {
        v1 = Vector3.zero;
        v2 = Vector3.zero;
        v3 = Vector3.zero;

        List<int> forbidden = new List<int>(forbiddenIn);

        int frontId = -1;
        float minDist = 99999999;

        Vector3 totalRotVector = Vector3.zero;
        Vector3 halfRotVector = Vector3.zero;

        shadowHit = false;

        outForbidden = new List<int>();
        bool istnieje = false;
        for (int i = 0; i < bh.Length; ++i)
        {
            if (forbidden.Contains(i)) continue;
            var center = bh[i].position;
            var axis = getAxis(ray, center).normalized;
            var d = getApproachCollisionDistance(ray, center);
            if (d > 0)
            {
                if (d < minDist)
                {
                    minDist = d;
                    frontId = i;
                }
            }
        }

        Vector3 v2out = Vector3.zero;
        Vector3 v3out = ray.direction;

        if (frontId >= 0)
        {
            istnieje = true;
            forbidden.Add(frontId);
            outForbidden.Add(frontId);
            v1 = ray.direction * minDist;
            Vector3 rot;
            Vector3 mv1;
            Vector3 mv2;
            Vector3 mv3;
            var extendedRay = new Ray(ray.origin - ray.direction * rayLength, ray.direction);
            solveFullState(extendedRay, frontId, out rot, out mv1, out mv2, out mv3);
            if (rot.magnitude >= 6)
            {
                //bh shadow
                v2 = bh[frontId].position - (ray.origin + v1);
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
                var state = solveCrossVector(extendedRay, rot, mv1, mv2, mv3, forbidden, out ray2, out orot, out ov1, out ov2, out ov3, out bhInd);

                if (state)
                {
                    if (orot.magnitude >= 6)
                    {
                        //bh shadow
                        v2 = bh[bhInd].position - (ray.origin + v1); shadowHit = true;
                        return false;
                    }
                    v2out += ov2;
                    //v3out += ov3;
                    //v3out.Normalize();
                    //midV2 += ov2.magnitude;
                    //midCounter++;
                    halfRotVector += orot / 2.0f;
                    forbidden.Add(bhInd);
                    outForbidden.Add(bhInd);
                }
                else break;
            }
        } else
        {
            //koniec, promień leci w nieskończoność
        }
        v2 = v2out;
        v3 = rodrigez(v3out, halfRotVector.normalized, halfRotVector.magnitude * 2);
        return istnieje;
    }

    void startPosition(Ray ray, out Vector3 v1, out Vector3 v2, out Vector3 v3, out List<int> forbidden, out bool shadowHit)
    {
        v1 = Vector3.zero;
        v2 = Vector3.zero;
        v3 = Vector3.zero;

        forbidden = new List<int>();

        int front = -1;
        float minDist = 99999999;
        shadowHit = false;
        Vector3 totalRotVector = Vector3.zero;
        Vector3 halfRotVector = Vector3.zero;

        for (int i = 0; i < bh.Length; ++i)
        {
            if (forbidden.Contains(i)) continue;
            var center = bh[i].position;
            var axis = getAxis(ray, center).normalized;
            var d = getApproachCollisionDistance(ray, center);
            if (d <= 0)
            {
                //za lub posrednio
                float beta = getBlackHoleAngle(center, ray);
                if (beta >= 6)
                {
                    //bh shadow
                    v2 = center - ray.origin;
                    shadowHit = true;
                    return;
                }
                var vecBack = (ray.origin + ray.direction * d) - center;
                var dir = ray.origin - center;
                if (angleAdvanced(vecBack, dir, axis) > beta)
                {
                    //za
                    totalRotVector += axis * beta;
                    halfRotVector += axis * beta / 2.0f;
                    forbidden.Add(i);
                }
            }
        }
        Vector3 v2out = Vector3.zero;
        Vector3 v3out = ray.direction;
        for (int i = 0; i < bh.Length; ++i)
        {
            if (forbidden.Contains(i)) continue;
            var center = bh[i].position;
            var axis = getAxis(ray, center).normalized;
            var d = getApproachCollisionDistance(ray, center);
            if (d > 0)
            {

            }
            else
            {
                float beta = getBlackHoleAngle(center, ray);
                var vecBack = (ray.origin + ray.direction * d) - center;
                var dir = ray.origin - center;
                if (angleAdvanced(vecBack, dir, axis) <= beta)
                {
                    forbidden.Add(i);
                    //stan posredni
                    Vector3 rot;
                    Vector3 mv1;
                    Vector3 mv2;
                    Vector3 mv3;
                    solveMiddleState(ray, i, out rot, out mv1, out mv2, out mv3);
                    v2out += mv2;
                    halfRotVector += rot/2.0f;

                    if (beta >= 6)
                    {
                        //bh shadow
                        v1 = ray.direction * d;
                        v2 = center - ray.origin - v1;
                        shadowHit = true;
                        return;
                    }

                    for (int _i = 0; _i < 10 ; ++_i)
                    {
                        Ray ray2;
                        Vector3 ov1, ov2, ov3, orot;
                        int bhInd;
                        var state = solveCrossVector(ray, rot, mv1, mv2, mv3, forbidden, out ray2, out orot, out ov1, out ov2, out ov3, out bhInd);
                        
                        if (state)
                        {
                            if (orot.magnitude >= 6)
                            {
                                //bh shadow
                                v1 = ray.direction * d;
                                v2 = bh[bhInd].position - ray.origin - v1;
                                shadowHit = true;
                                return;
                            }
                            v2out += ov2;
                            halfRotVector += orot / 2.0f;
                            forbidden.Add(bhInd);
                        }
                        else break;
                    }
                }
            }
        }
        v2 = v2out;
        v3 = rodrigez(v3out, halfRotVector.normalized, halfRotVector.magnitude*2);
    }

    void calcRay2()
    {
        _ray.origin = this.transform.position;
        _ray.direction = this.transform.forward;
        clearPoints();
        addPoint(_ray.origin);

        Vector3 v1, v2, v3;
        List<int> forbidden;
        bool shadowHit;
        startPosition(_ray, out v1, out v2, out v3, out forbidden, out shadowHit);
        addPoint(_ray.origin + v1);
        addPoint(_ray.origin + v1 + v2);
        if (shadowHit)
        {
            return;
        }
        float length = 0;
        for (int i = 0; i < 10 ; ++i)
        {
            length += v1.magnitude + v2.magnitude;
            //if (v1.magnitude == 0 && v2.magnitude == 0 && _ray.direction == v3) break;
            _ray.direction = v3;
            _ray.origin = _ray.origin + v1 + v2;
            List<int> forbidden2;
            bool state = nextPosition(_ray, length, forbidden, out v1, out v2, out v3, out forbidden2, out shadowHit);
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
            forbidden = forbidden2;
            addPoint(_ray.origin + v1);
            addPoint(_ray.origin + v1 + v2);
        }
        Debug.Log(angleToPlane(v3, new Vector3(0,1,0)));
    }



    // Update is called once per frame
    void Update()
    {
        calcRay2();
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
}
