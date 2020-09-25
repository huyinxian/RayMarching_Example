using System;
using UnityEngine;

public enum RayMarchingShapeType
{
    Sphere,
    Box,
    Capsule,
    Torus
}

public enum RayMarchingOperator
{
    Union,
    Subtraction,
    Intersection
}

[ExecuteInEditMode]
public class RayMarchingShape : MonoBehaviour
{
    public RayMarchingShapeType Type;
    public RayMarchingOperator Operator;

    private void Start()
    {
        RayMarchingMaster.AddRayMarchingShape(this);
    }

    private void OnDestroy()
    {
        RayMarchingMaster.RemoveRayMarchingShape(this);
    }
}