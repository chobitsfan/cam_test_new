Shader "Cubic Lens Distortion"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _K("K", Float) = -0.15
        _KCube("KCube", Float) = 0.5
        _CamTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth  
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert  
            #pragma fragment frag  

            #include "UnityCG.cginc"  

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _CamTex;
            fixed _K;
            fixed _KCube;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed2 r = i.uv - 0.5;
                fixed r2 = r.x * r.x + r.y * r.y;
                fixed f = 0;

                if (_KCube == 0)
                {
                    f = 1 + r2 * _K;
                }
                else
                {
                    f = 1 + r2 * (_K + _KCube * sqrt(r2));
                }

                fixed2 uv = f * (i.uv - 0.5) + 0.5;

                fixed4 col = tex2D(_MainTex, uv);
                return col * col.a + tex2D(_CamTex, i.uv) * (1 - col.a);
            }
            ENDCG
        }
    }
}