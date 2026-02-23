Shader "Custom/Skybox_Stars"
{
    Properties
    {
        [Header(Sky)]
        _SkyColor ("Sky Color", Color) = (0.03, 0.05, 0.11, 1)

        [Header(Stars)]
        _StarBaseColor ("Star Base Color", Color) = (0.8, 1.0, 0.3, 1)
        _StarHueOffset ("Star Hue Offset", Range(0.0, 1.0)) = 0.6
        _StarIntensity ("Star Intensity", Range(0.0, 0.2)) = 0.08

        [Header(Twinkle)]
        [Toggle(_USE_TWINKLE)] _UseTwinkle ("Use Twinkle", Float) = 1
        _StarTwinkleSpeed ("Star Twinkle Speed", Range(0.0, 2.0)) = 0.8
        _StarTwinkleIntensity ("Star Twinkle Intensity", Range(0.0, 1.0)) = 0.2

        [Header(Layers)]
        _LayerScale ("Layer Scale", Range(0.0, 60.0)) = 20.0
        _LayerScaleStep ("Layer Scale Step", Range(0.0, 40.0)) = 10.0
        _LayersCount ("Layers Count", Range(0, 12)) = 3
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _USE_TWINKLE

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            // Uniforms
            float3 _SkyColor;
            float3 _StarBaseColor;
            float _StarHueOffset;
            float _StarIntensity;
            float _StarTwinkleSpeed;
            float _StarTwinkleIntensity;
            float _LayerScale;
            float _LayerScaleStep;
            int _LayersCount;

            #define PI 3.14159265359
            #define TAU 6.28318530718

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                // The texcoord in a skybox is the 3D local position of the vertex,
                // which acts as the view direction vector.
                o.texcoord = v.texcoord; 
                return o;
            }

            // Hue shift function mapped to HLSL
            float3 hue(float3 inputColor, float offset, int range_index) 
            {
                float4 k = float4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
                float4 p = lerp(float4(inputColor.bg, k.wz), float4(inputColor.gb, k.xy), step(inputColor.b, inputColor.g));
                float4 q = lerp(float4(p.xyw, inputColor.r), float4(inputColor.r, p.yzx), step(p.x, inputColor.r));
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                float3 hsv = float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);

                offset = (range_index == 0) ? offset / 360.0 : offset;
                float h = hsv.x + offset;
                
                if (h < 0.0) {
                    hsv.x = h + 1.0;
                } else if (h > 1.0) {
                    hsv.x = h - 1.0;
                } else {
                    hsv.x = h;
                }

                float4 k2 = float4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
                float3 p2 = abs(frac(hsv.xxx + k2.xyz) * 6.0 - k2.www);
                float3 rgb = hsv.z * lerp(k2.xxx, clamp(p2 - k2.xxx, 0.0, 1.0), hsv.y);
                return rgb;
            }

            // Hash mapped to HLSL
            float3 hash(float3 x) 
            {
                x = float3(dot(x, float3(127.1, 311.7, 74.7)),
                           dot(x, float3(269.5, 183.3, 246.1)),
                           dot(x, float3(113.5, 271.9, 124.6)));
                return frac(sin(x) * 43758.5453123);
            }

            // Voronoi mapped to HLSL
            float2 voronoi(float3 x)
            {
                float3 p = floor(x);
                float3 f = frac(x);
                
                float res = 100.0;
                float id = 0.0;
                
                for (float k = -1.0; k <= 1.0; k += 1.0) {
                    for (float j = -1.0; j <= 1.0; j += 1.0) {
                        for (float i = -1.0; i <= 1.0; i += 1.0) {
                            float3 b = float3(i, j, k);
                            float3 r = b - f + hash(p + b);
                            float d = dot(r, r);
                            if (d < res) {
                                res = d;
                                id = dot(p + b, float3(0.0, 57.0, 113.0));
                            }
                        }
                    }
                }
                
                return float2(sqrt(res), id);
            }

            // Main fragment function (Replaces sky() in Godot)
            float4 frag (v2f i) : SV_Target
            {
                // EYEDIR equivalent in Unity
                float3 rayDir = normalize(i.texcoord); 
                float3 col = _SkyColor;
                
                for (int j = 0; j < _LayersCount; j++) 
                {
                    float3 pos = rayDir * (_LayerScale + float(j) * _LayerScaleStep);
                    float2 layer = voronoi(pos);
                    
                    // HLSL doesn't auto-cast float to float3 like vec3(float) in GLSL.
                    float3 rand = hash(float3(layer.y, layer.y, layer.y));
                    
                    float star = 0.0;
                    
                    #if _USE_TWINKLE
                        // TIME maps to _Time.y
                        float twinkle = sin(_Time.y * PI * _StarTwinkleSpeed + rand.x * TAU);
                        twinkle *= _StarTwinkleIntensity;
                        star = smoothstep(_StarIntensity + _StarIntensity * twinkle, 0.0, layer.x);
                    #else
                        star = smoothstep(_StarIntensity, 0.0, layer.x);
                    #endif
                    
                    float3 star_color = star * hue((col + _StarBaseColor), rand.y * _StarHueOffset, 1);
                    
                    col += star_color;
                }
                
                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}