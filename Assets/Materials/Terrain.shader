Shader "Custom/Terrain"
{
    Properties
    {
        _Ground1 ("Ground 1", Color) = (1, 1, 1, 1)
        _Ground1Dark ("Ground 1 Dark", Color) = (1, 1, 1, 1)
        _Ground2 ("Ground 2", Color) = (1, 1, 1, 1)
        _Ground2Dark ("Ground 2 Dark", Color) = (1, 1, 1, 1)
        _Ceiling1 ("Ceiling 1", Color) = (1, 1, 1, 1)
        _Ceiling1Dark ("Ceiling 1 Dark", Color) = (1, 1, 1, 1)
        _Ceiling2 ("Ceiling 2", Color) = (1, 1, 1, 1)
        _Ceiling2Dark ("Ceiling 2 Dark", Color) = (1, 1, 1, 1)

        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5

        _NoiseTex ("Noise Texture", 2D) = "White" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 worldPos;
        };

        half _Glossiness;
        fixed4 _Color;

        float4 _Ground1;
        float4 _Ground1Dark;
        float4 _Ground2;
        float4 _Ground2Dark;
        float4 _Ceiling1;
        float4 _Ceiling1Dark;
        float4 _Ceiling2;
        float4 _Ceiling2Dark;

        sampler2D _NoiseTex;

        float minHeight;
        float maxHeight;
        float textureScale;

        float4 triplanarMapping(float3 position, float3 normal, float scale, sampler2D tex)
        {
            float3 scaledPosition = position / scale;
            
            float4 xColor = tex2D(tex, scaledPosition.zy);
            float4 yColor = tex2D(tex, scaledPosition.xz);
            float4 zColor = tex2D(tex, scaledPosition.xy);

            float3 blend = normal * normal;
            blend /= dot(blend, 1);
            
            return xColor * blend.x + yColor * blend.y + zColor * blend.z;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float4 noise = triplanarMapping(IN.worldPos, IN.worldNormal, textureScale, _NoiseTex);

            if (IN.worldNormal.y > 0.5) {
                if (IN.worldPos.y < minHeight)
                    o.Albedo = lerp(_Ground1, _Ground1Dark, noise.r);
                else
                    o.Albedo = lerp(_Ground2, _Ground2Dark, noise.r);
            } else if (IN.worldNormal.y < -0.5) {
                if (IN.worldPos.y > maxHeight)
                    o.Albedo = lerp(_Ceiling1, _Ceiling1Dark, noise.r);
                else
                    o.Albedo = lerp(_Ceiling2, _Ceiling2Dark, noise.r);
            } else {
                float4 wallColorLight = lerp(_Ground1, _Ceiling1, (IN.worldPos.y - minHeight) / (maxHeight - minHeight));
                float4 wallColorDark = lerp(_Ground1Dark, _Ceiling1Dark, (IN.worldPos.y - minHeight) / (maxHeight - minHeight));
                o.Albedo = lerp(wallColorLight, wallColorDark, noise.r);
            }

            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
