Shader "Custom/SpaceWavesBackground"
{
    Properties
    {
        [Header(Pixelation)]
        _PixelFilter("Pixel Density", Float) = 740

        [Header(Animation)]
        _ScrollSpeed("Scroll Speed", Range(0, 10)) = 1.0
        _FlowAngle("Flow Direction (degrees)", Range(0, 360)) = 30.0

        [Header(Waves)]
        _NoiseScale("Noise Scale", Range(1, 50)) = 6.0
        _Lacunarity("Lacunarity (wave smoothness)", Range(1.1, 3.0)) = 1.6
        _WaveDensity("Wave Density (band packing)", Range(1, 10)) = 2.0
        _WaveFrequency("Wave Frequency", Range(0.5, 10)) = 3.0
        _WaveAmplitude("Wave Amplitude", Range(0, 1)) = 0.3
        _WaveLayers("Wave Layers", Range(1, 5)) = 3
        _WarpStrength("Warp Strength", Range(0, 8)) = 3.0
        _CenterBias("Center Bias", Range(0, 3)) = 0.5
        _Contrast("Contrast", Range(0.5, 8)) = 3.5

        [Header(Palette)]
        [Toggle(_USE_GRADIENT)] _UseGradient("Use Gradient Texture", Float) = 0
        _GradientTex("Gradient 2D", 2D) = "white" {}
        [IntRange] _PaletteSize("Palette Size", Range(1, 8)) = 3
        _Palette0("Palette 0", Color) = (0.02, 0.02, 0.06, 1.0)
        _Palette1("Palette 1", Color) = (0.1, 0.05, 0.25, 1.0)
        _Palette2("Palette 2", Color) = (0.15, 0.25, 0.55, 1.0)
        _Palette3("Palette 3", Color) = (0, 0, 0, 1)
        _Palette4("Palette 4", Color) = (0, 0, 0, 1)
        _Palette5("Palette 5", Color) = (0, 0, 0, 1)
        _Palette6("Palette 6", Color) = (0, 0, 0, 1)
        _Palette7("Palette 7", Color) = (0, 0, 0, 1)
        _Lighting("Lighting", Range(0, 1)) = 0.3

        [Header(Stars)]
        _StarScarcity("Star Scarcity", Range(0, 1)) = 0.7
        [IntRange] _StarMaxSize("Star Max Size (px)", Range(1, 3)) = 2
        _StarColor1("Star Color 1", Color) = (1, 1, 1, 1)
        _StarColor2("Star Color 2", Color) = (0.8, 0.85, 1, 1)
        _StarColor3("Star Color 3", Color) = (1, 0.9, 0.7, 1)

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
            Name "SpaceWavesBackground"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _USE_GRADIENT
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

            CBUFFER_START(UnityPerMaterial)
                float _PixelFilter;
                float _ScrollSpeed;
                float _FlowAngle;
                float _NoiseScale;
                float _Lacunarity;
                float _WaveDensity;
                float _WaveFrequency;
                float _WaveAmplitude;
                float _WaveLayers;
                float _WarpStrength;
                float _CenterBias;
                float _Contrast;
                float _PaletteSize;
                float _Lighting;
                float _StarScarcity;
                float _StarMaxSize;
                float4 _StarColor1;
                float4 _StarColor2;
                float4 _StarColor3;
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
                // Pixelation
                // --------------------------------------------------------
                float pixelSize = screenDiag / _PixelFilter;
                float2 screenCoord = input.uv * screenSize;
                screenCoord = floor(screenCoord / pixelSize) * pixelSize;

                // --------------------------------------------------------
                // Aspect-independent centered UVs
                // --------------------------------------------------------
                float2 uv = (screenCoord - 0.5 * screenSize) / screenDiag - _Offset.xy;
                float uvLen = length(uv);

                // --------------------------------------------------------
                // Directional flow: rotate UVs by FlowAngle
                // --------------------------------------------------------
                float rad = _FlowAngle * 0.01745329; // degrees to radians
                float cs = cos(rad);
                float sn = sin(rad);
                float2 flowUV = float2(
                    uv.x * cs - uv.y * sn,
                    uv.x * sn + uv.y * cs
                );

                // --------------------------------------------------------
                // Layered sine wave displacement
                // Each layer has different angle and frequency for
                // organic, flowing, non-repeating wave shapes
                // --------------------------------------------------------
                float t = _Time.y * _ScrollSpeed;
                int layers = (int)_WaveLayers;
                float2 waveOffset = float2(0.0, 0.0);

                for (int w = 0; w < layers; w++)
                {
                    float layerAngle = (float)w * 2.399 + 0.5; // golden angle spacing
                    float2 dir = float2(cos(layerAngle), sin(layerAngle));
                    float freq = _WaveFrequency * (1.0 + (float)w * 0.7);
                    float phase = t * (0.8 + (float)w * 0.3) + (float)w * 3.7;
                    waveOffset += dir * sin(dot(flowUV, dir) * freq + phase) * _WaveAmplitude / (1.0 + (float)w * 0.5);
                }

                float2 warpedUV = flowUV + waveOffset;

                // --------------------------------------------------------
                // Domain warping marbling (iquilezles)
                // --------------------------------------------------------
                float2 p = warpedUV * _NoiseScale;
                float lac = _Lacunarity;

                float2 q = float2(
                    fbm(p + float2(0.0, 0.0) + float2(t * 0.08, t * 0.05), lac),
                    fbm(p + float2(5.2, 1.3) + float2(t * 0.06, t * 0.07), lac)
                );

                float2 r = float2(
                    fbm(p + _WarpStrength * q + float2(1.7, 9.2) + float2(t * 0.04, 0.0), lac),
                    fbm(p + _WarpStrength * q + float2(8.3, 2.8) + float2(0.0, t * 0.05), lac)
                );

                float pattern = fbm(p + _WarpStrength * r, lac);

                // --------------------------------------------------------
                // Wave density folding
                // --------------------------------------------------------
                pattern = frac(pattern * _WaveDensity);

                // --------------------------------------------------------
                // Center bias
                // --------------------------------------------------------
                float centerFalloff = 1.0 - saturate(uvLen * _CenterBias);
                centerFalloff *= centerFalloff;
                pattern = pattern * (0.6 + 0.4 * centerFalloff) + 0.25 * centerFalloff;

                // --------------------------------------------------------
                // Color: palette snap
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
                    float contrastMod = 0.25 * _Contrast + 0.5 * _WaveAmplitude + 1.2;
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

                // --------------------------------------------------------
                // Stars: fly across screen along flow + per-star noise curve
                // --------------------------------------------------------
                float2 pixelId = screenCoord / pixelSize;
                float2 starDrift = float2(cs, sn) * t * 5.0;
                float2 driftedId = pixelId - starDrift;

                float starGrid = 15.0;
                float2 starCell = floor(driftedId / starGrid);
                int maxSz = (int)_StarMaxSize;

                for (int sy = -1; sy <= 1; sy++)
                {
                    for (int sx = -1; sx <= 1; sx++)
                    {
                        float2 cell = starCell + float2(sx, sy);

                        float presence = hash21(cell + 0.5);
                        if (presence > 1.0 - _StarScarcity) continue;

                        // Star center in drifted space
                        float2 starPosDrifted = floor((cell + float2(
                            hash21(cell + 13.7),
                            hash21(cell + 57.3)
                        )) * starGrid);

                        // Convert back to screen space
                        float2 starPosScreen = starPosDrifted + starDrift;

                        // Evaluate wave field at THIS star's position
                        float2 starScreenUV = (starPosScreen * pixelSize - 0.5 * screenSize) / screenDiag;
                        float2 starFlowUV = float2(
                            starScreenUV.x * cs - starScreenUV.y * sn,
                            starScreenUV.x * sn + starScreenUV.y * cs
                        );
                        float2 starWave = float2(0.0, 0.0);
                        for (int w2 = 0; w2 < layers; w2++)
                        {
                            float la2 = (float)w2 * 2.399 + 0.5;
                            float2 wd2 = float2(cos(la2), sin(la2));
                            float freq2 = _WaveFrequency * (1.0 + (float)w2 * 0.7);
                            float phase2 = t * (0.8 + (float)w2 * 0.3) + (float)w2 * 3.7;
                            starWave += wd2 * sin(dot(starFlowUV, wd2) * freq2 + phase2) * _WaveAmplitude / (1.0 + (float)w2 * 0.5);
                        }
                        starPosScreen += starWave * _PixelFilter;

                        float sizeRnd = hash21(cell + 41.7);
                        float halfSize = ceil(1.0 + sizeRnd * (maxSz - 1.0)) * 0.5;

                        // Distance in screen pixels — star size never distorts
                        float dist = max(abs(pixelId.x - starPosScreen.x), abs(pixelId.y - starPosScreen.y));

                        if (dist < halfSize + 0.1)
                        {
                            float colorPick = hash21(cell + 97.3);
                            finalColor.rgb = colorPick < 0.33 ? _StarColor1.rgb
                                           : colorPick < 0.66 ? _StarColor2.rgb
                                           : _StarColor3.rgb;
                        }
                    }
                }

                finalColor.a *= input.color.a;
                return finalColor;
            }

            ENDHLSL
        }
    }

    Fallback "UI/Default"
}
