Shader "Spring/2D/Minimal Sprite Moss"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        [Header(Moss Colors)]
        _MossColor ("Moss Color", Color) = (0.4, 0.545, 0.157, 1)
        _MossHighlightColor ("Moss Highlight Color", Color) = (0.596, 0.722, 0.243, 1)

        [Header(Moss Shape)]
        [Range(0, 1)] _MossAmount ("Moss Amount", Float) = 0.8
        [Range(0, 1)] _MossHeight ("Moss Height", Float) = 0.78
        [Range(0.001, 0.3)] _MossSoftness ("Moss Softness", Float) = 0.12

        [Header(Noise)]
        [NoScaleOffset] _NoiseTex ("Noise Texture", 2D) = "gray" {}
        [Range(0.25, 12)] _NoiseScale ("Noise Scale", Float) = 4
        [Range(0, 0.35)] _NoiseStrength ("Noise Strength", Float) = 0.12
        _Seed ("Seed", Float) = 0

        [Header(Depth)]
        [Range(0, 0.15)] _MossShadow ("Moss Shadow", Float) = 0.06

        // SpriteRenderer compatibility properties.
        [HideInInspector] _Color ("Tint", Color) = (1, 1, 1, 1)
        [HideInInspector] _RendererColor ("Renderer Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _Flip ("Flip", Vector) = (1, 1, 1, 1)
        [PerRendererData] [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
            "PreviewType" = "Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Name "MinimalSpriteMoss"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex MossVertex
            #pragma fragment MossFragment
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);
            TEXTURE2D(_AlphaTex);
            SAMPLER(sampler_AlphaTex);

            float4 _MainTex_ST;
            half4 _Color;
            half4 _RendererColor;
            half4 _MossColor;
            half4 _MossHighlightColor;
            half _MossAmount;
            half _MossHeight;
            half _MossSoftness;
            half _NoiseScale;
            half _NoiseStrength;
            half _Seed;
            half _MossShadow;
            half _EnableExternalAlpha;

            Varyings MossVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color * _RendererColor;
                return output;
            }

            half4 SampleSprite(float2 uv)
            {
                half4 sprite = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                #if defined(ETC1_EXTERNAL_ALPHA)
                    half externalAlpha = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, uv).r;
                    sprite.a = lerp(sprite.a, externalAlpha, _EnableExternalAlpha);
                #endif

                return sprite;
            }

            half4 MossFragment(Varyings input) : SV_Target
            {
                half4 sprite = SampleSprite(input.uv) * input.color;

                float2 noiseUV = input.uv * _NoiseScale + float2(_Seed, _Seed * 0.37);
                half noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, frac(noiseUV)).r;

                // Centering the noise keeps Moss Height intuitive while breaking the flat border.
                half distortedHeight = input.uv.y + (noise - 0.5h) * _NoiseStrength;
                half softness = max(_MossSoftness, 0.001h);
                half topMask = smoothstep(
                    _MossHeight - softness,
                    _MossHeight + softness,
                    distortedHeight
                );

                // A single extra mask makes small gaps near the lower edge, but keeps the top solid.
                half noiseMask = smoothstep(0.2h, 0.55h, noise + topMask * 0.35h);
                half mossMask = saturate(topMask * noiseMask);

                // Thin hand-painted highlight along the irregular lower moss boundary.
                half highlightMask = smoothstep(
                    _MossHeight - softness,
                    _MossHeight,
                    distortedHeight
                ) - smoothstep(
                    _MossHeight,
                    _MossHeight + softness,
                    distortedHeight
                );
                highlightMask = saturate(highlightMask) * noiseMask;

                half highlightAmount = saturate(noise * 0.35h + highlightMask * 0.8h);
                half3 mossColor = lerp(_MossColor.rgb, _MossHighlightColor.rgb, highlightAmount);

                // Narrow contact shadow directly below the moss; no blur or extra texture sample.
                half shadowStart = _MossHeight - softness * 1.35h;
                half shadowEnd = _MossHeight - softness;
                half shadowMask = smoothstep(shadowStart, shadowEnd, distortedHeight);
                shadowMask *= 1.0h - smoothstep(0.0h, 0.25h, topMask);
                sprite.rgb *= 1.0h - saturate(shadowMask) * _MossShadow * _MossAmount;

                half mossBlend = saturate(mossMask * _MossAmount) * sprite.a;
                half3 finalColor = lerp(sprite.rgb, mossColor, mossBlend);

                // The original sprite alpha owns the silhouette.
                return half4(finalColor, sprite.a);
            }
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/2D/Sprite-Unlit-Default"
}
