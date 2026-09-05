Shader "Skybox/CityPanorama"
{
    Properties
    {
        _MainTex ("Panorama (LatLong)", 2D) = "grey" {}
        _Tint ("Tint", Color) = (0.5, 0.5, 0.5, 0.5)
        _Exposure ("Exposure", Range(0, 8)) = 1.05
        _Rotation ("Rotation", Range(0, 360)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Background"
            "PreviewType" = "Skybox"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Tint;
                half _Exposure;
                float _Rotation;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 viewDir : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float3 RotateAroundY(float3 dir, float degrees)
            {
                float rad = degrees * 0.01745329251;
                float s = sin(rad);
                float c = cos(rad);
                return float3(c * dir.x + s * dir.z, dir.y, -s * dir.x + c * dir.z);
            }

            float2 ToLatLongUV(float3 dir)
            {
                dir = normalize(dir);
                float longitude = atan2(dir.z, dir.x);
                float latitude = acos(clamp(dir.y, -1.0, 1.0));
                float2 sphereCoords = float2(longitude, latitude) * float2(0.5 / 3.14159265359, 1.0 / 3.14159265359);
                return float2(0.5, 1.0) - sphereCoords;
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float4 positionCS = TransformWorldToHClip(positionWS);
#if UNITY_REVERSED_Z
                positionCS.z = positionCS.w * 0.0000001;
#else
                positionCS.z = positionCS.w * 0.999999;
#endif
                output.positionCS = positionCS;
                output.viewDir = input.positionOS.xyz;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float3 dir = RotateAroundY(input.viewDir, _Rotation);
                float2 uv = ToLatLongUV(dir);
                half3 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;
                color *= _Tint.rgb * 2.0 * _Exposure;
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
