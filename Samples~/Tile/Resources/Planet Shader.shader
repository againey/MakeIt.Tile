Shader "Custom/Planet Shader" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_OverlayTex ("Overlay (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _OverlayTex;

		struct Input {
			float2 uv_MainTex;
			float2 uv2_OverlayTex;
		};

		half _Glossiness;
		half _Metallic;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			float4 surface = tex2D (_MainTex, IN.uv_MainTex);
			float4 overlay = tex2D (_OverlayTex, IN.uv2_OverlayTex);
			o.Albedo = lerp (surface.rgb, overlay.rgb, overlay.a);
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = surface.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
