Shader "Custom/DotPlotter"
{
	Properties {
		_MainTex("Texture", 2D) = "white" {}

	}
	
	SubShader {
		
		Tags{ "RenderType" = "Opaque" }
		ZWrite Off ZTest Always

		CGINCLUDE
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		float4 _MainTex_TexelSize;

		float3 _Mouse;
		fixed4 _RandColor;

		fixed4 frag(v2f_img i) : SV_Target {
			fixed4 col = tex2D(_MainTex, i.uv);

			float dist = distance(i.uv.xy, _Mouse.xy/256);

			if (dist < _MainTex_TexelSize.x) {
				col = _RandColor;
			}

			return col;
		}

		ENDCG

		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			ENDCG
		}
	}
}