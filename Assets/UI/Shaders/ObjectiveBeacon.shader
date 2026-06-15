Shader "Custom/ObjectiveBeacon"
{
    Properties
    {
        _Color ("Color", Color) = (0.25, 1, 0.35, 0.85)
        _TopFade ("Top Fade Power", Range(0.5, 4)) = 1.8
        _BottomBoost ("Bottom Boost", Range(0, 1)) = 0.35
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _TopFade;
                float _BottomBoost;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float height01 : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.height01 = saturate(input.positionOS.y + 0.5);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float fade = pow(1.0 - input.height01, _TopFade);
                float bottom = lerp(1.0, 1.0 + _BottomBoost, 1.0 - input.height01);
                half alpha = _Color.a * fade * bottom;
                return half4(_Color.rgb * bottom, alpha);
            }
            ENDHLSL
        }
    }
}
