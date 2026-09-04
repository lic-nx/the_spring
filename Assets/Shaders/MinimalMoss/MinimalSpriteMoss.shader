Shader "Spring/2D/Minimal Sprite Moss"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [Enum(Final,0,FullGreen,1,HeightMask,2,Noise,3)] _DebugMode ("Debug Mode", Float) = 0

        [Header(Moss)]
        _MossColor ("Moss Color", Color) = (0.4, 0.545, 0.157, 1)
        [Range(0, 1)] _MossAmount ("Moss Amount", Float) = 1
        [Range(0, 1)] _MossHeight ("Moss Height", Float) = 0.72
        [Range(0.001, 0.3)] _MossSoftness ("Moss Softness", Float) = 0.04

        [Header(Noise)]
        [NoScaleOffset] _NoiseTex ("Noise Texture", 2D) = "gray" {}
        [Range(0.25, 12)] _NoiseScale ("Noise Scale", Float) = 4
        [Range(0, 0.35)] _NoiseStrength ("Noise Strength", Float) = 0.12
        _Seed ("Seed", Float) = 0

        [HideInInspector] _MossLocalMinY ("Moss Local Min Y", Float) = -0.5
        [HideInInspector] _MossLocalMaxY ("Moss Local Max Y", Float) = 0.5

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
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
            "PreviewType" = "Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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
                float positionOSY : TEXCOORD1;
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
            half _DebugMode;
            half _MossAmount;
            half _MossHeight;
            half _MossSoftness;
            half _NoiseScale;
            half _NoiseStrength;
            half _Seed;
            float _MossLocalMinY;
            float _MossLocalMaxY;
            half _EnableExternalAlpha;

            Varyings MossVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.positionOSY = input.positionOS.y;
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

                if (_DebugMode == 1)
                    return half4(0.0h, 1.0h, 0.0h, sprite.a);

                float localY = saturate(
                    (input.positionOSY - _MossLocalMinY) /
                    max(0.0001, _MossLocalMaxY - _MossLocalMinY)
                );

                float heightMask = smoothstep(
                    _MossHeight - _MossSoftness,
                    _MossHeight + _MossSoftness,
                    localY
                );

                if (_DebugMode == 2)
                    return half4(heightMask.xxx, sprite.a);

                float noise = SAMPLE_TEXTURE2D(
                    _NoiseTex,
                    sampler_NoiseTex,
                    input.uv * _NoiseScale + float2(_Seed, _Seed * 0.37)
                ).r;

                if (_DebugMode == 3)
                    return half4(noise.xxx, sprite.a);

                float distortedY = localY + (noise - 0.5) * _NoiseStrength;
                float mossMask = smoothstep(
                    _MossHeight - _MossSoftness,
                    _MossHeight + _MossSoftness,
                    distortedY
                );

                float3 finalRGB = lerp(
                    sprite.rgb,
                    _MossColor.rgb,
                    mossMask * _MossAmount
                );

                return half4(finalRGB, sprite.a);
            }
            ENDHLSL
        }
    }

    // The project's Graphics/Quality settings currently reference a missing URP
    // asset. This equivalent pass prevents a silent fallback to Sprites/Default.
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
            "PreviewType" = "Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "MinimalSpriteMossBuiltIn"

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex MossVertex
            #pragma fragment MossFragment
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #include "UnityCG.cginc"

            struct Attributes
            {
                float3 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                float positionOSY : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            sampler2D _AlphaTex;
            float4 _MainTex_ST;
            half4 _Color;
            half4 _RendererColor;
            half4 _MossColor;
            half _DebugMode;
            half _MossAmount;
            half _MossHeight;
            half _MossSoftness;
            half _NoiseScale;
            half _NoiseStrength;
            half _Seed;
            float _MossLocalMinY;
            float _MossLocalMaxY;
            half _EnableExternalAlpha;

            Varyings MossVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = UnityObjectToClipPos(input.positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.positionOSY = input.positionOS.y;
                output.color = input.color * _Color * _RendererColor;
                return output;
            }

            half4 SampleSprite(float2 uv)
            {
                half4 sprite = tex2D(_MainTex, uv);

                #if defined(ETC1_EXTERNAL_ALPHA)
                    half externalAlpha = tex2D(_AlphaTex, uv).r;
                    sprite.a = lerp(sprite.a, externalAlpha, _EnableExternalAlpha);
                #endif

                return sprite;
            }

            half4 MossFragment(Varyings input) : SV_Target
            {
                half4 sprite = SampleSprite(input.uv) * input.color;

                if (_DebugMode == 1)
                    return half4(0.0h, 1.0h, 0.0h, sprite.a);

                float localY = saturate(
                    (input.positionOSY - _MossLocalMinY) /
                    max(0.0001, _MossLocalMaxY - _MossLocalMinY)
                );

                float heightMask = smoothstep(
                    _MossHeight - _MossSoftness,
                    _MossHeight + _MossSoftness,
                    localY
                );

                if (_DebugMode == 2)
                    return half4(heightMask.xxx, sprite.a);

                float noise = tex2D(
                    _NoiseTex,
                    input.uv * _NoiseScale + float2(_Seed, _Seed * 0.37)
                ).r;

                if (_DebugMode == 3)
                    return half4(noise.xxx, sprite.a);

                float distortedY = localY + (noise - 0.5) * _NoiseStrength;
                float mossMask = smoothstep(
                    _MossHeight - _MossSoftness,
                    _MossHeight + _MossSoftness,
                    distortedY
                );

                float3 finalRGB = lerp(
                    sprite.rgb,
                    _MossColor.rgb,
                    mossMask * _MossAmount
                );

                return half4(finalRGB, sprite.a);
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
