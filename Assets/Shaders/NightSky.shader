Shader "Custom/NightSky"
{
	Properties
	{
		_StarGradient("Star Gradient", 2D) = "white" {}
		_NumStars("Stars Count", Range(0, 1000)) = 500
		_Randomization("Randomize ", Range(0, 3.1416)) = 3.1416
	}
	SubShader
	{
		Tags { "RenderType"="Background" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows
		
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		#include "utils.cginc"

		// Get point on fibonacci sphere by id
			// Depends on rand (for randomization support)
		float3 fibonacci_point(int id, float num_points, float randomize)
		{
			float3 fp; // init point

			fp.y = 1 - (id / float(num_points - 1)) * 2;

			float radius = sqrt(1 - fp.y * fp.y); // radius at y

			float theta = G * id; // rotation
			theta += (hash11(id) - 0.5) * 2 * randomize; // randomize

			fp.x = cos(theta) * radius;
			fp.z = sin(theta) * radius;

			return fp;
		}
		
		// Returns id of closest point on fibonacci
		int fibonacci_sphere(float3 p, float num_points, float randomize)
		{
			float min_dist = 10; // bigger than the maximum
			int closest_point;

			for (int i = 0; i < num_points; i++)
			{
				float3 fp = fibonacci_point(i, num_points, randomize);

				float dist = find_angle(fp, p);
				if (dist < min_dist) {
					min_dist = dist;
					closest_point = i;
				}
			}

			return closest_point;
		}

		struct Input
		{
			float3 worldPos;
		};

		fixed4 _Color;
		float _NumStars;
		float _Randomization;
		sampler _StarGradient;

		void surf (Input IN, inout SurfaceOutputStandard OUT)
		{
			float3 pos = normalize(IN.worldPos);

			int id = fibonacci_sphere(pos, _NumStars, _Randomization);
			float d = find_angle(
				fibonacci_point(id, _NumStars, _Randomization),
				pos
			);

			float temperature = hash11(id);
			d = smoothstep(temperature * 0.0075 + 0.001, 0, d);
			float3 col = tex1D(_StarGradient, temperature);
						
			OUT.Emission = d * col;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
