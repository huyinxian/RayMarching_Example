Shader "Hidden/RayMarching"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // 不需要做剔除和深度写入
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Shapes.cginc"

            uniform float4x4 _FrustumCornersES;
            uniform sampler2D _MainTex;
            uniform float4 _MainTex_TexelSize;
            uniform float4x4 _CameraInvViewMatrix;
            uniform float3 _CameraWS;
            uniform float3 _LightDir;
            uniform sampler2D _CameraDepthTexture;
            uniform int _ShapesCount;
            uniform int4 _ShapesData0Arr[100];
            uniform float4 _ShapesData1Arr[100];
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 ray : TEXCOORD1;
            };
            
            float map(float3 p)
            {
                float ret = INFINITY;

                Shape s;
                s.data0 = _ShapesData0Arr[0];
                s.data1 = _ShapesData1Arr[0];
                ret = op(ret, sdf(p, s), s.data0.y);
                s.data0 = _ShapesData0Arr[1];
                s.data1 = _ShapesData1Arr[1];
                ret = op(ret, sdf(p, s), s.data0.y);

                return ret;
            }
            
            float3 calcNormal(in float3 pos)
            {
                // epsilon
                const float2 eps = float2(0.001, 0.0);
            
                // 利用距离场的梯度近似计算表面法线
                float3 nor = float3(
                    map(pos + eps.xyy).x - map(pos - eps.xyy).x,
                    map(pos + eps.yxy).x - map(pos - eps.yxy).x,
                    map(pos + eps.yyx).x - map(pos - eps.yyx).x);
                return normalize(nor);
            }
            
            fixed4 rayMarching(float3 ro, float3 rd, float s)
            {
                fixed4 ret = fixed4(0,0,0,0);
            
                const int maxstep = 64;     // 最大步进次数
                const float drawdist = 40;  // 绘制距离限制
                
                // 步进的总长度
                float t = 0;
                for (int i = 0; i < maxstep; ++i)
                {
                    // 判断深度值，保证遮挡关系正确
                    // 超过最大距离时也不需要绘制
                    if (t >= s || t > drawdist)
                    {
                        // 如果RayMarching被遮挡，那么就将透明度设为0
                        ret = fixed4(0, 0, 0, 0);
                        break;
                    }
                    
                    float3 p = ro + rd * t; // 当前的步进点在世界空间下的坐标
                    float d = map(p);       // SDF
            
                    // 当返回值小于某个常数值时，视作是击中了表面
                    if (d < 0.001)
                    {
                        // 光照模型
                        float3 n = calcNormal(p);
                        ret = fixed4(dot(-_LightDir.xyz, n).rrr, 1);
                        break;
                    }
            
                    // 没有击中表面，继续步进
                    t += d;
                }
                return ret;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // vertex的z值被C#脚本设置成了_FrustumCornersES的索引
                half index = v.vertex.z;
                v.vertex.z = 0.1;
                
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv.xy;
                
                #if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0)
                    o.uv.y = 1 - o.uv.y;
                #endif
            
                // 获取视空间下的ViewRay，即该顶点对应的视锥体的侧棱
                o.ray = _FrustumCornersES[(int)index].xyz;
                
                o.ray /= abs(o.ray.z);
            
                // 从视空间转换到世界空间
                o.ray = mul(_CameraInvViewMatrix, o.ray);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 经过GPU插值后，片元着色器中的ray就是从相机朝着像素发射的射线
                float3 rd = normalize(i.ray.xyz);
                float3 ro = _CameraWS;
            
                float2 duv = i.uv;
                #if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0)
                    duv.y = 1 - duv.y;
                #endif
            
                // 采样深度图
                float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, duv).r);
                depth *= length(i.ray.xyz);
            
                fixed3 col = tex2D(_MainTex, i.uv);
                fixed4 add = rayMarching(ro, rd, depth);
            
                // 将RayMarching的结果与正常渲染的画面进行透明度混合
                return fixed4(col * (1.0 - add.w) + add.xyz * add.w, 1.0);
            }
            ENDCG
        }
    }
}
