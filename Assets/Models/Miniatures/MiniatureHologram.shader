Shader "Custom/MiniatureHologram" {
    Properties {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Holo Color", Color) = (1, 1, 1, 1)
        _FColor("Fresnel Color", Color) = (1, 1, 1, 1)
        _Scale("Alpha Tiling", Range(0, 5.0)) = 1
        _ScrollSpeed("Alpha Speed", Range(0.01, 2.0)) = 0.1
        _FresnelInt("Fresnel Intensity", Range(0, 10)) = 0.5
        _FresnelPow("Fresnel Power", Range(1, 5)) = 1
    }

    SubShader {
        Tags {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Blend SrcAlpha One
        ZWrite On
        Cull Off

        Pass {
            HLSLPROGRAM#include "HLSLSupport.cginc"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _MainTex_ST, _Color, _FColor;
            half _Scale, _ScrollSpeed, _FresnelInt, _FresnelPow;

            struct appdata {
                float4 vertex: POSITION;
                float2 uv: TEXCOORD0;
                float3 normal: NORMAL;
            };

            struct v2f {
                float2 uv: TEXCOORD0;
                float4 vertex: SV_POSITION;
                float3 alphaPos: TEXCOORD1;
                float3 normal: TEXCOORD2;
                float3 vertex_W: TEXCOORD3;

            };

            v2f vert(appdata v) {
                v2f o;

                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                //Alpha mask coord
				o.alphaPos = TransformObjectToWorld(v.vertex.xyz);

                //Scroll Alpha
                o.alphaPos.y += _Time.y * _ScrollSpeed;

                //Vertex World and Normal
                o.normal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0))).xyz;
                o.vertex_W = mul(unity_ObjectToWorld, v.vertex);

                return o;
            }

            float FresnelCalculator(float3 normal, float3 viewDir, float fresnelPow) {
                return 1 - max(0, dot(normal, viewDir));
            }

            float4 frag(v2f i): SV_Target {

                float4 color = tex2D(_MainTex, i.uv);
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.vertex_W);

                //FRESNEL
                float fresnel = FresnelCalculator(i.normal, viewDir, _FresnelPow);
                fresnel = pow(fresnel, _FresnelPow) * _FresnelInt;
                float3 fresnelColor = fresnel * _FColor;

                color.rgb += fresnelColor;

                // THIN scanlines
                float thinLines = sin(i.alphaPos.y * 140 - _Time.y * _ScrollSpeed * 6);
                thinLines = thinLines * 0.5 + 0.5;

                // BIG sweeping bands
                float bigBands =
                    sin(i.alphaPos.y * 18 - _Time.y * _ScrollSpeed * 2);

                bigBands = bigBands * 0.5 + 0.5;

                // Flicker
                float flicker = sin(_Time.y * 40) * 0.04 + 0.96;

                // Combine
                float scan = thinLines * 0.7 + bigBands * 0.3;
                scan *= flicker;

                // Sharpen a little
                scan = pow(scan, 1.5);

                // color.a = scan;
                color.a = scan * 0.6 + 0.2;

                float4 finalColor = _Color * color;

                return finalColor;

            }
            ENDHLSL
        }
    }
}