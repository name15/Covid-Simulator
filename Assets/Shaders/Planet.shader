Shader "Custom/Planet"
{
	Properties
	{
		[Header(Physical map data)] 
		_LandMask("Land-Water Mask", 2D) = "white" {}
		
		_Topography("Topography", 2D) = "white" {}
		_Bathymetry("Bathymetry", 2D) = "white" {}

		_ElevationMagnitude("Terrain magnitude", Range(0, 10)) = 0.25
		_WavesMagnitude("Waves magnitude", Range(0, 10)) = 0.25

		_WaterGradient("Water gradient", 2D) = "white" {}
		_WavesNormalMap("Waves normal map", 2D) = "white" {}

		[Space(25)] [Header(Political map data)]
		_PoliticalMap("Political map", 2D) = "white" {}
		_BorderMask("Country borders", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 500

		CGPROGRAM	
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		#include "utils.cginc" //Custom utils functions
		#include "simplex.cginc"
		
		#define country_count 177. // WARNING: Must be set manually


		struct Input
		{
			float3 worldPos;
		};
		
		// Physical map data	
		sampler2D _LandMask;

		sampler2D _Topography;
		sampler2D _Bathymetry;

		float _ElevationMagnitude;
		float _WavesMagnitude;

		sampler _WaterGradient;
		sampler2D _WavesNormalMap;

		float getHeight(float2 uv) {
			return tex2D(_Topography, uv);
		}

		// Political map data
		sampler2D _PoliticalMap;
		sampler2D _BorderMask;

		// Country buffer data
		uniform float _InfectionStatus[country_count];	//WARNING: Must be manually set from script

		// Converts hiught to narmal map
			// Depends on getHeight()
		float4 Height2NormalMap(float2 uv, float2 resolution, float scale) {
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
			
			// Border Mask
			bool border = tex2D(_BorderMask, sphericalPos) > 0.1;
			if (border) {
				// >>> Render borders <<<
				OUT.Albedo = 0.15;
				
				// TODO: highlight countries (with gradient tex)
				
				return;
			}

			// Water-Land mask
			float land = tex2D(_LandMask, sphericalPos);
			if (land < 0.5) {
				// >>> Render Ocean <<<
				OUT.Smoothness = 0.5;
				OUT.Albedo = tex2D(_WaterGradient, tex2D(_Bathymetry, sphericalPos));
				
				// > Compute (moving) waves normals <
				float3 wavesNormal = UnpackNormal(
					tex2D(
						_WavesNormalMap,
						frac((sphericalPos + float2(_CosTime.x, _SinTime.x) / 100) * 10 * float2(5, 3)) // Make waves move over time
					)
				);
				wavesNormal = lerp(float4(0, 0, 0, 0), wavesNormal, _WavesMagnitude); // Set waves magnitude
				OUT.Normal += wavesNormal; // Set waves normal

				return;
			}


			// >>> Render Terrain <<<
			// > Compute terrain normals <
			float4 elevNormal = Height2NormalMap(sphericalPos, float2(2048, 1024), 0.05); // WARNING: manually set texture resolution
			elevNormal = lerp(float4(0, 0, 0, 0), elevNormal, _ElevationMagnitude); // Set elevation magnitude
			
			OUT.Normal += elevNormal; // Set terrain normal


			// >>> Render Political map <<<
			float countryId = round(tex2D(_PoliticalMap, sphericalPos) * 177.);
			OUT.Albedo = float2hue(tex2D(_PoliticalMap, sphericalPos)) / 1.5;

			// >>> Covid map <<<
			float covid = snoise(localPos * 50. + _Time.y) + 0.5;
			OUT.Albedo.gb -= _InfectionStatus[countryId];
			OUT.Albedo.r += covid * _InfectionStatus[countryId];
		}
		ENDCG
	}
	FallBack "Diffuse"
}
