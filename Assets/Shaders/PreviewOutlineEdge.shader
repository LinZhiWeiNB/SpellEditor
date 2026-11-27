Shader "GameContentEditor/PreviewOutlineEdge"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }
        Pass
        {
            Name "Edge"
            Cull Off
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            SAMPLER(sampler_BlitTexture);

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
                float4 center = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);
                if (center.a > 0.5)
                    discard;

                float2 texel = _BlitTexture_TexelSize.xy;
                float2 offsets[8] =
                {
                    float2(1, 0), float2(-1, 0), float2(0, 1), float2(0, -1),
                    float2(1, 1), float2(1, -1), float2(-1, 1), float2(-1, -1)
                };
                float4 found = 0;
                float best = 4;
                [unroll]
                for (int i = 0; i < 8; i++)
                {
                    [unroll]
                    for (int radius = 1; radius <= 3; radius++)
                    {
                        float4 hit = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + offsets[i] * texel * radius);
                        if (hit.a > 0.5 && radius < best)
                        {
                            best = radius;
                            found = hit;
                        }
                    }
                }

                if (found.a < 0.5)
                    discard;
                return found;
            }
            ENDHLSL
        }
    }
}
