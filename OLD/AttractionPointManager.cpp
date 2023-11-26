#include "AttractionPointManager.h"

AttractionPointManager::AttractionPointManager()
{
}

AttractionPointManager::~AttractionPointManager()
{
}

void AttractionPointManager::AddPoint(XMFLOAT4X4 matrix, XMFLOAT3 position)
{
	Point p;
	p._matrix = matrix;
	p._position = position;
	_attractionPoints.push_back(p);
}