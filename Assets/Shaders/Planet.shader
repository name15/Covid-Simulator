Shader "Custom/Planet"
{
	Properties
	{
		// TODO: merge the two heightmaps
		_WaterMask("Water Mask", 2D) = "white" {}
		_HeightMap("Heightmap", 2D) = "white" {}
		_Bathymetry("Bathymetry", 2D) = "white" {}
		
		_ElevationStrength("Terrain height", Range(0, 10)) = 0.25
		_WavesStrength("Waves height", Range(0, 10)) = 0.25

		_WaterGradient("Water gradient", 2D) = "white" {}
		_WavesNormalMap("Waves normal map", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 300

		CGPROGRAM
		// Physically based Standard lighting model, shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		#pragma target 3.0

		#include "utils.cginc"


		struct Input
		{
			float3 worldPos;
		};

		struct country {
			float2 coords; // In spherical coordinates
		};

		sampler2D _WaterMask;

		sampler2D _HeightMap;
		float _ElevationStrength;
		float _WavesStrength;
		
		sampler2D _Bathymetry;
		
		sampler _WaterGradient;
		sampler2D _WavesNormalMap;
		
		float getHeight(float2 uv) {
			return tex2D(_HeightMap, uv);
		}

		float4 bumpFromDepth(float2 uv, float2 resolution, float scale) {
			float2 step = 1. / resolution;

			float height = getHeight(uv);

			float2 dxy = height - float2(
				getHeight(uv + float2(step.x, 0.)),
				getHeight(uv + float2(0., step.y))
			);

			return float4(normalize(float3(dxy * scale / step, 1.)), height);
		}

		void surf (Input IN, inout SurfaceOutputStandard OUT)
		{
			float3 localPos = normalize(IN.worldPos- mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz);
			float2 sphericalPos = cartesian2spherical(localPos);
			


			// Height effect
			float4 elevNormal = bumpFromDepth(sphericalPos, float2(2048, 1024), 0.05); // WARNING: manually set texture resolution
			elevNormal = lerp(float4(0, 0, 0, 0), elevNormal, _ElevationStrength);
			// Will be applied only where there is no water

			float3 wavesNormal = UnpackNormal(tex2D(_WavesNormalMap, frac((sphericalPos + float2(_CosTime.x, _SinTime.x) / 100) * 50))); // 100 - speed; 50 - amount of wrapping around the sphere
			wavesNormal = lerp(float4(0, 0, 0, 0), wavesNormal, _WavesStrength);
			// Will be applied only where there is water


			// Render Ocean
			float water = tex2D(_WaterMask, sphericalPos);
			
			float water0 = tex2D(_WaterMask, sphericalPos + float2(1. / 2048, 0)); // WARNING: manually set texture resolution
			float water1 = tex2D(_WaterMask, sphericalPos - float2(0, 1. / 1024)); // WARNING: manually set texture resolution
			float water2 = tex2D(_WaterMask, sphericalPos - float2(-1. / 2048, 0)); // WARNING: manually set texture resolution
			float water3 = tex2D(_WaterMask, sphericalPos - float2(0, -1. / 1024)); // WARNING: manually set texture resolution

			float water_min = min(min(water0, water1), min(water2, water3));
			float water_max = max(max(water0, water1), max(water2, water3));

			bool border = false; // Whether there is border

			if (water > 0.5 || water_max > 0.25) {
				if (water_min > 0.5) {
					OUT.Smoothness = 0.6;
					OUT.Normal += wavesNormal; // Apply water normal
					OUT.Albedo = tex2D(_WaterGradient, tex2D(_Bathymetry, sphericalPos) );
					return;
				}

				else border = true;
			}

			OUT.Normal += elevNormal; // Apply terrain normal

			float3 col = localPos;

			if (border) OUT.Albedo = col / 2;
			else OUT.Albedo = col;
			
		}
		ENDCG
	}
	FallBack "Diffuse"
}
