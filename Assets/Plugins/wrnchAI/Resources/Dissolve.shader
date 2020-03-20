/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

Shader "Custom/Dissolve"
{
	Properties{

		_MainTex("Texture", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpPower("Normal Power", float) = 1
		_NoiseTex("Dissolve Map", 2D) = "white" {}
		_EdgeColor("Dissolve Edge Color", Color) = (1,1,1,0)
		_Level("Dissolve Intensity", Range(0.0, 1.0)) = 0
		_EdgeRange("Edge Range", Range(0.0, 1.0)) = 0
		_EdgeMultiplier("Edge Multiplier", Float) = 1
	}

	SubShader{

		Tags{ "RenderType" = "Opaque" }
		Cull Off

		CGPROGRAM
		#pragma surface surf Lambert addshadow

		struct Input{

		float2 uv_MainTex;
		float2 uv_BumpMap;
		float2 uv_NoiseTex;
		float3 worldPos;
		float3 viewDir;
		float3 worldNormal;
		};

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _NoiseTex;

		uniform float _BumpPower;
		uniform float4 _EdgeColor;
		uniform float _EdgeRange;
		uniform float _Level;
		uniform float _EdgeMultiplier;

		void surf(Input IN, inout SurfaceOutput o){

			float4 dissolveColor = tex2D(_NoiseTex, IN.uv_NoiseTex);
			half dissolveClip = dissolveColor.r - _Level;
			half edgeRamp = max(0, _EdgeRange - dissolveClip);
			clip(dissolveClip);

			float4 texColor = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = lerp(texColor, _EdgeColor, min(1, edgeRamp * _EdgeMultiplier));
			if (edgeRamp > 2)
				o.Albedo * 2;

			fixed3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			normal.z /= _BumpPower;

			o.Normal = normalize(normal);

			return;
		}
		ENDCG
	}
	Fallback "Diffuse"
}
