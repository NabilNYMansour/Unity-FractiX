//---------------Includes---------------//
#include "utils.hlsl"

//---------------Constructions---------------//
///////////////////////////////////////////////////////////////
 // First fractal for Fractal Glide (Scene = 0)
float apollonianHillsSDF(float3 pos) {
	pos = -max((Mod(pos + 5000, 20000) - 10000), -(Mod(pos + 5000, 20000) - 10000)) + 5000 + float3(2000, 350, 2000); // repeat space
	float4 p = float4(pos, 1.);
	for (int i = 0; i < 11; ++i)
	{
		boxFold(p, float3(1, 1.5, .5));
		p *= sphereFold(p, 0., 2, 0.02);
	}

	return p.y * 0.2 / p.w;
}
float3 apollonianHillsAlbedo(float3 pos) {
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
///////////////////////////////////////////////////////////////
// Shadows demo SDF and Albedo (Scene = 1)
float shadowDemoSDF(float3 pos) {
	float dBox = de_box3(pos - float3(0, 2, 0), 1);
	dBox = min(dBox, de_box3(pos - float3(0, 6, 0), 0.2));
	return min(dBox, pos.y);
}
float3 shadowDemoAlbedo(float3 pos) {
	float dBox = de_box3(pos - float3(0, 2, 0), 1);
	if (dBox < pos.y) {
		if (dBox < de_box3(pos - float3(0, 6, 0), 0.2)) return float3(0.2, 0.5, 0.8);
		else return float3(0.6, 0.1, 0.8);

	}
	else {
		float chessboard = frac((floor(pos.x) + floor(pos.z)) * 0.5);
		float3 col = chessboard > 0 ? float3(0.8, 0.5, 0.4) : float3(0.2, 0.8, 0.6);
		return col;
	}
}
///////////////////////////////////////////////////////////////
//---------------Main SDF definitions---------------//
////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// NOTE: This is where the fractal is defined. Change GetDis and GetAlbedo in order to change the fractal.
///		  See above in the "Constructions" section for examples on how fractals are made.
///		  Also, use _scene to change between the scenes you would want to make.
////////////////////////////////////////////////////////////////////////////////////////////////////////////
float GetDis(float3 pos) {
	switch (_scene) {
	case 0:
		return apollonianHillsSDF(pos);
	default:
		return shadowDemoSDF(pos);
	}
}
float3 GetAlbedo(float3 pos) {
	switch (_scene) {
	case 0:
		return apollonianHillsAlbedo(pos);
	default:
		return shadowDemoAlbedo(pos);
	}

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