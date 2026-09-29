Shader "EthicalLab/Office Sky"
{
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct v2f { float4 pos : SV_POSITION; float3 ray : TEXCOORD0; };
            v2f vert(appdata_base v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.ray = v.vertex.xyz; return o; }
            fixed4 frag(v2f i) : SV_Target
            {
                float3 d = normalize(i.ray);
                float elevation = saturate(d.y);
                float3 sky = lerp(float3(0.79,0.83,0.84), float3(0.32,0.55,0.76), pow(elevation, 0.55));
                float2 p = d.xz / max(0.18, d.y) * 1.6 + _Time.y * float2(0.003,0.001);
                float cloud = sin(p.x + sin(p.y * 0.8)) * 0.5 + sin(p.y * 1.7 + p.x * 0.6) * 0.25;
                cloud = smoothstep(0.16, 0.65, cloud) * smoothstep(0.02, 0.22, elevation) * 0.65;
                sky = lerp(sky, float3(0.96,0.95,0.91), cloud);
                float sun = pow(saturate(dot(d, normalize(float3(-0.65,0.65,-0.35)))), 180);
                return fixed4(sky + sun * float3(0.15,0.12,0.07), 1);
            }
            ENDCG
        }
    }
}
