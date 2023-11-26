#pragma once
#include <windows.h>
#include <d3d11_1.h>
#include <d3dcompiler.h>
#include <directxmath.h>
#include <directxcolors.h>
#include "resource.h"

using namespace DirectX;

struct MeshData
{
	ID3D11Buffer* VertexBuffer;
	ID3D11Buffer* IndexBuffer;
	UINT VBStride;
	UINT VBOffset;
	UINT IndexCount;
};

struct Vertex
{
	XMFLOAT3 Pos;
	XMFLOAT4 Color;
};

struct SimpleVertex
{
	XMFLOAT3 Pos;
	XMFLOAT3 Normal;
	XMFLOAT2 TexC;

	bool operator<(const SimpleVertex other) const
	{
		return memcmp((void*)this, (void*)&other, sizeof(SimpleVertex)) > 0;
	};
};

struct ConstantBuffer
{
	XMMATRIX mWorld;
	XMMATRIX mView;
	XMMATRIX mProjection;

	//diffuse lights
	XMFLOAT4 DiffLight;
	XMFLOAT4 DiffMat;
	XMFLOAT3 DirToLight;
	FLOAT pad; //pad for float3 above 
	//ambient lights
	XMFLOAT4 AmbLight;
	XMFLOAT4 AmbMat;
	//specular lights
	XMFLOAT4 SpecMat;
	XMFLOAT4 SpecLight;
	FLOAT SpecPower;
	XMFLOAT3 EyeWorldPos;//eye position
};