Shader "Custom/TexCombineShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _BlendTex("BlendTexture", 2D) = "white" {}
        _Thresh("Threshold", Float) = 0.3
    }

    SubShader
    {
        Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        LOD 100

        ZWrite Off
                Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile MASK_ON MASK_OFF

#include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv_blend : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv_blend : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _BlendTex;
            float _Thresh;
            float4 _MaskRotation;
            uniform float4x4 _Rotation;
            float4 _MainTex_ST;
            float4 _BlendTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(mul(_Rotation, v.vertex));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
#ifdef MASK_ON
                o.uv_blend = TRANSFORM_TEX(v.uv_blend, _BlendTex);
                float2x2 rotmat = float2x2(_MaskRotation.xzyw);
                o.uv_blend = mul(o.uv_blend, rotmat);
#endif
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
#ifdef MASK_ON
                return tex2D(_MainTex, i.uv) * (tex2D(_BlendTex, i.uv_blend).a > _Thresh);
#else
                return tex2D(_MainTex, i.uv);
#endif
            }
            ENDCG
        }
    }
}