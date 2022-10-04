Shader "Unlit/New Outline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineSize ("Outline Size", float) = 0.0075
        _OutlineColour ("Outline Colour", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _OutlineColour;
            float _OutlineSize;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                // UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // offset alphas for outline
                float2 right = float2(_OutlineSize, 0);
                float2 up = float2(0, -_OutlineSize);

                fixed outlineAlpha = tex2D(_MainTex, i.uv + right).a + tex2D(_MainTex, i.uv - right).a + tex2D(_MainTex, i.uv + up).a + tex2D(_MainTex, i.uv - up).a;
                outlineAlpha = clamp(outlineAlpha, 0, 1);

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // mask outline alpha
                outlineAlpha *= 1 - col.a;
                // mask base colour
                col *= 1 - outlineAlpha;
                // combine
                col += outlineAlpha * _OutlineColour;

                return col * i.color;
            }
            ENDCG
        }
    }
}
