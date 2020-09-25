#ifndef RAY_MARCHING_SDF
#define RAY_MARCHING_SDF

// ====================SDF==================== //

float sdfBox(float3 p, float c, float3 b)
{
    p -= c;
    float3 d = abs(p) - b;
    return min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0));
}

float sdfSphere(float3 p, float3 c, float r)
{
    return length(p - c) - r;
}

float sdfCapsule(float3 p, float3 a, float3 b, float r)
{
    float3 ab = b - a;
    float3 ap = p - a;
    p -= a + saturate(dot(ap, ab) / dot(ab, ab)) * ab;
    return length(p) - r;
}

float sdfTorus(float3 p, float2 t)
{
    float2 q = float2(length(p.xz) - t.x, p.y);
    return length(q) - t.y;
}

// =========================================== //

// ====================Operator==================== //

// Union 并
float opU(float a, float b, float smooth)
{
    float h = clamp(0.5 + 0.5 * (b - a) / smooth, 0.0, 1.0);
    return lerp(b, a, h) - smooth * h * (1.0 - h);
}

// Subtraction 减
float opS(float a, float b, float smooth)
{
    float h = clamp(0.5 - 0.5 * (b + a) / smooth, 0.0, 1.0);
    return lerp(b, -a, h) + smooth * h * (1.0 - h);
}

// Intersection 交
float opI(float a, float b, float smooth)
{
    float h = clamp(0.5 - 0.5 * (b - a) / smooth, 0.0, 1.0);
    return lerp(b, a, h) + smooth * h * (1.0 - h);
}

// ================================================ //

#endif