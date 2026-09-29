Shader "EthicalLab/Cel"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Optional clothing albedo", 2D) = "white" {}
        _TextureScale ("Fabric repeats per metre", Float) = 4
        _TextureStrength ("Fabric detail", Range(0,1)) = 0
        _Steps ("Light steps", Range(2, 4)) = 3
        _OutlineColor ("Outline", Color) = (0.18, 0.12, 0.08, 1)
        _OutlineWidth ("Outline width", Range(0, 0.02)) = 0.0011
        _Rim ("Rim", Range(0, 1)) = 0.1
        [HDR] _EmissionColor ("Emission", Color) = (0, 0, 0, 1)
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    sampler2D _MainTex;
    float _TextureScale;
    float _TextureStrength;

    float3 ClothingTexture(float3 worldPos, float3 worldNormal)
    {
        if (_TextureStrength <= 0) return float3(1, 1, 1);
        float3 scale = float3(length(unity_ObjectToWorld._m00_m10_m20),
            length(unity_ObjectToWorld._m01_m11_m21), length(unity_ObjectToWorld._m02_m12_m22));
        float3 p = mul(unity_WorldToObject, float4(worldPos, 1)).xyz * scale * max(_TextureScale, 0.01);
        float3 n = normalize(mul((float3x3)unity_WorldToObject, worldNormal) * scale);
        float3 blend = pow(abs(n), 4);
        blend /= max(blend.x + blend.y + blend.z, 0.0001);
        float3 albedo = tex2D(_MainTex, p.zy).rgb * blend.x
            + tex2D(_MainTex, p.xz).rgb * blend.y + tex2D(_MainTex, p.xy).rgb * blend.z;
        float3 average = tex2Dlod(_MainTex, float4(0.5, 0.5, 0, 16)).rgb;
        return lerp(float3(1, 1, 1), albedo / max(average, float3(0.15, 0.15, 0.15)), _TextureStrength);
    }
    ENDCG

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 200

        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }
            Cull Front
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _OutlineWidth;
            fixed4 _OutlineColor;
            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float3 viewNormal = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                float2 offset = TransformViewToProjection(viewNormal.xy);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.pos.xy += offset * _OutlineWidth * o.pos.w;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Ancho cero: las manos no dibujan el cascarón. El resto usa el propio color, más oscuro.
                if (_OutlineWidth < 0.00015) discard;
                fixed3 ink = _Color.rgb * 0.32 + _OutlineColor.rgb * 0.12;
                return fixed4(ink, 1);
            }
            ENDCG
        }

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            fixed4 _Color;
            fixed4 _OutlineColor;
            fixed4 _EmissionColor;
            float _Steps;
            float _Rim;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                UNITY_LIGHTING_COORDS(2, 3)
            };

            float CelBand(float value, float steps)
            {
                float count = clamp(steps, 2.0, 4.0);
                return floor(min(saturate(value), 0.999) * count) / (count - 1.0);
            }

            float3 CelVertexLights(float3 worldPos, float3 worldNormal)
            {
                float3 sum = 0;
                #ifdef VERTEXLIGHT_ON
                float3 viewPos = mul(UNITY_MATRIX_V, float4(worldPos, 1)).xyz;
                float3 viewNormal = normalize(mul((float3x3)UNITY_MATRIX_V, worldNormal));
                for (int lamp = 0; lamp < 4; lamp++)
                {
                    float3 toLight = unity_LightPosition[lamp].xyz - viewPos * unity_LightPosition[lamp].w;
                    float lengthSq = max(dot(toLight, toLight), 0.000001);
                    float ndotl = saturate(dot(viewNormal, toLight) * rsqrt(lengthSq));
                    float atten = 1.0 / (1.0 + lengthSq * unity_LightAtten[lamp].z);
                    sum += unity_LightColor[lamp].rgb * CelBand(ndotl * atten, _Steps);
                }
                #endif
                return sum;
            }

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                UNITY_TRANSFER_LIGHTING(o, v.texcoord.xy);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);
                float3 lightVec = _WorldSpaceLightPos0.xyz - i.worldPos * _WorldSpaceLightPos0.w;
                float ndotl = saturate(dot(normal, normalize(lightVec)));
                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                float band = CelBand(ndotl * atten, _Steps);
                float3 lighting = UNITY_LIGHTMODEL_AMBIENT.rgb + _LightColor0.rgb * band * 0.72 + CelVertexLights(i.worldPos, normal);
                lighting = min(lighting, 1.2);
                float3 albedo = _Color.rgb * ClothingTexture(i.worldPos, normal);
                float3 color = albedo * lighting + _EmissionColor.rgb;
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                float fresnel = 1.0 - saturate(dot(normal, viewDir));
                float rim = smoothstep(0.68, 0.94, fresnel);
                color = lerp(color, albedo * lighting * 0.62, rim * _Rim);
                return fixed4(color, 1);
            }
            ENDCG
        }

        Pass
        {
            Name "FORWARD_ADD"
            Tags { "LightMode" = "ForwardAdd" }
            Cull Back
            Blend One One
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            fixed4 _Color;
            float _Steps;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                UNITY_LIGHTING_COORDS(2, 3)
            };

            float CelBand(float value, float steps)
            {
                float count = clamp(steps, 2.0, 4.0);
                return floor(min(saturate(value), 0.999) * count) / (count - 1.0);
            }

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                UNITY_TRANSFER_LIGHTING(o, v.texcoord.xy);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);
                float3 lightVec = _WorldSpaceLightPos0.xyz - i.worldPos * _WorldSpaceLightPos0.w;
                float ndotl = saturate(dot(normal, normalize(lightVec)));
                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                float band = CelBand(ndotl * atten, _Steps);
                return fixed4(_Color.rgb * ClothingTexture(i.worldPos, normal) * _LightColor0.rgb * band * 0.42, 1);
            }
            ENDCG
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f
            {
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }

    Fallback "Diffuse"
}
