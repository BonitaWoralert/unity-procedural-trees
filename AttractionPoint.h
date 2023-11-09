#pragma once
#include <directxmath.h>

using namespace DirectX;

class AttractionPoint
{
private:
	XMFLOAT3 position;
public:
	AttractionPoint(XMFLOAT3 position) : position(position) {}
	~AttractionPoint();
	void FindNearestBranch();
};

