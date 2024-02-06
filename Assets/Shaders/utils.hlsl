//---------------Utils---------------//
float gt(float v1, float v2)
{
    return step(v2, v1);
}
float lt(float v1, float v2)
{
    return step(v1, v2);
}
float between(float val, float start, float end)
{
    return gt(val, start) * lt(val, end);
}
float eq(float v1, float v2, float e)
{
    return between(v1, v2 - e, v2 + e);
}
float s_gt(float v1, float v2, float e)
{
    return smoothstep(v2 - e, v2 + e, v1);
}
float s_lt(float v1, float v2, float e)
{
    return smoothstep(v1 - e, v1 + e, v2);
}
float s_between(float val, float start, float end, float epsilon)
{
    return s_gt(val, start, epsilon) * s_lt(val, end, epsilon);
}
float s_eq(float v1, float v2, float e, float s_e)
{
    return s_between(v1, v2 - e, v2 + e, s_e);
}

//---------------Space folding---------------//
////////////////////////////////////////////////////////////////////
/// From https://github.com/HackerPoet/PySpace but slightly edited.
////////////////////////////////////////////////////////////////////
void planeFold(inout float4 z, float3 n, float d) {
    z.xyz -= 2.0 * min(0.0, dot(z.xyz, n) - d) * n;
}
void absFold(inout float4 z, float3 c) {
    z.xyz = abs(z.xyz - c) + c;
}
void sierpinskiFold(inout float4 z) {
    z.xy -= min(z.x + z.y, 0.0);
    z.xz -= min(z.x + z.z, 0.0);
    z.yz -= min(z.y + z.z, 0.0);
}
void mengerFold(inout float4 z) {
    float a = min(z.x - z.y, 0.0);
    z.x -= a;
    z.y += a;
    a = min(z.x - z.z, 0.0);
    z.x -= a;
    z.z += a;
    a = min(z.y - z.z, 0.0);
    z.y -= a;
    z.z += a;
}
float sphereFold(float4 z, float minR, float maxR, float bloatFactor) { // bloat = 1 will not change size.
    float r2 = dot(z.xyz, z.xyz);
    return max(maxR / max(minR, r2), bloatFactor);
}
float spikeySphereFold(float4 z, float minR, float maxR, float bloatFactor, float spikeFactor) {
    float r2 = dot(z.xyz, z.xyz);
    r2 = r2 * spikeFactor + max(abs(z.x), max(abs(z.y), abs(z.z))) * (1.0 - spikeFactor);
    return max(maxR / max(minR, r2), bloatFactor);
}
void boxFold(inout float4 z, float3 r) {
    z.xyz = clamp(z.xyz, -r, r) * 2.0 - z.xyz;
}
void rotX(inout float4 z, float s, float c) {
    z.yz = float2(c * z.y + s * z.z, c * z.z - s * z.y);
}
void rotY(inout float4 z, float s, float c) {
    z.xz = float2(c * z.x - s * z.z, c * z.z + s * z.x);
}
void rotZ(inout float4 z, float s, float c) {
    z.xy = float2(c * z.x + s * z.y, c * z.y - s * z.x);
}
void rotX(inout float4 z, float a) {
    rotX(z, sin(a), cos(a));
}
void rotY(inout float4 z, float a) {
    rotY(z, sin(a), cos(a));
}
void rotZ(inout float4 z, float a) {
    rotZ(z, sin(a), cos(a));
}

//---------------Repitions---------------//
////////////////////////////////////////////////
/// From https://github.com/HackerPoet/PySpace.
////////////////////////////////////////////////
float Mod(float x, float y)
{
    return x - y * floor(x / y);
}
float2 Mod(float2 x, float2 y)
{
    return x - y * floor(x / y);
}
float3 Mod(float3 x, float3 y)
{
    return x - y * floor(x / y);
}
float4 Mod(float4 x, float4 y)
{
    return x - y * floor(x / y);
}
float SignOfSin(float angle)
{
    // Convert the angle to the range -? to ?
    float wrappedAngle = angle - 2 * 3.14159265359 * floor((angle + 3.14159265359) / (2 * 3.14159265359));

    // Determine the sign based on the quadrant
    float sign = wrappedAngle < 0 ? -1 : 1;

    return sign;
}
float2 SignOfSin(float2 angles)
{
    float2 wrappedAngles = angles - 2 * 3.14159265359 * floor((angles + 3.14159265359) / (2 * 3.14159265359));
    float2 sign = wrappedAngles < float2(0, 0) ? float2(-1, -1) : float2(1, 1);
    return sign;
}
float3 SignOfSin(float3 angles)
{
    float3 wrappedAngles = angles - 2 * 3.14159265359 * floor((angles + 3.14159265359) / (2 * 3.14159265359));
    float3 sign = wrappedAngles < float3(0, 0, 0) ? float3(-1, -1, -1) : float3(1, 1, 1);
    return sign;
}
float4 SignOfSin(float4 angles)
{
    float4 wrappedAngles = angles - 2 * 3.14159265359 * floor((angles + 3.14159265359) / (2 * 3.14159265359));
    float4 sign = wrappedAngles < float4(0, 0, 0, 0) ? float4(-1, -1, -1, -1) : float4(1, 1, 1, 1);
    return sign;
}

//---------------Distance estimators---------------//
////////////////////////////////////////////////
/// From https://github.com/HackerPoet/PySpace.
////////////////////////////////////////////////
float de_box3(float3 p, float3 b)
{
    float3 q = abs(p) - b;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
}
float de_sphere(float4 p, float r) {
    return (length(p.xyz) - r) / p.w;
}
float de_box(float4 p, float3 s) {
    float3 a = abs(p.xyz) - s;
    return (min(max(max(a.x, a.y), a.z), 0.0) + length(max(a, 0.0))) / p.w;
}
float de_tetrahedron(float4 p, float r) {
    float md = max(max(-p.x - p.y - p.z, p.x + p.y - p.z),
        max(-p.x + p.y + p.z, p.x - p.y + p.z));
    return (md - r) / (p.w * sqrt(3.0));
}
float de_inf_cross(float4 p, float r) {
    float3 q = p.xyz * p.xyz;
    return (sqrt(min(min(q.x + q.y, q.x + q.z), q.y + q.z)) - r) / p.w;
}
float de_inf_cross_xy(float4 p, float r) {
    float3 q = p.xyz * p.xyz;
    return (sqrt(min(q.x, q.y) + q.z) - r) / p.w;
}
float de_inf_line(float4 p, float3 n, float r) {
    return (length(p.xyz - n * dot(p.xyz, n)) - r) / p.w;
}