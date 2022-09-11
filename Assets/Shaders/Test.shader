Shader "Custom/Test"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Color", Color) = (0, 0, 0, 0)
        _CheckerTex ("Offset Texture", 2D) = "White" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Back

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "HLSLSupport.cginc"
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _BaseColor;
            sampler2D _CheckerTex;
            float4 _CheckerTex_ST;
            
            struct geomData
            {
                float4 outputPos : SV_POSITION;
                //float4 vertex : POSITION;
                //float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                float2 xyOff = TRANSFORM_TEX(v.uv, _CheckerTex);
                fixed4 cOff = tex2Dlod(_CheckerTex, float4(v.uv.x, v.uv.y, 0, 0));
                float totOff = mul(mul(cOff.x, 5) - 0.5, 2);
                //float4 vOff = float4(totOff, totOff, totOff, 0);
                float3 m = mul(v.normal, totOff);
                //o.vertex = UnityObjectToClipPos(v.vertex) + (UnityObjectToClipPos(m));
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2f input[3], inout TriangleStream<geomData> triStream)
            {
                geomData vert0;
                geomData vert1;
                geomData vert2;

                vert0.outputPos = input[0].vertex;
                vert1.outputPos = input[1].vertex;
                vert2.outputPos = input[2].vertex;

                //vert0.normal = input[0].normal;
                //vert1.normal = input[1].normal;
                //vert2.normal = input[2].normal;

                vert0.uv = input[0].uv;
                vert1.uv = input[1].uv;
                vert2.uv = input[2].uv;


                //vert0.outputPos = UnityObjectToClipPos(input[0].vertex);
                //vert1.outputPos = UnityObjectToClipPos(input[1].vertex);
                //vert2.outputPos = UnityObjectToClipPos(input[2].vertex);


                triStream.Append(vert0);
                triStream.Append(vert1);
                triStream.Append(vert2);
                triStream.RestartStrip();
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //fixed4 col = _BaseColor;
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) + _BaseColor;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDHLSL
        }
    }
}
