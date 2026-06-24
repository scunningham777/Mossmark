Shader "Mossmark/EdgeBlur"
{
    Properties
    {
        _FalloffStart  ("Falloff Start",         Range(0, 1)) = 0.4
        _FalloffEnd    ("Falloff End",            Range(0, 1)) = 0.9
        _MaxBlurRadius ("Max Blur Radius (px)",   Float)       = 12
        _SampleCount   ("Sample Count",           Float)       = 24
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off

        Pass
        {
            Name "EdgeBlur"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragEdgeBlur

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _FalloffStart;
                float _FalloffEnd;
                float _MaxBlurRadius;
                float _SampleCount;
            CBUFFER_END

            // Golden angle (radians) — distributes samples evenly across the disk
            static const float GOLDEN_ANGLE = 2.39996323;

            half4 FragEdgeBlur(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;

                // Radial distance from screen center: 0 = center, 1 = midpoint of edges
                float dist = length(uv - 0.5) * 2.0;
                float blurFactor = saturate((dist - _FalloffStart) / max(_FalloffEnd - _FalloffStart, 0.001));
                blurFactor *= blurFactor; // quadratic: sharp center, heavy edges

                [branch]
                if (blurFactor < 0.001)
                    return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                // _ScreenParams.xy = (width, height) — always set by Unity, unlike _BlitTexture_TexelSize
                float2 maxRadius = _MaxBlurRadius * blurFactor / _ScreenParams.xy;

                half4 color = 0;
                int count = (int)_SampleCount;
                [loop] for (int i = 0; i < count; i++)
                {
                    float fi = (float)i;
                    float angle = fi * GOLDEN_ANGLE;
                    float r = sqrt((fi + 0.5) / _SampleCount); // sqrt = uniform area distribution
                    float2 offset = float2(cos(angle), sin(angle)) * r * maxRadius;
                    color += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + offset);
                }
                return color / _SampleCount;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
