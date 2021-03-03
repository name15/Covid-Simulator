#ifndef __CUSTOM_UTILS
#define __CUSTOM_UTILS

//TODO
#define PI	3.1416
#define G 2.4 // golden angle in radians

/* Random Noise */

// TODO Credit: David Hoskins; from: https://www.shadertoy.com/view/4djSRW
float hash11(float p)
{
    p = frac(p * .1031);
    p *= p + 33.33;
    p *= p + p;
    return frac(p);
}

float3 hash33(float3 p3)
{
    p3 = frac(p3 * float3(.1031, .1030, .0973));
    p3 += dot(p3, p3.yxz + 19.19);
    return frac((p3.xxy + p3.yxx) * p3.zyx);

}

// hash11 based 3d value noise
// function taken from https://www.shadertoy.com/view/XslGRr
// Created by inigo quilez - iq/2013
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.

// ported from GLSL to HLSL

float noise(float3 x)
{
    // The noise function returns a value in the range -1.0f -> 1.0f

    float3 p = floor(x);
    float3 f = frac(x);

    f = f * f * (3.0 - 2.0 * f);
    float n = p.x + p.y * 57.0 + 113.0 * p.z;

    return lerp(lerp(lerp(hash11(n + 0.0), hash11(n + 1.0), f.x),
                   lerp(hash11(n + 57.0), hash11(n + 58.0), f.x), f.y),
               lerp(lerp(hash11(n + 113.0), hash11(n + 114.0), f.x),
                   lerp(hash11(n + 170.0), hash11(n + 171.0), f.x), f.y), f.z);
}

/* Colors */

// Returns rgb color with given hue
float3 float2hue(float c)
{
    return float3(
		smoothstep(1. / 6., 0, min(c - 1. / 6., 5. / 6. - c)),
		smoothstep(1. / 6., 0, abs(c - 1. / 3.) - 1. / 6.),
		smoothstep(1. / 6., 0, abs(c - 2. / 3.) - 1. / 6.)
	);
}

/* 3d Math */
float find_angle(float3 a, float3 b)
{
    return acos((a.x * b.x + a.y * b.y + a.z * b.z) / (length(a) * length(b)));
}

float2 cartesian2spherical(float3 c) {
	float r = length(c);
	float2 s = float2(
		atan2(c.z, c.x),
		asin(c.y / r)
	);
    s += float2(PI, PI / 2);
    s /= float2(PI * 2, PI);
	
    return s;
}

float3 spherical2cartesian(float2 s) {
    s *= float2(PI * 2, PI);
    s -= float2(PI, PI / 2);
	
	return float3(
		cos(s.y) * cos(s.x),
		sin(s.y),
		cos(s.y) * sin(s.x)
	);
}
#endif // __CUSTOM_UTILS