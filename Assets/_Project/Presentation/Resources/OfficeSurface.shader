Shader "EthicalLab/Office Surface"
{
    Properties
    {
        _Color ("Tint", Color) = (1,1,1,1)
        _MainTex ("Optional authored albedo", 2D) = "white" {}
        _TextureScale ("Texture repeats per metre", Float) = 1
        _TextureStrength ("Texture detail", Range(0,1)) = 1
        _Glossiness ("Smoothness", Range(0,1)) = 0.32
        _Metallic ("Metallic", Range(0,1)) = 0
        [Enum(Plain,0,Wood,1,Fabric,2,Plaster,3)] _Pattern ("Surface finish", Float) = 0
        _GrainStrength ("Subtle surface variation", Range(0,0.3)) = 0.1
        [HDR] _EmissionColor ("Emission", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        #pragma multi_compile_instancing
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        fixed4 _Color;
        half _Glossiness, _Metallic, _GrainStrength, _Pattern, _TextureScale, _TextureStrength;
        half4 _EmissionColor;
        struct Input { float3 worldPos; float3 worldNormal; };

        // Object-local metres follow movable furniture without stretching textures.
        // The last mip gives the image average, preserving the existing art palette.
        half3 SurfaceTexture(float3 p, float3 worldNormal, float3 scale)
        {
            if (_TextureStrength <= 0) return half3(1, 1, 1);
            float3 n = normalize(mul((float3x3)unity_WorldToObject, worldNormal) * scale);
            float3 blend = pow(abs(n), 4);
            blend /= max(blend.x + blend.y + blend.z, 0.0001);
            float3 tiled = p * max(_TextureScale, 0.01);
            half3 sampleX = tex2D(_MainTex, tiled.zy).rgb;
            half3 sampleY = tex2D(_MainTex, tiled.zx).rgb;
            half3 sampleZ = tex2D(_MainTex, tiled.yx).rgb;
            half3 albedo = sampleX * blend.x + sampleY * blend.y + sampleZ * blend.z;
            half3 average = tex2Dlod(_MainTex, float4(0.5, 0.5, 0, 16)).rgb;
            return lerp(half3(1, 1, 1), albedo / max(average, half3(0.15, 0.15, 0.15)), _TextureStrength);
        }

        // Filtrar las vetas finas al alejarnos evita parpadeo sin texturas pesadas.
        float FilteredWave(float phase)
        {
            return sin(phase) * (1.0 - smoothstep(0.7, 3.0, fwidth(phase)));
        }

        void surf(Input i, inout SurfaceOutputStandard o)
        {
            float3 scale = float3(length(unity_ObjectToWorld._m00_m10_m20),
                length(unity_ObjectToWorld._m01_m11_m21), length(unity_ObjectToWorld._m02_m12_m22));
            float3 p = mul(unity_WorldToObject, float4(i.worldPos, 1)).xyz * scale;
            float grain = 0;
            if (_Pattern > 0.5 && _Pattern < 1.5)
            {
                float warp = sin(p.x * 2.7 + p.y * 1.6) * 1.7 + sin(p.x * 7.1) * 0.22;
                grain = FilteredWave(p.z * 105 + p.y * 61 + warp) * 0.55
                    + FilteredWave(p.z * 290 + p.y * 190 + warp * 2) * 0.15;
            }
            else if (_Pattern > 1.5 && _Pattern < 2.5)
            {
                grain = FilteredWave((p.x + p.y) * 260) * FilteredWave((p.z + p.y) * 240) * 0.6
                    + FilteredWave((p.x + p.z) * 43) * 0.18;
            }
            else if (_Pattern > 2.5)
                grain = FilteredWave(dot(p, float3(117,161,97))) * FilteredWave(dot(p, float3(71,113,149)));
            o.Albedo = saturate(SurfaceTexture(p, i.worldNormal, scale) * _Color.rgb * (1 + grain * _GrainStrength));
            o.Smoothness = saturate(_Glossiness + grain * _GrainStrength * 0.1);
            o.Metallic = _Metallic;
            o.Emission = _EmissionColor.rgb;
            o.Occlusion = 1;
            o.Alpha = 1;
        }
        ENDCG
    }
    Fallback "Standard"
}
