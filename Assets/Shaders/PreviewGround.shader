Shader "GameContentEditor/PreviewGround"
{
    Properties
    {
        _Ground ("Ground", Color) = (0.11, 0.12, 0.15, 1)
        _Minor ("Minor", Color) = (0.28, 0.30, 0.34, 1)
        _Major ("Major", Color) = (0.72, 0.68, 0.58, 1)
        _Axis ("Axis", Color) = (0.91, 0.72, 0.28, 1)
        _Label ("Label", Color) = (0.93, 0.90, 0.84, 1)
        _LabelDim ("Label Dim", Color) = (0.62, 0.60, 0.55, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            float4 _Ground;
            float4 _Minor;
            float4 _Major;
            float4 _Axis;
            float4 _Label;
            float4 _LabelDim;

            static const int Glyphs[84] = {
                14, 17, 17, 17, 17, 17, 14,
                4, 12, 4, 4, 4, 4, 14,
                14, 17, 1, 2, 4, 8, 31,
                14, 17, 1, 6, 1, 17, 14,
                2, 6, 10, 18, 31, 2, 2,
                31, 16, 30, 1, 1, 17, 14,
                6, 8, 16, 30, 17, 17, 14,
                31, 1, 2, 4, 8, 8, 8,
                14, 17, 17, 14, 17, 17, 14,
                14, 17, 17, 15, 1, 2, 12,
                0, 0, 26, 21, 21, 21, 21,
                0, 0, 0, 31, 0, 0, 0
            };

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 world : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float LineAt(float coord, float width)
            {
                float along = frac(coord);
                float dist = min(along, 1.0 - along);
                float aa = max(fwidth(coord), 1e-4);
                return 1.0 - smoothstep(width, width + aa, dist);
            }

            float Glyph(int id, float2 uv)
            {
                int col = (int)floor(uv.x * 5.0);
                int row = (int)floor((1.0 - uv.y) * 7.0);
                if (col < 0 || col > 4 || row < 0 || row > 6 || id < 0 || id > 11)
                    return 0;
                int bits = Glyphs[id * 7 + row];
                return (bits >> (4 - col)) & 1;
            }

            void Metrics(int n, out int chars, out float height, out float width)
            {
                int mag = abs(n);
                int sign = n < 0 ? 1 : 0;
                int digits = mag >= 10 ? 2 : 1;
                chars = digits + 1 + sign;
                height = (abs(n) % 5) == 0 ? 0.46 : 0.32;
                width = height * (5.0 / 7.0);
            }

            float TextAt(float2 xz, float2 bottomLeft, int n)
            {
                int mag = abs(n);
                int sign = n < 0 ? 1 : 0;
                int digits = mag >= 10 ? 2 : 1;
                int chars;
                float height;
                float width;
                Metrics(n, chars, height, width);
                float2 local = xz - bottomLeft;
                if (local.x < 0 || local.y < 0 || local.x >= width * chars || local.y >= height)
                    return 0;
                int index = (int)floor(local.x / width);
                float2 uv = float2(frac(local.x / width), local.y / height);
                int glyph = 10;
                if (sign == 1 && index == 0)
                    glyph = 11;
                else if (index < chars - 1)
                {
                    int digitIndex = index - sign;
                    int scale = digits == 1 || digitIndex == 1 ? 1 : 10;
                    glyph = (mag / scale) - (mag / (scale * 10)) * 10;
                }
                return Glyph(glyph, uv);
            }

            float LabelZ(float2 xz)
            {
                int n = (int)round(xz.y);
                if (n == 0)
                    return 0;
                int chars;
                float height;
                float width;
                Metrics(n, chars, height, width);
                return TextAt(xz, float2(0.62, n - height * 0.5), n);
            }

            float LabelX(float2 xz, out int meter)
            {
                int center = (int)round(xz.x);
                float mask = 0;
                float nearest = 1e9;
                meter = 0;
                [unroll]
                for (int i = -1; i <= 1; i++)
                {
                    int n = center + i;
                    if (n == 0 || n == 1)
                        continue;
                    int chars;
                    float height;
                    float width;
                    Metrics(n, chars, height, width);
                    float span = width * chars;
                    float hit = TextAt(xz, float2(n - span * 0.5, -0.62 - height), n);
                    float dist = abs(xz.x - n);
                    if (hit > 0.5 && dist < nearest)
                    {
                        nearest = dist;
                        mask = hit;
                        meter = n;
                    }
                }
                return mask;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 world = i.world;
                float minor = max(LineAt(world.x, 0.012), LineAt(world.z, 0.012));
                float major = max(LineAt(world.x / 5.0, 0.006), LineAt(world.z / 5.0, 0.006));
                float axisX = 1.0 - smoothstep(0.04, 0.04 + max(fwidth(world.x), 1e-4), abs(world.x));
                float axisZ = 1.0 - smoothstep(0.04, 0.04 + max(fwidth(world.z), 1e-4), abs(world.z));
                float disc = length(world.xz);
                float discAa = max(fwidth(disc), 1e-4);
                float origin = 1.0 - smoothstep(0.28, 0.28 + discAa, disc);

                float3 color = _Ground.rgb;
                color = lerp(color, _Minor.rgb, saturate(minor));
                color = lerp(color, _Major.rgb, saturate(major));
                color = lerp(color, _Axis.rgb, saturate(max(max(axisX, axisZ), origin)));
                float labelZ = LabelZ(world.xz);
                int meterZ = (int)round(world.z);
                float3 inkZ = (abs(meterZ) % 5) == 0 ? _Label.rgb : _LabelDim.rgb;
                color = lerp(color, inkZ, labelZ);
                int meterX;
                float labelX = LabelX(world.xz, meterX);
                float3 inkX = (abs(meterX) % 5) == 0 ? _Label.rgb : _LabelDim.rgb;
                color = lerp(color, inkX, labelX);
                return float4(color, 1);
            }
            ENDHLSL
        }
    }
}
