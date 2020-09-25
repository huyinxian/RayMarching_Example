using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Effects/Raymarch (Generic)")]
public class RayMarchingMaster : SceneViewFilter
{
    private static List<RayMarchingShape> _shapes = new List<RayMarchingShape>();

    [SerializeField]
    private Shader _EffectShader;

    public Material EffectMaterial
    {
        get
        {
            if (!_EffectMaterial && _EffectShader)
            {
                _EffectMaterial = new Material(_EffectShader);
                _EffectMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            return _EffectMaterial;
        }
    }
    private Material _EffectMaterial;

    public Camera CurrentCamera
    {
        get
        {
            if (!_CurrentCamera)
                _CurrentCamera = GetComponent<Camera>();
            return _CurrentCamera;
        }
    }
    private Camera _CurrentCamera;
    
    public Transform SunLight;
    
    public static void AddRayMarchingShape(RayMarchingShape shape)
    {
        if (_shapes?.IndexOf(shape) < 0)
        {
            _shapes.Add(shape);
        }
    }

    public static void RemoveRayMarchingShape(RayMarchingShape shape)
    {
        _shapes?.Remove(shape);
    }

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!EffectMaterial)
        {
            Graphics.Blit(source, destination);
            return;
        }
        
        var shapesData0Arr = new Vector4[_shapes.Count];
        var shapesData1Arr = new Vector4[_shapes.Count];
        for (int i = 0; i < _shapes.Count; i++)
        {
            var shape = _shapes[i];
            var shapePos = shape.transform.position;
            shapesData0Arr[i] = new Vector4((float)shape.Type, (float)shape.Operator);
            shapesData1Arr[i] = new Vector4(shapePos.x, shapePos.y, shapePos.z, shape.transform.localScale.x);
        }

        EffectMaterial.SetMatrix("_FrustumCornersES", GetFrustumCorners(CurrentCamera));
        EffectMaterial.SetMatrix("_CameraInvViewMatrix", CurrentCamera.cameraToWorldMatrix);
        EffectMaterial.SetVector("_CameraWS", CurrentCamera.transform.position);
        EffectMaterial.SetVector("_LightDir", SunLight ? SunLight.forward : Vector3.down);
        if (_shapes.Count > 0)
        {
            EffectMaterial.SetInt("_ShapesCount", _shapes.Count);
            EffectMaterial.SetVectorArray("_ShapesData0Arr", shapesData0Arr);
            EffectMaterial.SetVectorArray("_ShapesData1Arr", shapesData1Arr);
        }

        CustomGraphicsBlit(source, destination, EffectMaterial, 0);
    }
    
    /// <summary>
    /// 计算视锥体的四条侧棱，并将结果存放在一个矩阵中
    /// </summary>
    /// <param name="cam"></param>
    /// <returns></returns>
    private Matrix4x4 GetFrustumCorners(Camera cam)
    {
        float camFov = cam.fieldOfView;
        float camAspect = cam.aspect;

        Matrix4x4 frustumCorners = Matrix4x4.identity;

        float fovWHalf = camFov * 0.5f;

        float tan_fov = Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

        Vector3 toRight = Vector3.right * tan_fov * camAspect;
        Vector3 toTop = Vector3.up * tan_fov;

        Vector3 topLeft = (-Vector3.forward - toRight + toTop);
        Vector3 topRight = (-Vector3.forward + toRight + toTop);
        Vector3 bottomRight = (-Vector3.forward + toRight - toTop);
        Vector3 bottomLeft = (-Vector3.forward - toRight - toTop);

        frustumCorners.SetRow(0, topLeft);
        frustumCorners.SetRow(1, topRight);
        frustumCorners.SetRow(2, bottomRight);
        frustumCorners.SetRow(3, bottomLeft);

        return frustumCorners;
    }
    
    static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr)
    {
        RenderTexture.active = dest;

        fxMaterial.SetTexture("_MainTex", source);

        GL.PushMatrix();
        GL.LoadOrtho();

        fxMaterial.SetPass(passNr);

        GL.Begin(GL.QUADS);

        // 绘制屏幕大小的四边形
        // 由于GraphicsBlit是用正交投影绘制的，所以四边形顶点的z值就不需要了，可以用来存储该顶点对应的_FrustumCornersES矩阵的取值索引
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f); // BL

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f); // BR

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f); // TR

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f); // TL
    
        GL.End();
        GL.PopMatrix();
    }
}