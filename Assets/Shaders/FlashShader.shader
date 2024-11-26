Shader "Custom/FlashShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}   // The main sprite texture
        _Color ("Color Tint", Color) = (1,1,1,1)      // Color multiplier
        _FlashAmount ("Flash Amount", Range(0,1)) = 0 // Flash intensity
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha   // Enable alpha transparency blending

        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _FlashAmount;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Correctly transform the UV coordinates, even if flipped
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the sprite's texture
                fixed4 texColor = tex2D(_MainTex, i.texcoord);

                // Blend the sprite's color with white based on flash amount
                texColor.rgb = lerp(texColor.rgb, float3(1, 1, 1), _FlashAmount);

                // Apply alpha transparency and color multiplier
                texColor *= _Color;

                return texColor;
            }
            ENDCG
        }
    }
}
