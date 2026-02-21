Shader "Custom/BalatroBackground"
{
    Properties
    {
        [Header(Pixelation)]
        _PixelFilter("Pixel Density", Float) = 740

        [Header(Animation)]
        _SpinSpeed("Rotation Speed", Range(0, 5)) = 0.3
        _ScrollSpeed("Scroll Speed", Range(0, 10)) = 1.5

        [Header(Pattern)]
        _NoiseScale("Noise Scale", Range(1, 50)) = 8.0
        _Lacunarity("Lacunarity (wave smoothness)", Range(1.1, 3.0)) = 2.0
        _WaveDensity("Wave Density (band packing)", Range(1, 10)) = 1.0
        _SpinAmount("Spiral Strength", Range(0, 1)) = 0.25
        _WarpStrength("Warp Strength", Range(0, 8)) = 4.0
        _CenterBias("Center Bias", Range(0, 3)) = 1.0
        _Contrast("Contrast", Range(0.5, 8)) = 3.5

        [Header(Palette)]
        [Toggle(_USE_GRADIENT)] _UseGradient("Use Gradient Texture", Float) = 0
        _GradientTex("Gradient 2D", 2D) = "white" {}
        [IntRange] _PaletteSize("Palette Size", Range(1, 8)) = 3
        _Palette0("Palette 0", Color) = (0.086, 0.137, 0.145, 1.0)
        _Palette1("Palette 1", Color) = (0.0, 0.42, 0.706, 1.0)
        _Palette2("Palette 2", Color) = (0.871, 0.267, 0.231, 1.0)
        _Palette3("Palette 3", Color) = (0, 0, 0, 1)
        _Palette4("Palette 4", Color) = (0, 0, 0, 1)
        _Palette5("Palette 5", Color) = (0, 0, 0, 1)
        _Palette6("Palette 6", Color) = (0, 0, 0, 1)
        _Palette7("Palette 7", Color) = (0, 0, 0, 1)
        _Lighting("Lighting", Range(0, 1)) = 0.4

        [Header(Offset)]
        _Offset("UV Offset", Vector) = (0, 0, 0, 0)

        // UI Stencil
        [HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "RenderPipeline" = "UniversalPipeline"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "BalatroBackground"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _USE_GRADIENT
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // ================================================================
            // Structs
            // ================================================================

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv        : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 color       : COLOR;
            };

            // ================================================================
            // Properties
            // ================================================================

            CBUFFER_START(UnityPerMaterial)
                float _PixelFilter;
                float _SpinSpeed;
                float _ScrollSpeed;
                float _NoiseScale;
                float _Lacunarity;
                float _WaveDensity;
                float _SpinAmount;
                float _WarpStrength;
                float _CenterBias;
                float _Contrast;
                float _PaletteSize;
                float _Lighting;
                float4 _Offset;
                float4 _Palette0;
                float4 _Palette1;
                float4 _Palette2;
                float4 _Palette3;
                float4 _Palette4;
                float4 _Palette5;
                float4 _Palette6;
                float4 _Palette7;
                float4 _GradientTex_ST;
            CBUFFER_END

            TEXTURE2D(_GradientTex);
            SAMPLER(sampler_GradientTex);

            // ================================================================
            // Procedural Noise (Dave Hoskins hash)
            // ================================================================

            float hash21(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(
                    lerp(hash21(i), hash21(i + float2(1, 0)), u.x),
                    lerp(hash21(i + float2(0, 1)), hash21(i + float2(1, 1)), u.x),
                    u.y
                );
            }

            // Fractional Brownian Motion - 4 octaves
            // lacunarity controls frequency growth per octave
            // lower = bigger smoother waves, higher = more detail
            float fbm(float2 p, float lacunarity)
            {
                float value = 0.0;
                float amp = 0.5;

                for (int i = 0; i < 4; i++)
                {
                    value += amp * valueNoise(p);
                    p *= lacunarity;
                    amp *= 0.5;
                }
                return value;
            }

            // ================================================================
            // Vertex
            // ================================================================

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            // ================================================================
            // Fragment
            // ================================================================

            half4 frag(Varyings input) : SV_Target
            {
                float2 screenSize = _ScreenParams.xy;
                float screenDiag = length(screenSize);

                // --------------------------------------------------------
                // Pixelation: snap screen coords to grid
                // pixel size is relative to screen diagonal = same on all resolutions
                // --------------------------------------------------------
                float pixelSize = screenDiag / _PixelFilter;
                float2 screenCoord = input.uv * screenSize;
                screenCoord = floor(screenCoord / pixelSize) * pixelSize;

                // --------------------------------------------------------
                // Aspect-independent centered UVs
                // Divide by diagonal (not width/height) so pattern is
                // identical regardless of aspect ratio
                // --------------------------------------------------------
                float2 uv = (screenCoord - 0.5 * screenSize) / screenDiag - _Offset.xy;
                float uvLen = length(uv);

                // --------------------------------------------------------
                // Step 5: Rotation (continuous spin)
                // --------------------------------------------------------
                float rotAngle = _Time.y * _SpinSpeed * 0.2 + 302.2;

                // --------------------------------------------------------
                // Step 4: Spiral distortion (atan-based vortex)
                // Stronger near center, fades at edges via SpinAmount
                // --------------------------------------------------------
                float angle = atan2(uv.y, uv.x);
                float spiralFactor = _SpinAmount * uvLen + (1.0 - _SpinAmount);
                float newAngle = angle + rotAngle - 20.0 * spiralFactor;

                float2 mid = (screenSize / screenDiag) * 0.5;
                uv = float2(
                    uvLen * cos(newAngle) + mid.x,
                    uvLen * sin(newAngle) + mid.y
                ) - mid;

                // --------------------------------------------------------
                // Step 1: Scrolling noise coordinates
                // --------------------------------------------------------
                float2 p = uv * _NoiseScale;
                float t = _Time.y * _ScrollSpeed;

                // --------------------------------------------------------
                // Step 2: Procedural marbling via domain warping
                // (iquilezles.org/articles/warp/)
                //
                // Level 1: q = (fbm(p + offset_a), fbm(p + offset_b))
                // Level 2: r = (fbm(p + W*q + offset_c), fbm(p + W*q + offset_d))
                // Result:  fbm(p + W*r)
                //
                // Time offsets on each fbm call create scrolling animation
                // --------------------------------------------------------
                float lac = _Lacunarity;

                float2 q = float2(
                    fbm(p + float2(0.0, 0.0) + float2(t * 0.12, t * 0.08), lac),
                    fbm(p + float2(5.2, 1.3) + float2(t * 0.09, t * 0.11), lac)
                );

                float2 r = float2(
                    fbm(p + _WarpStrength * q + float2(1.7, 9.2) + float2(t * 0.06, 0.0), lac),
                    fbm(p + _WarpStrength * q + float2(8.3, 2.8) + float2(0.0, t * 0.07), lac)
                );

                float pattern = fbm(p + _WarpStrength * r, lac);

                // --------------------------------------------------------
                // Wave density: fold the smooth pattern to pack more
                // color bands into the same large waves
                // frac(x * density) creates repeating sawtooth from smooth noise
                // --------------------------------------------------------
                pattern = frac(pattern * _WaveDensity);

                // --------------------------------------------------------
                // Step 3: Center bias
                // Quadratic falloff makes center brighter / more prominent
                // --------------------------------------------------------
                float centerFalloff = 1.0 - saturate(uvLen * _CenterBias);
                centerFalloff *= centerFalloff;
                pattern = pattern * (0.6 + 0.4 * centerFalloff) + 0.25 * centerFalloff;

                // --------------------------------------------------------
                // Step 6: Color output — hard color bands, no lerp
                // --------------------------------------------------------

                int count = (int)_PaletteSize;
                half4 palette[8] = {
                    _Palette0, _Palette1, _Palette2, _Palette3,
                    _Palette4, _Palette5, _Palette6, _Palette7
                };

                #ifdef _USE_GRADIENT
                    float gradX = saturate(pattern);
                    float gradY = saturate(uvLen * 2.0);
                    half4 finalColor = SAMPLE_TEXTURE2D(_GradientTex, sampler_GradientTex, float2(gradX, gradY));
                #else
                    float contrastMod = 0.25 * _Contrast + 0.5 * _SpinAmount + 1.2;
                    float paintRes = saturate(pattern * contrastMod);
                    int idx = clamp((int)(paintRes * count), 0, count - 1);
                    half4 finalColor = palette[idx];
                    finalColor.rgb += _Lighting * step(count - 1, idx) * 0.15;
                #endif

                // Snap to nearest palette color
                half3 snapped = palette[0].rgb;
                float minDist = distance(finalColor.rgb, palette[0].rgb);
                for (int j = 1; j < count; j++)
                {
                    float d = distance(finalColor.rgb, palette[j].rgb);
                    if (d < minDist)
                    {
                        minDist = d;
                        snapped = palette[j].rgb;
                    }
                }
                finalColor.rgb = snapped;

                finalColor.a *= input.color.a;
                return finalColor;
            }

            ENDHLSL
        }
    }

    Fallback "UI/Default"
}
