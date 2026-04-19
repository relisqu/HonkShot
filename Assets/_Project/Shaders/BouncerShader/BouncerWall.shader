Shader "Custom/BouncerWall"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}

        [Header(Bouncer)]
        _BouncerColor ("Bouncer Color", Color) = (1, 0.5, 0, 1)
        _BouncerTex ("Bouncer Texture", 2D) = "white" {}
        [IntRange] _PixelCount ("Bouncer Pixels", Range(1, 10)) = 5
        [Toggle] _BouncerTexStretch ("Stretch Texture to Band", Float) = 1
        _BrokenAmount ("Broken Amount", Range(0, 1)) = 0
        _BrokenShadowColor ("Broken Edge Shadow", Color) = (0, 0, 0, 0.4)

        [Header(Shadow)]
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 0.4)
        [IntRange] _ShadowPixelCount ("Shadow Pixels", Range(0, 10)) = 3

        [Header(Highlight)]
        _HighlightColor ("Highlight Color", Color) = (1, 1, 0.8, 1)
        [IntRange] _HighlightPixelCount ("Highlight Pixels", Range(1, 10)) = 2
        _HighlightDirection ("Highlight Direction (world XY)", Vector) = (0.2, 1, 0, 0)

        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [PerRendererData] _SpriteUVBounds("Sprite UV Bounds", Vector) = (0, 0, 1, 1)
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        // ============================================================
        // Pass 1 — Universal2D (Lit)
        // ============================================================
        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex BouncerLitVertex
            #pragma fragment BouncerLitFragment

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"

            #pragma multi_compile_instancing

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                float3 normal     : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                half4  color       : COLOR;
                float2 uv          : TEXCOORD0;
                half2  lightingUV  : TEXCOORD1;
                float2 worldPos    : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            TEXTURE2D(_BouncerTex);
            SAMPLER(sampler_BouncerTex);
            float4 _BouncerTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _BouncerColor;
                half4 _ShadowColor;
                half4 _HighlightColor;
                float4 _HighlightDirection;
                float4 _BouncerTex_ST;
                float _PixelCount;
                float _ShadowPixelCount;
                float _HighlightPixelCount;
                float _BouncerTexStretch;
                float _BrokenAmount;
                half4 _BrokenShadowColor;
                float4 _SpriteUVBounds;
            CBUFFER_END

            #if USE_SHAPE_LIGHT_TYPE_0
            SHAPE_LIGHT(0)
            #endif
            #if USE_SHAPE_LIGHT_TYPE_1
            SHAPE_LIGHT(1)
            #endif
            #if USE_SHAPE_LIGHT_TYPE_2
            SHAPE_LIGHT(2)
            #endif
            #if USE_SHAPE_LIGHT_TYPE_3
            SHAPE_LIGHT(3)
            #endif

            Varyings BouncerLitVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                SetUpSpriteInstanceProperties();
                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv         = v.uv;
                o.lightingUV = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);
                o.color      = v.color * _Color * unity_SpriteColor;
                o.worldPos   = TransformObjectToWorld(v.positionOS).xy;

                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            half4 BouncerLitFragment(Varyings i) : SV_Target
            {
                // Screen-aligned UV steps — computed from derivatives, independent of any rotation.
                // Normalized to 1 texel per step so the effect is zoom-independent (edit & play mode match).
                float2 dUVdx = ddx(i.uv);
                float2 dUVdy = ddy(i.uv);
                float2 uvDirUp    = dUVdy * sign(ddy(i.worldPos.y));
                float2 uvDirRight = dUVdx * sign(ddx(i.worldPos.x));
                float2 uvStepUp    = uvDirUp    / max(length(uvDirUp    * _MainTex_TexelSize.zw), 1e-8);
                float2 uvStepRight = uvDirRight / max(length(uvDirRight * _MainTex_TexelSize.zw), 1e-8);
                half4 texCol = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, i.uv, 0);

                half4 mainColor;
                half4 mask;

                if (texCol.a > 0.01)
                {
                    mainColor = texCol * i.color;
                    mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);

                    float2 lightDir = normalize(_HighlightDirection.xy);
                    int hlCount = (int)_HighlightPixelCount;
                    bool isEdge = false;

                    if (abs(lightDir.y) > 0.25)
                    {
                        float2 stepY = sign(lightDir.y) * uvStepUp;
                        [loop] for (int hy = 1; hy <= hlCount; hy++)
                        {
                            if (SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, i.uv + stepY * (float)hy, 0).a < 0.01)
                            { isEdge = true; break; }
                        }
                    }

                    if (!isEdge && abs(lightDir.x) > 0.25)
                    {
                        float2 stepX = sign(lightDir.x) * uvStepRight;
                        [loop] for (int hx = 1; hx <= hlCount; hx++)
                        {
                            if (SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, i.uv + stepX * (float)hx, 0).a < 0.01)
                            { isEdge = true; break; }
                        }
                    }

                    if (isEdge)
                        mainColor = half4(_HighlightColor.rgb, mainColor.a);

                    if (_BrokenAmount > 0.01)
                    {
                        float brickX = floor(i.uv.x * _MainTex_TexelSize.z / 3.0);
                        float colNoise = frac(sin(dot(float2(brickX, 0), float2(127.1, 311.7))) * 43758.5453);
                        int reduction = (int)floor(colNoise * _BrokenAmount * 7.0);
                        if (reduction > 0)
                        {
                            bool replacedBouncer = false;
                            [loop] for (int e = 1; e <= 6; e++)
                            {
                                float2 checkUV = i.uv + uvStepUp * (float)e;
                                if (SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, checkUV, 0).a < 0.01)
                                {
                                    if (e <= reduction)
                                        mainColor.a = 0;
                                    break;
                                }
                            }
                            int bottomReduction = max(reduction - 1, 0);
                            if (bottomReduction > 0 && mainColor.a > 0)
                            {
                                [loop] for (int b = 1; b <= 5; b++)
                                {
                                    float2 checkUV = i.uv - uvStepUp * (float)b;
                                    if (SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, checkUV, 0).a < 0.01)
                                    {
                                        if (b <= bottomReduction)
                                        {
                                            int bRow = bottomReduction - b + 1;
                                            int bcB = (int)_PixelCount;
                                            float2 bUV;
                                            float pixBX = i.uv.x * _MainTex_TexelSize.z;
                                            bUV.x = (pixBX + 0.5) * _BouncerTex_TexelSize.x * _BouncerTex_ST.x + _BouncerTex_ST.z;
                                            float sY = (_BouncerTex_TexelSize.w - 0.5 - (float)(bRow - 1) / max((float)(bcB - 1), 1.0) * (_BouncerTex_TexelSize.w - 1.0)) * _BouncerTex_TexelSize.y;
                                            float tY = (_BouncerTex_TexelSize.w - (float)bRow + 0.5) * _BouncerTex_TexelSize.y;
                                            bUV.y = _BouncerTexStretch > 0.5 ? sY : tY;
                                            half4 bTex = SAMPLE_TEXTURE2D_LOD(_BouncerTex, sampler_BouncerTex, bUV, 0);
                                            mainColor = bTex * _BouncerColor;
                                            mask = half4(1, 1, 1, 1);
                                            replacedBouncer = true;
                                        }
                                        break;
                                    }
                                }
                            }
                            if (mainColor.a > 0 && !replacedBouncer)
                                mainColor.rgb *= _BrokenShadowColor.rgb;
                        }
                    }
                }
                else
                {
                    int bouncerCount = (int)_PixelCount;
                    int shadowCount  = (int)_ShadowPixelCount;
                    int totalCount   = bouncerCount + shadowCount;

                    mainColor = half4(0, 0, 0, 0);
                    mask = half4(1, 1, 1, 1);

                    float brickXS = floor(i.uv.x * _MainTex_TexelSize.z / 3.0);
                    float colNoiseS = frac(sin(dot(float2(brickXS, 0), float2(127.1, 311.7))) * 43758.5453);
                    int reductionS = (int)floor(colNoiseS * _BrokenAmount * 7.0);
                    int bottomSkip = max(reductionS - 1, 0);
                    int skippedCount = 0;

                    [loop] for (int s = 1; s <= totalCount + bottomSkip; s++)
                    {
                        float2 sampleUV = i.uv + uvStepUp * (float)s;
                        if (any(sampleUV < _SpriteUVBounds.xy) || any(sampleUV > _SpriteUVBounds.zw))
                            break;
                        if (length((sampleUV - i.uv) * _MainTex_TexelSize.zw) < 0.3)
                            continue;
                        if (SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, sampleUV, 0).a > 0.01)
                        {
                            if (skippedCount < bottomSkip)
                            {
                                skippedCount++;
                                continue;
                            }
                            int es = s - skippedCount;
                            if (es <= bouncerCount)
                            {
                                float localDist = (float)es;
                                float2 bUV;
                                float pixelX = i.uv.x * _MainTex_TexelSize.z;
                                bUV.x = (pixelX + 0.5) * _BouncerTex_TexelSize.x * _BouncerTex_ST.x + _BouncerTex_ST.z;
                                float stretchY = (_BouncerTex_TexelSize.w - 0.5 - (float)(es - 1) / max((float)(bouncerCount - 1), 1.0) * (_BouncerTex_TexelSize.w - 1.0)) * _BouncerTex_TexelSize.y;
                                float tileY = (_BouncerTex_TexelSize.w - localDist + 0.5) * _BouncerTex_TexelSize.y;
                                bUV.y = _BouncerTexStretch > 0.5 ? stretchY : tileY;
                                half4 bTex = SAMPLE_TEXTURE2D_LOD(_BouncerTex, sampler_BouncerTex, bUV, 0);
                                mainColor = bTex * _BouncerColor;
                            }
                            else
                            {
                                float t = (float)(es - bouncerCount) / (float)(shadowCount + 1);
                                mainColor = _ShadowColor;
                            }
                            break;
                        }
                    }
                }

                SurfaceData2D surfaceData;
                InputData2D   inputData;

                InitializeSurfaceData(mainColor.rgb, mainColor.a, mask, surfaceData);
                InitializeInputData(i.uv, i.lightingUV, inputData);

                return CombinedShapeLightShared(surfaceData, inputData);
            }
            ENDHLSL
        }

        // ============================================================
        // Pass 2 — NormalsRendering
        // ============================================================
        Pass
        {
            Tags { "LightMode" = "NormalsRendering" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex BouncerNormalsVertex
            #pragma fragment BouncerNormalsFragment

            #pragma multi_compile_instancing

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                float3 normal     : NORMAL;
                float4 tangent    : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                half4  color       : COLOR;
                float2 uv          : TEXCOORD0;
                half3  normalWS    : TEXCOORD1;
                half3  tangentWS   : TEXCOORD2;
                half3  bitangentWS : TEXCOORD3;
                float2 worldPos    : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _BouncerColor;
                half4 _ShadowColor;
                half4 _HighlightColor;
                float4 _HighlightDirection;
                float4 _BouncerTex_ST;
                float _PixelCount;
                float _ShadowPixelCount;
                float _HighlightPixelCount;
                float _BouncerTexStretch;
                float _BrokenAmount;
                half4 _BrokenShadowColor;
                float4 _SpriteUVBounds;
            CBUFFER_END

            Varyings BouncerNormalsVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                SetUpSpriteInstanceProperties();
                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv         = v.uv;
                o.color      = v.color * _Color * unity_SpriteColor;
                o.normalWS   = TransformObjectToWorldDir(v.normal);
                o.tangentWS  = TransformObjectToWorldDir(v.tangent.xyz);
                o.bitangentWS = cross(o.normalWS, o.tangentWS) * v.tangent.w;
                o.worldPos   = TransformObjectToWorld(v.positionOS).xy;

                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

            half4 BouncerNormalsFragment(Varyings i) : SV_Target
            {
                float2 uvDirUp = ddy(i.uv) * sign(ddy(i.worldPos.y));
                float2 uvStepUp = uvDirUp / max(length(uvDirUp * _MainTex_TexelSize.zw), 1e-8);
                half4 texCol = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, i.uv, 0);
                half  resolvedAlpha;

                if (texCol.a > 0.01)
                {
                    resolvedAlpha = texCol.a * i.color.a;

                    if (_BrokenAmount > 0.01)
                    {
                        float brickX = floor(i.uv.x * _MainTex_TexelSize.z / 3.0);
                        float colNoise = frac(sin(dot(float2(brickX, 0), float2(127.1, 311.7))) * 43758.5453);
                        int reduction = (int)floor(colNoise * _BrokenAmount * 7.0);
                        if (reduction > 0)
                        {
                            [loop] for (int e = 1; e <= 6; e++)
                            {
                                float2 checkUV = i.uv + uvStepUp * (float)e;
                                if (SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, checkUV, 0).a < 0.01)
                                {
                                    if (e <= reduction)
                                        resolvedAlpha = 0;
                                    break;
                                }
                            }
                            int bottomReduction = max(reduction - 1, 0);
                            if (bottomReduction > 0 && resolvedAlpha > 0)
                            {
                                [loop] for (int b = 1; b <= 5; b++)
                                {
                                    float2 checkUV = i.uv - uvStepUp * (float)b;
                                    if (SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, checkUV, 0).a < 0.01)
                                    {
                                        if (b <= bottomReduction)
                                            resolvedAlpha = _BouncerColor.a;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    int bouncerCount = (int)_PixelCount;
                    int shadowCount  = (int)_ShadowPixelCount;
                    int totalCount   = bouncerCount + shadowCount;

                    resolvedAlpha = 0;

                    float brickXS = floor(i.uv.x * _MainTex_TexelSize.z / 3.0);
                    float colNoiseS = frac(sin(dot(float2(brickXS, 0), float2(127.1, 311.7))) * 43758.5453);
                    int reductionS = (int)floor(colNoiseS * _BrokenAmount * 7.0);
                    int bottomSkip = max(reductionS - 1, 0);
                    int skippedCount = 0;

                    [loop] for (int s = 1; s <= totalCount + bottomSkip; s++)
                    {
                        float2 sampleUV = i.uv + uvStepUp * (float)s;
                        if (any(sampleUV < _SpriteUVBounds.xy) || any(sampleUV > _SpriteUVBounds.zw))
                            break;
                        if (length((sampleUV - i.uv) * _MainTex_TexelSize.zw) < 0.3)
                            continue;
                        if (SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, sampleUV, 0).a > 0.01)
                        {
                            if (skippedCount < bottomSkip)
                            {
                                skippedCount++;
                                continue;
                            }
                            int es = s - skippedCount;
                            if (es <= bouncerCount)
                            {
                                resolvedAlpha = _BouncerColor.a;
                            }
                            else
                            {
                                float t = (float)(es - bouncerCount) / (float)(shadowCount + 1);
                                resolvedAlpha = _ShadowColor.a * (1.0 - t);
                            }
                            break;
                        }
                    }
                }

                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv));
                half4 colorForNormals = half4(i.color.rgb, resolvedAlpha);

                return NormalsRenderingShared(colorForNormals, normalTS,
                    i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
            }
            ENDHLSL
        }

        // ============================================================
        // Pass 3 — UniversalForward (Unlit fallback)
        // ============================================================
        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue" = "Transparent" "RenderType" = "Transparent" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex BouncerUnlitVertex
            #pragma fragment BouncerUnlitFragment

            #pragma multi_compile_instancing

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float4 color       : COLOR;
                float2 uv          : TEXCOORD0;
                float2 worldPos    : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            TEXTURE2D(_BouncerTex);
            SAMPLER(sampler_BouncerTex);
            float4 _BouncerTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _BouncerColor;
                half4 _ShadowColor;
                half4 _HighlightColor;
                float4 _HighlightDirection;
                float4 _BouncerTex_ST;
                float _PixelCount;
                float _ShadowPixelCount;
                float _HighlightPixelCount;
                float _BouncerTexStretch;
                float _BrokenAmount;
                half4 _BrokenShadowColor;
                float4 _SpriteUVBounds;
            CBUFFER_END

            Varyings BouncerUnlitVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                SetUpSpriteInstanceProperties();
                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv    = v.uv;
                o.color = v.color * _Color * unity_SpriteColor;
                o.worldPos = TransformObjectToWorld(v.positionOS).xy;

                return o;
            }

            half4 BouncerUnlitFragment(Varyings i) : SV_Target
            {
                float2 dUVdx = ddx(i.uv);
                float2 dUVdy = ddy(i.uv);
                float2 uvDirUp    = dUVdy * sign(ddy(i.worldPos.y));
                float2 uvDirRight = dUVdx * sign(ddx(i.worldPos.x));
                float2 uvStepUp    = uvDirUp    / max(length(uvDirUp    * _MainTex_TexelSize.zw), 1e-8);
                float2 uvStepRight = uvDirRight / max(length(uvDirRight * _MainTex_TexelSize.zw), 1e-8);
                half4 texCol = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, i.uv, 0);

                if (texCol.a > 0.01)
                {
                    half4 mainColor = texCol * i.color;

                    float2 lightDir = normalize(_HighlightDirection.xy);
                    int hlCount = (int)_HighlightPixelCount;
                    bool isEdge = false;

                    if (abs(lightDir.y) > 0.25)
                    {
                        float2 stepY = sign(lightDir.y) * uvStepUp;
                        [loop] for (int hy = 1; hy <= hlCount; hy++)
                        {
                            if (SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, i.uv + stepY * (float)hy, 0).a < 0.01)
                            { isEdge = true; break; }
                        }
                    }
                    if (!isEdge && abs(lightDir.x) > 0.25)
                    {
                        float2 stepX = sign(lightDir.x) * uvStepRight;
                        [loop] for (int hx = 1; hx <= hlCount; hx++)
                        {
                            if (SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, i.uv + stepX * (float)hx, 0).a < 0.01)
                            { isEdge = true; break; }
                        }
                    }

                    if (isEdge)
                        mainColor = half4(_HighlightColor.rgb, mainColor.a);

                    if (_BrokenAmount > 0.01)
                    {
                        float brickX = floor(i.uv.x * _MainTex_TexelSize.z / 3.0);
                        float colNoise = frac(sin(dot(float2(brickX, 0), float2(127.1, 311.7))) * 43758.5453);
                        int reduction = (int)floor(colNoise * _BrokenAmount * 7.0);
                        if (reduction > 0)
                        {
                            bool replacedBouncer = false;
                            [loop] for (int e = 1; e <= 6; e++)
                            {
                                float2 checkUV = i.uv + uvStepUp * (float)e;
                                if (SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, checkUV, 0).a < 0.01)
                                {
                                    if (e <= reduction)
                                        mainColor.a = 0;
                                    break;
                                }
                            }
                            int bottomReduction = max(reduction - 1, 0);
                            if (bottomReduction > 0 && mainColor.a > 0)
                            {
                                [loop] for (int b = 1; b <= 5; b++)
                                {
                                    float2 checkUV = i.uv - uvStepUp * (float)b;
                                    if (SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, checkUV, 0).a < 0.01)
                                    {
                                        if (b <= bottomReduction)
                                        {
                                            int bRow = bottomReduction - b + 1;
                                            int bcB = (int)_PixelCount;
                                            float2 bUV;
                                            float pixBX = i.uv.x * _MainTex_TexelSize.z;
                                            bUV.x = (pixBX + 0.5) * _BouncerTex_TexelSize.x * _BouncerTex_ST.x + _BouncerTex_ST.z;
                                            float sY = (_BouncerTex_TexelSize.w - 0.5 - (float)(bRow - 1) / max((float)(bcB - 1), 1.0) * (_BouncerTex_TexelSize.w - 1.0)) * _BouncerTex_TexelSize.y;
                                            float tY = (_BouncerTex_TexelSize.w - (float)bRow + 0.5) * _BouncerTex_TexelSize.y;
                                            bUV.y = _BouncerTexStretch > 0.5 ? sY : tY;
                                            half4 bTex = SAMPLE_TEXTURE2D_LOD(_BouncerTex, sampler_BouncerTex, bUV, 0);
                                            mainColor = bTex * _BouncerColor;
                                            replacedBouncer = true;
                                        }
                                        break;
                                    }
                                }
                            }
                            if (mainColor.a > 0 && !replacedBouncer)
                                mainColor.rgb *= _BrokenShadowColor.rgb;
                        }
                    }

                    return mainColor;
                }

                int bouncerCount = (int)_PixelCount;
                int shadowCount  = (int)_ShadowPixelCount;
                int totalCount   = bouncerCount + shadowCount;

                float brickXS = floor(i.uv.x * _MainTex_TexelSize.z / 3.0);
                float colNoiseS = frac(sin(dot(float2(brickXS, 0), float2(127.1, 311.7))) * 43758.5453);
                int reductionS = (int)floor(colNoiseS * _BrokenAmount * 7.0);
                int bottomSkip = max(reductionS - 1, 0);
                int skippedCount = 0;

                [loop] for (int s = 1; s <= totalCount + bottomSkip; s++)
                {
                    float2 sampleUV = i.uv + uvStepUp * (float)s;
                    if (any(sampleUV < _SpriteUVBounds.xy) || any(sampleUV > _SpriteUVBounds.zw))
                        break;
                    if (length((sampleUV - i.uv) * _MainTex_TexelSize.zw) < 0.3)
                        continue;
                    if (SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, sampleUV, 0).a > 0.01)
                    {
                        if (skippedCount < bottomSkip)
                        {
                            skippedCount++;
                            continue;
                        }
                        int es = s - skippedCount;
                        if (es <= bouncerCount)
                        {
                            float localDist = (float)es;
                            float2 bUV;
                            float pixelX = i.uv.x * _MainTex_TexelSize.z;
                            bUV.x = (pixelX + 0.5) * _BouncerTex_TexelSize.x * _BouncerTex_ST.x + _BouncerTex_ST.z;
                            float stretchY = (_BouncerTex_TexelSize.w - 0.5 - (float)(es - 1) / max((float)(bouncerCount - 1), 1.0) * (_BouncerTex_TexelSize.w - 1.0)) * _BouncerTex_TexelSize.y;
                            float tileY = (_BouncerTex_TexelSize.w - localDist + 0.5) * _BouncerTex_TexelSize.y;
                            bUV.y = _BouncerTexStretch > 0.5 ? stretchY : tileY;
                            half4 bTex = SAMPLE_TEXTURE2D_LOD(_BouncerTex, sampler_BouncerTex, bUV, 0);
                            return bTex * _BouncerColor;
                        }

                        float t = (float)(es - bouncerCount) / (float)(shadowCount + 1);
                        half4 shadow = _ShadowColor;
                        shadow.a *= (1.0 - t);
                        return shadow;
                    }
                }

                return half4(0, 0, 0, 0);
            }
            ENDHLSL
        }
    }
}
