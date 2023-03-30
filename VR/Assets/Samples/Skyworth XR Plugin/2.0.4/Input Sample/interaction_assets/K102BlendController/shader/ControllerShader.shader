Shader "Unlit/ControllerShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            uniform float _Battery;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                if (i.uv.y < 0.1431)
                {
                 
                    if(_Battery>0.2 &&_Battery< 0.4 &&  i.uv.x < 0.2)
                    col = fixed4(0, 1, 0, 1);
                    else if(_Battery>=0.4&&_Battery<0.6 &&  i.uv.x < 0.4)
                    col = fixed4(0, 1, 0, 1);
                    else if(_Battery >=0.6 && _Battery <0.8 && i.uv.x < 0.6)
                    col = fixed4(0, 1, 0, 1);
                    else if(_Battery >=0.8)
                    col = fixed4(0, 1, 0, 1);
                    
                }
                return col;
            }
            ENDCG
        }
    }
}
