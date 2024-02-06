//---------------Includes---------------//
#include "hit.hlsl"

//---------------Main marcher---------------//
struct March {
    float dE;
    int steps;
    bool hasHitPoly;
};
March RayMarch(float startDis, float3 ro, float3 rd, float depth, int maxSteps, float epsFactor)
{
    float d = startDis;
    float cd;
    float3 p;
    float eps = HIT_EPS;

    March m;
    m.hasHitPoly = false;

    for (int i = 0; i < maxSteps; ++i) {
        if (d >= depth) {
            d = depth;
            m.hasHitPoly = true;
            break;
        }

        p = ro + d * rd;
        cd = GetDis(p);

        eps = HIT_EPS * d * epsFactor; // adaptive epsilon
        if (d >= MAX_DIS) {
            d = MAX_DIS;
            break;
        }

        if (cd < 0) { // Quick hack
            d += cd * 10;
        }
        else {
            if (cd < eps) break;
            d += cd;
        }
    }

    m.dE = d;
    m.steps = i;
    return m;
}
March ConeMarch(float subdiv, float startDis, float3 ro, float3 rd, float depth, int maxSteps, float epsFactor)
{
    float d = startDis;
    float cd;
    float3 p;
    float eps = HIT_EPS;

    March m;
    m.hasHitPoly = false;

    for (int i = 0; i < maxSteps; ++i) {
        if (d >= depth) {
            d = depth;
            m.hasHitPoly = true;
            break;
        }

        p = ro + d * rd;
        cd = GetDis(p);

        eps = HIT_EPS * d * epsFactor; // adaptive epsilon
        if (cd < (d * _camTanFov) / subdiv || cd < eps || d >= MAX_DIS) {
            break;
        }

        d += cd;
    }

    m.dE = d;
    m.steps = i;
    return m;
}