Shader "Custom/RSCVertexColorFlat"
{
    Properties {}
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos    : SV_POSITION;
                float4 color  : COLOR;
                float3 normal : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // one hard directional light fake — gives faceted shading
                // without needing real scene lighting
                float3 lightDir = normalize(float3(0.4, 1.0, 0.3));
                float ndotl = saturate(dot(normalize(i.normal), lightDir));
                float shade = 0.5 + 0.5 * ndotl; // keep it from going fully black
                return fixed4(i.color.rgb * shade, 1);
            }
            ENDCG
        }
    }
}