//---------------Includes---------------//
#include "utils.hlsl"

//---------------Constructions---------------//
float apollonianHillsSDF(float3 pos) { // First fractal for Fractal Glide.
	pos = -max((Mod(pos + 5000, 20000) - 10000), -(Mod(pos + 5000, 20000) - 10000)) + 5000 + float3(2000, 350, 2000); // repeat space
	float4 p = float4(pos, 1.);
	for (int i = 0; i < 11; ++i)
	{
		boxFold(p, float3(1, 1.5, .5));
		p *= sphereFold(p, 0., 2, 0.02);
	}

	return p.y * 0.2 / p.w;
}
float3 apollonianHillsAlbedo(float3 pos) { // First fractal for Fractal Glide.
	pos = -max((Mod(pos + 5000, 20000) - 10000), -(Mod(pos + 5000, 20000) - 10000)) + 5000 + float3(2000, 350, 2000); // repeat space
	float4 p = float4(pos, 1.);
	float3 col = 1e20;
	for (int i = 0; i < 11; ++i)
	{
		boxFold(p, float3(1, 1.5, .5));
		p *= sphereFold(p, 0., 2, 0.02);
		col = min(col, abs(fmod(abs(p.xyz), float3(1000, 1000, 0.001))));
	}

	return col;
}
//---------------Main SDF definitions---------------//
////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// NOTE: This is where the fractal is defined. Change GetDis and GetAlbedo in order to change the fractal.
///		  See above in the "Constructions" section for examples on how fractals are made.
////////////////////////////////////////////////////////////////////////////////////////////////////////////
float GetDis(float3 pos) {
	return apollonianHillsSDF(pos);
}
float3 GetAlbedo(float3 pos) {
	return apollonianHillsAlbedo(pos);
}
//---------------Get normal of SDF---------------//
float3 GetNormal(float3 pos, float3 SLOPE_EPS) // from https://iquilezles.org/articles/normalsSDF/
{
	float3 n = float3(0, 0, 0);
	float3 e;
	for (int i = 0; i < 4; i++)
	{
		e = 0.5773 * (2.0 * float3((((i + 3) >> 1) & 1), ((i >> 1) & 1), (i & 1)) - 1.0);
		n += e * GetDis(pos + e * SLOPE_EPS);
	}
	return normalize(n);
}