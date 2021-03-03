Shader "Custom/Menu"
{
	Properties {
		_MainTex("Land-Water Mask", 2D) = "white" {}
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

			#include "UnityCG.cginc"
			#include "simplex.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float tex = tex2D(_MainTex, i.uv);
				float2 p = i.vertex / 10. + 10. * float2(_CosTime.x, _SinTime.x);
				fixed4 col = float4(1, 0, 0, 0) * tex * (snoise(p) + 0.75) / abs(_CosTime.y);
				return col / 5;
			}
			ENDCG
		}
	}
}
