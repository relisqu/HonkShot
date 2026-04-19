Shader "Custom/BrambleWall"
{
    Properties
    {
        [PerRendererData] _MainTex ("Shape Sprite", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}

        [Header(Fill)]
        _FillTex ("Fill Texture", 2D) = "white" {}
        _FillColor ("Fill Tint", Color) = (1, 1, 1, 1)
        _FillScale ("Fill Scale (world units per tile)", Float) = 2

        [Header(Edge)]
        _EdgeTex ("Edge Texture", 2D) = "white" {}
        _EdgeColor ("Edge Tint", Color) = (1, 1, 1, 1)
        _EdgeScale ("Edge Scale (world units per tile)", Float) = 2
        [IntRange] _EdgeWidth ("Edge Width (pixels)", Range(1, 30)) = 5
        [IntRange] _BlendWidth ("Blend Width (pixels)", Range(0, 20)) = 3
        _EdgeExtendTiles ("Edge Extend (tile repeats)", Range(0, 3)) = 1

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

            #pragma vertex BrambleLitVertex
            #pragma fragment BrambleLitFragment

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
                float2 objectPos   : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            TEXTURE2D(_FillTex);
            TEXTURE2D(_EdgeTex);
            SamplerState sampler_point_repeat;
            SamplerState sampler_point_clamp;

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _FillColor;
                half4 _EdgeColor;
                float _FillScale;
                float _EdgeScale;
                float _EdgeWidth;
                float _BlendWidth;
                float _EdgeExtendTiles;
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

            Varyings BrambleLitVertex(Attributes v)
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
                o.objectPos  = v.positionOS.xy;

                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            // Returns true if the sample UV is outside the sprite's UV bounds
            bool IsOutOfBounds(float2 sampleUV)
            {
                return any(sampleUV < _SpriteUVBounds.xy) || any(sampleUV > _SpriteUVBounds.zw);
            }

            // Returns true if the sample position is transparent (out of bounds or alpha < 0.01)
            bool IsTransparent(float2 sampleUV)
            {
                if (IsOutOfBounds(sampleUV)) return true;
                return SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, sampleUV, 0).a < 0.01;
            }

            // Returns min distance (in pixels) to an OPAQUE neighbor (for transparent pixels).
            float GetDistToOpaque(float2 uv, float2 uvStepUp, float2 uvStepRight, int maxDist)
            {
                float minDist = (float)(maxDist + 1);

                [loop] for (int d = 1; d <= maxDist; d++)
                {
                    if (!IsTransparent(uv + uvStepUp * (float)d))
                    { minDist = min(minDist, (float)d); break; }
                }
                [loop] for (int d2 = 1; d2 <= maxDist; d2++)
                {
                    if (!IsTransparent(uv - uvStepUp * (float)d2))
                    { minDist = min(minDist, (float)d2); break; }
                }
                [loop] for (int d3 = 1; d3 <= maxDist; d3++)
                {
                    if (!IsTransparent(uv + uvStepRight * (float)d3))
                    { minDist = min(minDist, (float)d3); break; }
                }
                [loop] for (int d4 = 1; d4 <= maxDist; d4++)
                {
                    if (!IsTransparent(uv - uvStepRight * (float)d4))
                    { minDist = min(minDist, (float)d4); break; }
                }

                float2 diagA = uvStepUp + uvStepRight;
                float2 diagB = uvStepUp - uvStepRight;
                [loop] for (int d5 = 1; d5 <= maxDist; d5++)
                {
                    if (!IsTransparent(uv + diagA * (float)d5))
                    { minDist = min(minDist, (float)d5 * 1.414); break; }
                }
                [loop] for (int d6 = 1; d6 <= maxDist; d6++)
                {
                    if (!IsTransparent(uv - diagA * (float)d6))
                    { minDist = min(minDist, (float)d6 * 1.414); break; }
                }
                [loop] for (int d7 = 1; d7 <= maxDist; d7++)
                {
                    if (!IsTransparent(uv + diagB * (float)d7))
                    { minDist = min(minDist, (float)d7 * 1.414); break; }
                }
                [loop] for (int d8 = 1; d8 <= maxDist; d8++)
                {
                    if (!IsTransparent(uv - diagB * (float)d8))
                    { minDist = min(minDist, (float)d8 * 1.414); break; }
                }

                return minDist;
            }

            // Returns min distance (in pixels) to a transparent neighbor.
            // Searches 4 cardinal + 4 diagonal directions.
            float GetEdgeDistance(float2 uv, float2 uvStepUp, float2 uvStepRight, int maxDist)
            {
                float minDist = (float)(maxDist + 1);

                // cardinal directions
                [loop] for (int d = 1; d <= maxDist; d++)
                {
                    if (IsTransparent(uv + uvStepUp * (float)d))
                    { minDist = min(minDist, (float)d); break; }
                }
                [loop] for (int d2 = 1; d2 <= maxDist; d2++)
                {
                    if (IsTransparent(uv - uvStepUp * (float)d2))
                    { minDist = min(minDist, (float)d2); break; }
                }
                [loop] for (int d3 = 1; d3 <= maxDist; d3++)
                {
                    if (IsTransparent(uv + uvStepRight * (float)d3))
                    { minDist = min(minDist, (float)d3); break; }
                }
                [loop] for (int d4 = 1; d4 <= maxDist; d4++)
                {
                    if (IsTransparent(uv - uvStepRight * (float)d4))
                    { minDist = min(minDist, (float)d4); break; }
                }

                // diagonal directions (distance scaled by sqrt(2))
                float2 diagA = uvStepUp + uvStepRight;
                float2 diagB = uvStepUp - uvStepRight;
                [loop] for (int d5 = 1; d5 <= maxDist; d5++)
                {
                    if (IsTransparent(uv + diagA * (float)d5))
                    { minDist = min(minDist, (float)d5 * 1.414); break; }
                }
                [loop] for (int d6 = 1; d6 <= maxDist; d6++)
                {
                    if (IsTransparent(uv - diagA * (float)d6))
                    { minDist = min(minDist, (float)d6 * 1.414); break; }
                }
                [loop] for (int d7 = 1; d7 <= maxDist; d7++)
                {
                    if (IsTransparent(uv + diagB * (float)d7))
                    { minDist = min(minDist, (float)d7 * 1.414); break; }
                }
                [loop] for (int d8 = 1; d8 <= maxDist; d8++)
                {
                    if (IsTransparent(uv - diagB * (float)d8))
                    { minDist = min(minDist, (float)d8 * 1.414); break; }
                }

                return minDist;
            }

            half4 BrambleLitFragment(Varyings i) : SV_Target
            {
                // Derivative-based UV steps (zoom-independent, rotation-safe)
                float2 dUVdx = ddx(i.uv);
                float2 dUVdy = ddy(i.uv);
                float2 uvDirUp    = dUVdy * sign(ddy(i.worldPos.y));
                float2 uvDirRight = dUVdx * sign(ddx(i.worldPos.x));
                float2 uvStepUp    = uvDirUp    / max(length(uvDirUp    * _MainTex_TexelSize.zw), 1e-8);
                float2 uvStepRight = uvDirRight / max(length(uvDirRight * _MainTex_TexelSize.zw), 1e-8);

                half4 shapeCol = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, i.uv, 0);

                // Fill: world-space tiling UVs (doesn't rotate with sprite)
                float2 fillUV = i.worldPos / _FillScale;
                half4 fillSample = SAMPLE_TEXTURE2D(_FillTex, sampler_point_repeat, fillUV) * _FillColor;

                // Edge: world-space UVs (doesn't rotate with sprite)
                float2 edgeUV = i.worldPos / _EdgeScale;
                half4 edgeSample = SAMPLE_TEXTURE2D(_EdgeTex, sampler_point_repeat, edgeUV) * _EdgeColor;

                half4 mainColor;

                if (shapeCol.a < 0.01)
                {
                    // Outside the shape — draw edge only if full tile cell fits
                    if (_EdgeExtendTiles < 0.01)
                        return half4(0, 0, 0, 0);

                    // World-to-UV Jacobian (correctly maps rotated sprite bounds)
                    float2 dWdx_v = ddx(i.worldPos);
                    float2 dWdy_v = ddy(i.worldPos);
                    float detW = dWdx_v.x * dWdy_v.y - dWdy_v.x * dWdx_v.y;
                    if (abs(detW) < 1e-10)
                        return half4(0, 0, 0, 0);
                    float invDetW = 1.0 / detW;

                    float2 dUdx_v = ddx(i.uv);
                    float2 dUdy_v = ddy(i.uv);

                    // UV offset per world-space unit
                    float2 uvPerWX = float2(
                        dUdx_v.x * dWdy_v.y - dUdy_v.x * dWdx_v.y,
                        dUdx_v.y * dWdy_v.y - dUdy_v.y * dWdx_v.y
                    ) * invDetW;
                    float2 uvPerWY = float2(
                        -dUdx_v.x * dWdy_v.x + dUdy_v.x * dWdx_v.x,
                        -dUdx_v.y * dWdy_v.x + dUdy_v.y * dWdx_v.x
                    ) * invDetW;

                    // Tile cell in world space (matches edge texture UV grid)
                    float2 cellMin = floor(i.worldPos / _EdgeScale) * _EdgeScale;
                    float2 cellMax = cellMin + _EdgeScale;

                    // Check 4 corners — if ANY is outside sprite bounds, tile would be cut → skip
                    float2 checkPts[4] = {
                        cellMin,
                        float2(cellMax.x, cellMin.y),
                        float2(cellMin.x, cellMax.y),
                        cellMax
                    };

                    bool tileFits = true;
                    [unroll] for (int c = 0; c < 4; c++)
                    {
                        float2 woff = checkPts[c] - i.worldPos;
                        float2 checkUV = i.uv + uvPerWX * woff.x + uvPerWY * woff.y;

                        if (IsOutOfBounds(checkUV))
                        {
                            tileFits = false;
                            break;
                        }
                    }

                    if (!tileFits)
                        return half4(0, 0, 0, 0);

                    // Also limit max extend distance
                    float worldPixelSize = min(length(ddx(i.worldPos)), length(ddy(i.worldPos)));
                    float extendWorld = _EdgeExtendTiles * _EdgeScale;
                    int searchPixels = min((int)ceil(extendWorld / max(worldPixelSize, 1e-6)) + 2, 150);
                    float distPixels = GetDistToOpaque(i.uv, uvStepUp, uvStepRight, searchPixels);
                    float distWorld = distPixels * worldPixelSize;
                    if (distWorld >= extendWorld)
                        return half4(0, 0, 0, 0);

                    mainColor = edgeSample;
                    mainColor.a *= edgeSample.a * i.color.a;
                }
                else
                {
                    // Inside the shape — fill is base, edge overlaid on top near boundary
                    int maxCheck = (int)(_EdgeWidth + _BlendWidth);
                    float edgeDist = GetEdgeDistance(i.uv, uvStepUp, uvStepRight, maxCheck);

                    // Start with fill
                    mainColor = fillSample;

                    if (edgeDist <= _EdgeWidth + _BlendWidth)
                    {
                        // Composite edge texture on top of fill using edge alpha
                        float edgeStrength = edgeSample.a;

                        // Fade out edge contribution in the blend zone
                        if (edgeDist > _EdgeWidth && _BlendWidth > 0)
                        {
                            float t = (edgeDist - _EdgeWidth) / _BlendWidth;
                            edgeStrength *= (1.0 - t);
                        }

                        // Where edge is opaque: show edge wires
                        // Where edge is transparent: fill shows through
                        mainColor.rgb = lerp(fillSample.rgb, edgeSample.rgb, edgeStrength);
                    }

                    mainColor.a *= shapeCol.a * i.color.a;
                }

                half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
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

            #pragma vertex BrambleNormalsVertex
            #pragma fragment BrambleNormalsFragment

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
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _FillColor;
                half4 _EdgeColor;
                float _FillScale;
                float _EdgeScale;
                float _EdgeWidth;
                float _BlendWidth;
                float _EdgeExtendTiles;
                float4 _SpriteUVBounds;
            CBUFFER_END

            Varyings BrambleNormalsVertex(Attributes v)
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

                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

            half4 BrambleNormalsFragment(Varyings i) : SV_Target
            {
                half4 texCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv));
                half4 colorForNormals = half4(i.color.rgb, texCol.a * i.color.a);

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

            #pragma vertex BrambleUnlitVertex
            #pragma fragment BrambleUnlitFragment

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
                float2 objectPos   : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            TEXTURE2D(_FillTex);
            TEXTURE2D(_EdgeTex);
            SamplerState sampler_point_repeat;
            SamplerState sampler_point_clamp;

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _FillColor;
                half4 _EdgeColor;
                float _FillScale;
                float _EdgeScale;
                float _EdgeWidth;
                float _BlendWidth;
                float _EdgeExtendTiles;
                float4 _SpriteUVBounds;
            CBUFFER_END

            Varyings BrambleUnlitVertex(Attributes v)
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
                o.objectPos = v.positionOS.xy;

                return o;
            }

            bool IsOutOfBoundsUnlit(float2 sampleUV)
            {
                return any(sampleUV < _SpriteUVBounds.xy) || any(sampleUV > _SpriteUVBounds.zw);
            }

            bool IsTransparentUnlit(float2 sampleUV)
            {
                if (IsOutOfBoundsUnlit(sampleUV)) return true;
                return SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, sampleUV, 0).a < 0.01;
            }

            float GetDistToOpaqueUnlit(float2 uv, float2 uvStepUp, float2 uvStepRight, int maxDist)
            {
                float minDist = (float)(maxDist + 1);

                [loop] for (int d = 1; d <= maxDist; d++)
                {
                    if (!IsTransparentUnlit(uv + uvStepUp * (float)d))
                    { minDist = min(minDist, (float)d); break; }
                }
                [loop] for (int d2 = 1; d2 <= maxDist; d2++)
                {
                    if (!IsTransparentUnlit(uv - uvStepUp * (float)d2))
                    { minDist = min(minDist, (float)d2); break; }
                }
                [loop] for (int d3 = 1; d3 <= maxDist; d3++)
                {
                    if (!IsTransparentUnlit(uv + uvStepRight * (float)d3))
                    { minDist = min(minDist, (float)d3); break; }
                }
                [loop] for (int d4 = 1; d4 <= maxDist; d4++)
                {
                    if (!IsTransparentUnlit(uv - uvStepRight * (float)d4))
                    { minDist = min(minDist, (float)d4); break; }
                }

                float2 diagA = uvStepUp + uvStepRight;
                float2 diagB = uvStepUp - uvStepRight;
                [loop] for (int d5 = 1; d5 <= maxDist; d5++)
                {
                    if (!IsTransparentUnlit(uv + diagA * (float)d5))
                    { minDist = min(minDist, (float)d5 * 1.414); break; }
                }
                [loop] for (int d6 = 1; d6 <= maxDist; d6++)
                {
                    if (!IsTransparentUnlit(uv - diagA * (float)d6))
                    { minDist = min(minDist, (float)d6 * 1.414); break; }
                }
                [loop] for (int d7 = 1; d7 <= maxDist; d7++)
                {
                    if (!IsTransparentUnlit(uv + diagB * (float)d7))
                    { minDist = min(minDist, (float)d7 * 1.414); break; }
                }
                [loop] for (int d8 = 1; d8 <= maxDist; d8++)
                {
                    if (!IsTransparentUnlit(uv - diagB * (float)d8))
                    { minDist = min(minDist, (float)d8 * 1.414); break; }
                }

                return minDist;
            }

            float GetEdgeDistanceUnlit(float2 uv, float2 uvStepUp, float2 uvStepRight, int maxDist)
            {
                float minDist = (float)(maxDist + 1);

                [loop] for (int d = 1; d <= maxDist; d++)
                {
                    if (IsTransparentUnlit(uv + uvStepUp * (float)d))
                    { minDist = min(minDist, (float)d); break; }
                }
                [loop] for (int d2 = 1; d2 <= maxDist; d2++)
                {
                    if (IsTransparentUnlit(uv - uvStepUp * (float)d2))
                    { minDist = min(minDist, (float)d2); break; }
                }
                [loop] for (int d3 = 1; d3 <= maxDist; d3++)
                {
                    if (IsTransparentUnlit(uv + uvStepRight * (float)d3))
                    { minDist = min(minDist, (float)d3); break; }
                }
                [loop] for (int d4 = 1; d4 <= maxDist; d4++)
                {
                    if (IsTransparentUnlit(uv - uvStepRight * (float)d4))
                    { minDist = min(minDist, (float)d4); break; }
                }

                float2 diagA = uvStepUp + uvStepRight;
                float2 diagB = uvStepUp - uvStepRight;
                [loop] for (int d5 = 1; d5 <= maxDist; d5++)
                {
                    if (IsTransparentUnlit(uv + diagA * (float)d5))
                    { minDist = min(minDist, (float)d5 * 1.414); break; }
                }
                [loop] for (int d6 = 1; d6 <= maxDist; d6++)
                {
                    if (IsTransparentUnlit(uv - diagA * (float)d6))
                    { minDist = min(minDist, (float)d6 * 1.414); break; }
                }
                [loop] for (int d7 = 1; d7 <= maxDist; d7++)
                {
                    if (IsTransparentUnlit(uv + diagB * (float)d7))
                    { minDist = min(minDist, (float)d7 * 1.414); break; }
                }
                [loop] for (int d8 = 1; d8 <= maxDist; d8++)
                {
                    if (IsTransparentUnlit(uv - diagB * (float)d8))
                    { minDist = min(minDist, (float)d8 * 1.414); break; }
                }

                return minDist;
            }

            half4 BrambleUnlitFragment(Varyings i) : SV_Target
            {
                float2 dUVdx = ddx(i.uv);
                float2 dUVdy = ddy(i.uv);
                float2 uvDirUp    = dUVdy * sign(ddy(i.worldPos.y));
                float2 uvDirRight = dUVdx * sign(ddx(i.worldPos.x));
                float2 uvStepUp    = uvDirUp    / max(length(uvDirUp    * _MainTex_TexelSize.zw), 1e-8);
                float2 uvStepRight = uvDirRight / max(length(uvDirRight * _MainTex_TexelSize.zw), 1e-8);

                half4 shapeCol = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, i.uv, 0);

                float2 fillUV = i.worldPos / _FillScale;
                float2 edgeUV = i.objectPos / _EdgeScale;

                half4 fillSample = SAMPLE_TEXTURE2D(_FillTex, sampler_point_repeat, fillUV) * _FillColor;
                half4 edgeSample = SAMPLE_TEXTURE2D(_EdgeTex, sampler_point_repeat, edgeUV) * _EdgeColor;

                half4 mainColor;

                if (shapeCol.a < 0.01)
                {
                    if (_EdgeExtendTiles < 0.01)
                        return half4(0, 0, 0, 0);

                    // World-to-UV Jacobian
                    float2 dWdx_v = ddx(i.worldPos);
                    float2 dWdy_v = ddy(i.worldPos);
                    float detW = dWdx_v.x * dWdy_v.y - dWdy_v.x * dWdx_v.y;
                    if (abs(detW) < 1e-10)
                        return half4(0, 0, 0, 0);
                    float invDetW = 1.0 / detW;

                    float2 dUdx_v = ddx(i.uv);
                    float2 dUdy_v = ddy(i.uv);

                    float2 uvPerWX = float2(
                        dUdx_v.x * dWdy_v.y - dUdy_v.x * dWdx_v.y,
                        dUdx_v.y * dWdy_v.y - dUdy_v.y * dWdx_v.y
                    ) * invDetW;
                    float2 uvPerWY = float2(
                        -dUdx_v.x * dWdy_v.x + dUdy_v.x * dWdx_v.x,
                        -dUdx_v.y * dWdy_v.x + dUdy_v.y * dWdx_v.x
                    ) * invDetW;

                    // Tile cell in world space (matches edge texture UV grid)
                    float2 cellMin = floor(i.worldPos / _EdgeScale) * _EdgeScale;
                    float2 cellMax = cellMin + _EdgeScale;

                    float2 checkPts[4] = {
                        cellMin,
                        float2(cellMax.x, cellMin.y),
                        float2(cellMin.x, cellMax.y),
                        cellMax
                    };

                    bool tileFits = true;
                    [unroll] for (int c = 0; c < 4; c++)
                    {
                        float2 woff = checkPts[c] - i.worldPos;
                        float2 checkUV = i.uv + uvPerWX * woff.x + uvPerWY * woff.y;

                        if (IsOutOfBoundsUnlit(checkUV))
                        {
                            tileFits = false;
                            break;
                        }
                    }

                    if (!tileFits)
                        return half4(0, 0, 0, 0);

                    float worldPixelSize = min(length(ddx(i.worldPos)), length(ddy(i.worldPos)));
                    float extendWorld = _EdgeExtendTiles * _EdgeScale;
                    int searchPixels = min((int)ceil(extendWorld / max(worldPixelSize, 1e-6)) + 2, 150);
                    float distPixels = GetDistToOpaqueUnlit(i.uv, uvStepUp, uvStepRight, searchPixels);
                    float distWorld = distPixels * worldPixelSize;
                    if (distWorld >= extendWorld)
                        return half4(0, 0, 0, 0);

                    mainColor = edgeSample;
                    mainColor.a *= edgeSample.a * i.color.a;
                }
                else
                {
                    // Inside the shape — fill is base, edge overlaid on top near boundary
                    int maxCheck = (int)(_EdgeWidth + _BlendWidth);
                    float edgeDist = GetEdgeDistanceUnlit(i.uv, uvStepUp, uvStepRight, maxCheck);

                    mainColor = fillSample;

                    if (edgeDist <= _EdgeWidth + _BlendWidth)
                    {
                        float edgeStrength = edgeSample.a;

                        if (edgeDist > _EdgeWidth && _BlendWidth > 0)
                        {
                            float t = (edgeDist - _EdgeWidth) / _BlendWidth;
                            edgeStrength *= (1.0 - t);
                        }

                        mainColor.rgb = lerp(fillSample.rgb, edgeSample.rgb, edgeStrength);
                    }

                    mainColor.a *= shapeCol.a * i.color.a;
                }

                return mainColor;
            }
            ENDHLSL
        }
    }
}
