#ifndef RAY_MARCHING_SHAPES
#define RAY_MARCHING_SHAPES

#include "SDF.cginc"

#define SHAPE_SPHERE    (0)
#define SHAPE_BOX       (1)
#define SHAPE_CAPSULE   (2)
#define SHAPE_TORUS     (3)

#define OP_UNION        (0)
#define OP_SUBTRACTION  (1)
#define OP_INTERSECTION (2)

#define INFINITY (1e32f)

struct Shape
{
    int4 data0;
    float4 data1;
};

float op(float a, float b, int type, float smooth = 0.1)
{
    float ret = a;

    if (type == OP_UNION) ret = opU(a, b, smooth);
    else if (type == OP_SUBTRACTION) ret = opS(a, b, smooth);
    else if (type == OP_INTERSECTION) ret = opI(a, b, smooth);

    return ret;
}

float sdf(float3 p, Shape s)
{
    if (s.data0.x == SHAPE_SPHERE) return sdfSphere(p, s.data1.xyz, s.data1.w);
    else if (s.data0.x == SHAPE_BOX) return sdfBox(p, s.data1.xyz, float3(1, 1, 1));

    return INFINITY;
}

#endif