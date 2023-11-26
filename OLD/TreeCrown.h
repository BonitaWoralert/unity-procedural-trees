#pragma once

#include <GeometricPrimitive.h>

using namespace DirectX;

class TreeCrown
{
public:
	TreeCrown();
	~TreeCrown();

	void GenerateCrown();
	GeometricPrimitive::VertexCollection crownVertices;
	GeometricPrimitive::IndexCollection crownIndices;
};

