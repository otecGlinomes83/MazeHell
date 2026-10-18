Shader "MazeHell/Environment Unlit"
{
    Properties
    {
        _BaseMap("Texture", 2D) = "white" {}
        _BaseColor("Tint", Color) = (0.75, 0.95, 1.05, 1)
        _Saturation("Saturation", Range(0, 1)) = 0.08
        _Brightness("Brightness", Range(0, 3)) = 1.4
        _GrimeMap("Blood And Skulls", 2D) = "black" {}
        _GrimeAmount("Corruption", Range(0, 1)) = 0
        _GrimeScale("Corruption Tile Size", Float) = 8
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_GrimeMap);
            SAMPLER(sampler_GrimeMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Saturation;
                half _Brightness;
                half _GrimeAmount;
                float _GrimeScale;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };
            Varyings Vertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }
            half4 Fragment(Varyings input) : SV_Target
            {
                half3 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;
                half luminance = dot(color, half3(0.2126, 0.7152, 0.0722));
                color = lerp(luminance.xxx, color, _Saturation) * _BaseColor.rgb * _Brightness;
                float2 grimeUV = input.positionWS.xy;
                if (abs(input.normalWS.x) > abs(input.normalWS.z))
                {
                    grimeUV = input.positionWS.zy;
                }
                half4 grime = SAMPLE_TEXTURE2D(_GrimeMap, sampler_GrimeMap, grimeUV / max(_GrimeScale, 0.01));
                color = lerp(color, grime.rgb * _Brightness, grime.a * _GrimeAmount);
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
