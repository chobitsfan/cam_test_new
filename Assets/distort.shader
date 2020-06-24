Shader "opencv distort"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _CamTex("Texture", 2D) = "white" {}
        _CX("CX", Float) = 315.46
        _CY("CY", Float) = 240.96
        _FX("FX", Float) = 246.88
        _FY("FY", Float) = 249.75
        _K1("K1", Float) = 0.21874
        _K2("K2", Float) = -0.24239
        _P1("P1", Float) = -0.00089613
        _P2("P2", Float) = 0.00064407
        _K3("K3", Float) = 0.063342
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _CamTex;
            float _FX;
            float _FY;
            float _CX;
            float _CY;
            float _K1;
            float _K2;
            float _K3;
            float _P1;
            float _P2;

            fixed4 frag(v2f i) : SV_Target
            {
                //fixed4 col = tex2D(_MainTex, i.uv);
                // just invert the colors
                //col.rgb = 1 - col.rgb;
                //return col;
                float2 r = i.uv;
                //float x = (r.x - _CX) / _FX;
                //float y = (r.y - _CY) / _FY;
                float x = r.x;
                float y = r.y;
                float r2 = x * x + y * y;
                float distort = 1 + _K1 * r2 + _K2 * r2 * r2 + _K3 * r2 * r2 * r2;
                float x_distort = x * distort;
                float y_distort = y * distort;
                x_distort = x_distort + (2 * _P1 * x * y + _P2 * (r2 + 2 * x * x));
                y_distort = y_distort + (_P1 * (r2 + 2 * y * y) + 2 * _P2 * x * y);
                //x_distort = x_distort * _FX + _CX;
                //y_distort = y_distort * _FY + _CY;
                fixed4 col = tex2D(_MainTex, float2(x_distort, y_distort));
                //return tex2D(_MainTex, float2(1-r.x, 1-r.y));
                //return fixed4(0.5, r.y, 0, 1);
                return col * col.a + tex2D(_CamTex, i.uv) * (1 - col.a);
            }
            ENDCG
        }
    }
}
