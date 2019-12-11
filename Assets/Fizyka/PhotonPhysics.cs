using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhotonPhysics {
    public static double G = (float)(6.674083131313131 / 100000000000.0);
    public static double c = 299792458;
    //public static double h = System.Math.Pow(6.62607004081f, -34);

    public static void getNextPosition(Vector3d position, Vector3d momentum, Mass[] masses, double dt, out Vector3d outPosition, out Vector3d outMomentum, out bool outLocker)
    {
        outPosition = position;
        outMomentum = momentum;
        outLocker = false;
        Vector3d delta = Vector3d.zero;
        foreach (Mass obj in masses) {
            try
            {
                delta += deltaVelocityD(position, obj, momentum, dt);
            } catch
            {
                outLocker = true;
                return;
            }
            if ((Vector3d.d_to_f(position) - obj.transform.position).magnitude < obj.rs)
            {
                outLocker = true;
            }
        }
        outMomentum += delta;
        outPosition += outMomentum * dt;
        outMomentum.Normalize();
        
    }

    //pozycja, masa, predkosc, czas
    static Vector3d deltaVelocityD(Vector3d p, Mass obj, Vector3d momentum, double dt)
    {
        double m = 0;
        double r = 0;
        double eventLocker = 0.05;

        Vector2d vv = cartesianToSphericalD(p, momentum, dt, Vector3d.f_to_d(obj.transform.position)) / dt;
        if (vv.x == 0 && vv.y == 0) return Vector3d.zero;
        r = (p - Vector3d.f_to_d(obj.transform.position)).magnitude;
        m = G * obj.mass / (c * c);

        double rs = obj.rs;
        eventLocker = obj.rs * eventLocker;
        double wsp = (r - rs);
        //if (r <= rs+eventLocker) { locker = true; return Vector3d.zero; }
        if (r <= eventLocker) {
            throw new System.Exception("Osiągnięto centrum czarnej dziury");
            return Vector3d.zero;
        }

        double d2rdt2 = 2.0 * m / (r * (r - 2.0 * m)) * vv.x * vv.x + (r - 3.0 * m) * vv.y * vv.y;
        double d2odt2 = -2.0 * (r - 3.0 * m) / (r * (r - 2.0 * m)) * vv.x * vv.y;

        if (r <= rs + eventLocker && r >= rs - eventLocker) { d2rdt2 = 0; d2odt2 = 0; }

        double dvr = d2rdt2 * dt;
        double dvo = d2odt2 * dt;

        vv.x += dvr;
        vv.y += dvo;

        double dr = vv.x * dt;
        double da = vv.y * dt;
        
        var nowa = sphericalToCartesianD(da, dr, p, Vector3d.f_to_d(obj.transform.position), momentum);

        Vector3d oldVelocity = momentum;
        Vector3d newVelocity = (nowa - p) / dt;
        return (newVelocity - oldVelocity);
    }

    static Vector3d sphericalToCartesianD(double da, double dr, Vector3d position, Vector3d zeroPoint, Vector3d momentum)
    {
        Vector3d relPos = position - zeroPoint;
        Vector3d pos = position;
        double r = relPos.magnitude;
        Vector3d dirMass = relPos.normalized;
        Vector3d dirPoz = Vector3d.ProjectOnPlane(momentum, dirMass);//?

        double bok = r * System.Math.Sqrt(2.0 * (1.0 - System.Math.Cos(da)));

        Vector3d dirA = Vector3d.RotateTowards(dirPoz, -dirMass, da / 2.0, 1.0).normalized * bok;//?
        pos += dirA;
        dirMass = (pos - zeroPoint).normalized;
        pos += dirMass * dr;
        return pos;
    }
    static Vector2d cartesianToSphericalD(Vector3d p, Vector3d v, double t, Vector3d zeroPoint)
    {
        Vector3d rel1 = (p - (v * t) - zeroPoint);
        Vector3d rel2 = (p - zeroPoint);
        double da = Vector3d.Angle(rel1, rel2) * System.Math.PI / 180.0;
        if (da < 3E-5) return Vector2d.zero;
        double m1 = rel1.magnitude;
        double m2 = rel2.magnitude;
        double dr = (m2 - m1);
        return new Vector2d(dr, da);
    }
}
